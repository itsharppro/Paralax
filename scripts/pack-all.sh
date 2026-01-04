#!/usr/bin/env bash
set -euo pipefail

base_dir="src"
divider="----------------------------------------"

FORCE_PACK_ALL=false
ONLY_PACKAGES=()
LIST_ONLY=false
PACKAGE_VERSION=""

# Load .env from repo root if present (exports variables)
load_env() {
  local script_dir repo_root
  script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  repo_root="$(cd "$script_dir/.." && pwd)"

  if [[ -f "$repo_root/.env" ]]; then
    set -a
    # shellcheck disable=SC1090
    source "$repo_root/.env"
    set +a
  fi
}

load_env

print_usage() {
  cat <<EOF
Usage:
  $0 [--version <x.y.z>] [--force] [--only <pkg...|comma-separated>] [--list]

Options:
  --version <x.y.z>    Package version to use (required locally; optional in CI).
  --force              Package ALL libraries (ignores change detection).
  --only               Package ONLY selected packages (space-separated or comma-separated).
  --list               Print the known package order and exit.
  -h, --help           Show this help.

Notes:
  - Loads env from .env at repo root if present.
  - Passes version to src/<Package>/scripts/build-and-pack.sh

Examples:
  $0 --version 1.2.3
  $0 --version 1.2.3 --force
  $0 --version 1.2.3 --only Paralax.WebApi Paralax.HTTP
  $0 --version 1.2.3 --only "Paralax.WebApi,Paralax.HTTP"
EOF
}

# Parse args
while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      shift
      [[ $# -eq 0 ]] && { echo "Error: --version requires a value."; exit 2; }
      PACKAGE_VERSION="$1"
      shift
      ;;
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

# Package order
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

if [[ "$LIST_ONLY" == "true" ]]; then
  printf "%s\n" "${packages[@]}"
  exit 0
fi

# Commit-message force flag
COMMIT_MESSAGE="$(git log -1 --pretty=%B 2>/dev/null || true)"
if [[ "$COMMIT_MESSAGE" == *"[pack-all-force]"* ]]; then
  echo "$divider"
  echo "Commit message contains [pack-all-force]. Forcing packaging of all libraries."
  echo "$divider"
  FORCE_PACK_ALL=true
fi

# Determine version
if [[ -z "$PACKAGE_VERSION" ]]; then
  if [[ -n "${GITHUB_RUN_NUMBER:-}" ]]; then
    PACKAGE_VERSION="1.0.${GITHUB_RUN_NUMBER}"
    echo "$divider"
    echo "No --version provided. Using CI default: $PACKAGE_VERSION"
    echo "$divider"
  else
    echo "Error: --version is required when running locally."
    echo "Example: $0 --version 1.2.3"
    exit 2
  fi
fi

normalize_pkg() {
  local s="$1"
  s="${s#"${s%%[![:space:]]*}"}"
  s="${s%"${s##*[![:space:]]}"}"
  echo "$s"
}

if [[ ${#ONLY_PACKAGES[@]} -gt 0 ]]; then
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
  echo "Mode: ONLY selected packages (version: $PACKAGE_VERSION)"
  printf " - %s\n" "${ONLY_PACKAGES[@]}"
  echo "$divider"
else
  echo "$divider"
  echo "Mode: auto-detect changed packages (version: $PACKAGE_VERSION) (use --force to override)"
  echo "$divider"
fi

# Detect changed files
CHANGED_FILES=""
echo "$divider"
echo "Detecting changed files..."
echo "$divider"

if [[ -n "${GITHUB_SHA:-}" && -n "${GITHUB_EVENT_BEFORE:-}" ]]; then
  if [[ "$GITHUB_EVENT_BEFORE" == "0000000000000000000000000000000000000000" ]]; then
    echo "GITHUB_EVENT_BEFORE empty (first push). Using all files."
    CHANGED_FILES="$(git ls-tree -r --name-only HEAD)"
  else
    echo "Using GitHub Actions diff: $GITHUB_EVENT_BEFORE..$GITHUB_SHA"
    CHANGED_FILES="$(git diff --name-only "$GITHUB_EVENT_BEFORE" "$GITHUB_SHA" || true)"
  fi
else
  if git rev-parse --verify HEAD~1 >/dev/null 2>&1; then
    echo "Local run. Using diff: HEAD~1..HEAD"
    CHANGED_FILES="$(git diff --name-only HEAD~1 HEAD || true)"
  else
    echo "Only one commit found. Using all files."
    CHANGED_FILES="$(git ls-tree -r --name-only HEAD)"
  fi
fi

directory_contains_changes() {
  local dir="$1"

  if [[ ${#ONLY_PACKAGES[@]} -gt 0 ]]; then
    return 1
  fi
  if [[ "$FORCE_PACK_ALL" == "true" ]]; then
    return 0
  fi
  [[ -z "$CHANGED_FILES" ]] && return 1

  while IFS= read -r file; do
    [[ -z "$file" ]] && continue
    if [[ "$file" == "$dir"* ]]; then
      return 0
    fi
  done <<< "$CHANGED_FILES"

  return 1
}

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
echo "Starting packaging libraries (version: $PACKAGE_VERSION)"
echo "$divider"

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
      chmod +x "$script_path"
      echo "Executing: $script_path $PACKAGE_VERSION"
      "$script_path" "$PACKAGE_VERSION"
      echo "$divider"
      echo "Done: $package"
      echo "$divider"
    else
      echo "$divider"
      echo "Warning: No script for $package at $script_path"
      echo "$divider"
    fi
  else
    echo "$divider"
    echo "Skipping: $package"
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
