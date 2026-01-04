#!/usr/bin/env bash
set -euo pipefail

divider="----------------------------------------"

VERSION="${1:-}"
if [[ -z "$VERSION" ]]; then
  echo "Usage: $0 <version>"
  exit 2
fi

# Resolve paths
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Load .env (exports variables)
if [[ -f "$REPO_ROOT/.env" ]]; then
  set -a
  # shellcheck disable=SC1090
  source "$REPO_ROOT/.env"
  set +a
fi

# Defaults (can be overridden by .env or CI env)
: "${NUGET_SOURCE:=https://api.nuget.org/v3/index.json}"
: "${NUGET_API_KEY:=}"
: "${CERTIFICATE_PATH:=}"
: "${CERTIFICATE_PASSWORD:=}"
: "${TIMESTAMPER_URL:=http://timestamp.digicert.com}"
: "${CONFIGURATION:=Release}"
: "${SKIP_PUSH:=false}"
: "${SKIP_SIGN:=false}"

# PACKAGE_DIR is exported by per-package wrapper
PACKAGE_DIR="${PACKAGE_DIR:-$(pwd)}"
PACKAGE_NAME="$(basename "$PACKAGE_DIR")"

# Preferred layout: src/<Package>/src/<Package>/
PROJECT_DIR="$PACKAGE_DIR/src/$PACKAGE_NAME"

if [[ ! -d "$PROJECT_DIR" ]]; then
  # fallback: find first csproj
  csproj="$(find "$PACKAGE_DIR" -maxdepth 5 -name "*.csproj" | head -n 1 || true)"
  if [[ -z "$csproj" ]]; then
    echo "Error: could not find a .csproj under $PACKAGE_DIR"
    exit 1
  fi
  PROJECT_DIR="$(dirname "$csproj")"
fi

OUT_DIR="$PROJECT_DIR/nupkg"

echo "$divider"
echo "Package:        $PACKAGE_NAME"
echo "Version:        $VERSION"
echo "Project dir:    $PROJECT_DIR"
echo "Configuration:  $CONFIGURATION"
echo "Output dir:     $OUT_DIR"
echo "Skip sign:      $SKIP_SIGN"
echo "Skip push:      $SKIP_PUSH"
echo "$divider"

cd "$PROJECT_DIR"

echo "Restoring..."
dotnet restore

# ✅ Ensure output folder exists and delete previously produced packages
mkdir -p "$OUT_DIR"
rm -f "$OUT_DIR/$PACKAGE_NAME.$VERSION.nupkg" \
      "$OUT_DIR/$PACKAGE_NAME.$VERSION.snupkg" 2>/dev/null || true

echo "Packing..."
dotnet pack -c "$CONFIGURATION" /p:PackageVersion="$VERSION" --no-restore -o "$OUT_DIR"

PACKAGE_PATH="$OUT_DIR/$PACKAGE_NAME.$VERSION.nupkg"
if [[ ! -f "$PACKAGE_PATH" ]]; then
  PACKAGE_PATH="$(find "$OUT_DIR" -maxdepth 1 -type f -name "*.nupkg" ! -name "*.snupkg" | grep -F "$VERSION" | head -n 1 || true)"
fi

if [[ -z "${PACKAGE_PATH:-}" || ! -f "$PACKAGE_PATH" ]]; then
  echo "Error: .nupkg not found in $OUT_DIR for version $VERSION"
  exit 1
fi

echo "Built package: $PACKAGE_PATH"

# Sign
if [[ "$SKIP_SIGN" == "true" ]]; then
  echo "Skipping signing (SKIP_SIGN=true)."
else
  if [[ -z "$CERTIFICATE_PATH" ]]; then
    echo "CERTIFICATE_PATH not set; skipping signing."
  else
    echo "Signing the NuGet package..."
    SIGN_ARGS=(--certificate-path "$CERTIFICATE_PATH" --timestamper "$TIMESTAMPER_URL" --overwrite)

    if [[ -n "$CERTIFICATE_PASSWORD" ]]; then
      SIGN_ARGS+=(--certificate-password "$CERTIFICATE_PASSWORD")
    fi

    dotnet nuget sign "$PACKAGE_PATH" "${SIGN_ARGS[@]}"
  fi
fi

# Push
if [[ "$SKIP_PUSH" == "true" ]]; then
  echo "Skipping push (SKIP_PUSH=true)."
else
  if [[ -z "$NUGET_API_KEY" ]]; then
    echo "Error: NUGET_API_KEY is not set (required to push)."
    echo "Tip: Put it in .env as NUGET_API_KEY=..."
    echo "Or run without pushing: SKIP_PUSH=true $0 $VERSION"
    exit 2
  fi

  echo "Pushing to NuGet..."
  dotnet nuget push "$PACKAGE_PATH" \
    -k "$NUGET_API_KEY" \
    -s "$NUGET_SOURCE" \
    --skip-duplicate

  echo "Package uploaded."
fi
