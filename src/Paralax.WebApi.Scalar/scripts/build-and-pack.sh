#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-}"
if [[ -z "$VERSION" ]]; then
  echo "Usage: $0 <version>"
  exit 2
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PACKAGE_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"      # src/<Package>
REPO_ROOT="$(cd "$PACKAGE_DIR/../.." && pwd)"    # repo root

export PACKAGE_DIR

chmod +x "$REPO_ROOT/scripts/nuget/build-and-pack-package.sh"
"$REPO_ROOT/scripts/nuget/build-and-pack-package.sh" "$VERSION"
