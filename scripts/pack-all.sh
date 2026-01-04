#!/usr/bin/env bash
set -euo pipefail

base_dir="src"
divider="----------------------------------------"

# Usage:
#   ./scripts/pack-all.sh                 # package changed libs only (auto-detect)
#   ./scripts/pack-all.sh --force         # package ALL libs
#   ./scripts/pack-all.sh --only Paralax.WebApi Paralax.HTTP
#   ./scripts/pack-all.sh --only "Paralax.WebApi,Paralax.HTTP"
#   ./scripts/pack-all.sh --force --only Paralax.WebApi   # --only wins (packages limited)
#
# Notes:
# - Uses per-package script: src/<Package>/scripts/build-and-pack.sh
# - Detects changed files both in GitHub Actions and locally.

FORCE_PACK_ALL=false
ONLY_PACKAGES=()

print_usage() {
  cat <<EOF
Usage:
  $0 [--force] [--only <pkg...|comma-separated>] [--list]

Options:
  --force            Package ALL libraries (ignores change detection).
  --only             Package ONLY selected packages (space-separated or comma-separated).
  --list             Print the known package order and exit.
  -h, --help         Show this help.
EOF
}

# Parse args
while [[ $# -gt 0 ]]; do
  case "$1" in
    --force)
      FORCE_PACK_ALL=true
      shift
      ;;
    --only)
      shift
      if [[ $# -eq 0 ]]; then
        echo "Error: --only requires at least one package name."
        exit 2
      fi
      # Accept comma-separated or multiple args until next flag
      if [[ "$1" == *","* ]]; then
        IFS=',' read -r -a ONLY_PACKAGES <<< "$1"
        shift
      else
        while [[ $# -gt 0 && "$1" != --* ]]; do
          ONLY_PACKAGES+=("$1")
          shift
        done
      fi
      ;;
    --list)
      LIST_ONLY=true
      shift
      ;;
    -h|--help)
      print_usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1"
      print_usage
      exit 2
      ;;
  esac
done

# Define the processing order
declare -a packages=(
  "Paralax"
  "Paralax.Auth"
  "Paralax.Logging"
  "Paralax.WebApi"
  "Paralax.MessageBrokers"
  "Paralax.gRPC.Protobuf"
  "Paralax.gRPC"
  "Paralax.Security"
  "Paralax.HTTP"
  "Paralax.Auth.Distributed"

  "Paralax.CQRS.Commands"
  "Paralax.CQRS.Events"
  "Paralax.CQRS.EventSourcing"
  "Paralax.CQRS.Queries"
  "Paralax.CQRS.Logging"
  "Paralax.Discovery.Consul"
  "Paralax.Docs.Scalar"
  "Paralax.Docs.Swagger"

  "Paralax.Persistence.MongoDB"
  "Paralax.Persistence.Redis"
  "Paralax.Persistence.Postgres"
  "Paralax.LoadBalancing.Fabio"

  "Paralax.MessageBrokers.CQRS"
  "Paralax.MessageBrokers.Outbox"
  "Paralax.MessageBrokers.Outbox.Mongo"
  "Paralax.MessageBrokers.RabbitMQ"
  "Paralax.Metrics.AppMetrics"
  "Paralax.Metrics.Prometheus"
  "Paralax.Secrets.Vault"

  "Paralax.Tracing.Jaeger"
  "Paralax.Tracing.Jaeger.RabbitMQ"

  "Paralax.WebApi.Scalar"
  "Paralax.WebApi.Security"
  "Paralax.WebApi.Swagger"
  "Paralax.CQRS.WebApi"

  "Paralax.OpenTelemetry"
  "Paralax.Diagnostics.HealthChecks"
  "Paralax.ServiceDefaults"
)

# --list
if [[ "${LIST_ONLY:-false}" == "true" ]]; then
  printf "%s\n" "${packages[@]}"
  exit 0
fi

# Commit-message force flag (still supported)
COMMIT_MESSAGE="$(git log -1 --pretty=%B)"
if [[ "$COMMIT_MESSAGE" == *"[pack-all-force]"* ]]; then
  echo "$divider"
  echo "Commit message contains [pack-all-force]. Forcing packaging of all libraries."
  echo "$divider"
  FORCE_PACK_ALL=true
fi

# If --only is set, it should override everything else (including --force)
normalize_pkg() {
  # trim spaces
  local s="$1"
  s="${s#"${s%%[![:space:]]*}"}"
  s="${s%"${s##*[![:space:]]}"}"
  echo "$s"
}

if [[ ${#ONLY_PACKAGES[@]} -gt 0 ]]; then
  # Normalize + validate against known list
  declare -A known=()
  for p in "${packages[@]}"; do known["$p"]=1; done

  filtered=()
  for p in "${ONLY_PACKAGES[@]}"; do
    p="$(normalize_pkg "$p")"
    [[ -z "$p" ]] && continue
    if [[ -z "${known[$p]:-}" ]]; then
      echo "Error: unknown package in --only: '$p'"
      echo "Run: $0 --list"
      exit 2
    fi
    filtered+=("$p")
  done

  ONLY_PACKAGES=("${filtered[@]}")
  echo "$divider"
  echo "Mode: ONLY selected packages:"
  printf " - %s\n" "${ONLY_PACKAGES[@]}"
  echo "$divider"
else
  echo "$divider"
  echo "Mode: auto-detect changed packages (use --force to override)."
  echo "$divider"
fi

# Detect changed files (works in GitHub Actions + locally)
CHANGED_FILES=""
echo "$divider"
echo "Detecting changed files..."
echo "$divider"

if [[ -n "${GITHUB_SHA:-}" && -n "${GITHUB_EVENT_BEFORE:-}" ]]; then
  if [[ "$GITHUB_EVENT_BEFORE" == "0000000000000000000000000000000000000000" ]]; then
    echo "GITHUB_EVENT_BEFORE is empty (first push). Using all files."
    CHANGED_FILES="$(git ls-tree -r --name-only HEAD)"
  else
    echo "Using GitHub Actions diff: $GITHUB_EVENT_BEFORE..$GITHUB_SHA"
    CHANGED_FILES="$(git diff --name-only "$GITHUB_EVENT_BEFORE" "$GITHUB_SHA" || true)"
  fi
elif [[ "${GITHUB_EVENT_NAME:-}" == "pull_request" && "${GITHUB_BASE_REF:-}" == "main" ]]; then
  # optional PR logic if you keep it
  echo "Pull request detected targeting main. Comparing origin/main..origin/dev"
  CHANGED_FILES="$(git diff --name-only origin/main origin/dev || true)"
else
  if git rev-parse --verify HEAD~1 >/dev/null 2>&1; then
    echo "Local run. Using diff: HEAD~1..HEAD"
    CHANGED_FILES="$(git diff --name-only HEAD~1 HEAD || true)"
  else
    echo "Only one commit found. Using all files."
    CHANGED_FILES="$(git ls-tree -r --name-only HEAD)"
  fi
fi

# Function to check if the directory contains changes
directory_contains_changes() {
  local dir="$1"

  # If --only is set, ignore change detection here (caller decides)
  if [[ ${#ONLY_PACKAGES[@]} -gt 0 ]]; then
    return 1
  fi

  if [[ "$FORCE_PACK_ALL" == "true" ]]; then
    return 0
  fi

  # If CHANGED_FILES is empty, assume nothing changed
  [[ -z "$CHANGED_FILES" ]] && return 1

  while IFS= read -r file; do
    [[ -z "$file" ]] && continue
    if [[ "$file" == "$dir"* ]]; then
      return 0
    fi
  done <<< "$CHANGED_FILES"

  return 1
}

# Decide if we should run a package
should_process_package() {
  local pkg="$1"
  local dir="$base_dir/$pkg"

  if [[ ${#ONLY_PACKAGES[@]} -gt 0 ]]; then
    for p in "${ONLY_PACKAGES[@]}"; do
      [[ "$p" == "$pkg" ]] && return 0
    done
    return 1
  fi

  directory_contains_changes "$dir"
}

echo "$divider"
echo "Starting the process of packaging libraries"
echo "$divider"

# Iterate through packages
for package in "${packages[@]}"; do
  dir="$base_dir/$package"
  script_path="$dir/scripts/build-and-pack.sh"

  echo "$divider"
  echo "Checking package: $package"
  echo "$divider"

  if should_process_package "$package"; then
    echo "Processing package: $package"
    echo "$divider"

    if [[ -f "$script_path" ]]; then
      echo "Found packaging script for: $package"
      chmod +x "$script_path"

      echo "Executing packaging script for: $package"
      "$script_path"

      echo "$divider"
      echo "Successfully packed and published: $package"
      echo "$divider"
    else
      echo "$divider"
      echo "Warning: No packaging script found for $package at $script_path"
      echo "$divider"
    fi
  else
    echo "$divider"
    echo "Skipping package: $package"
    if [[ ${#ONLY_PACKAGES[@]} -gt 0 ]]; then
      echo "(not in --only list)"
    else
      echo "(no changes detected; use --force to pack all)"
    fi
    echo "$divider"
  fi
done

echo "$divider"
echo "Finished processing NuGet packages."
echo "$divider"
