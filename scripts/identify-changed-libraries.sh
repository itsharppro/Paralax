#!/bin/bash

# Check if the commit message contains "build-test-force"
if git log -1 --pretty=%B | grep -q "\[build-test-force\]"; then
  echo "[build-test-force] flag detected, processing all libraries."
  FORCE_BUILD_TEST=true
else
  echo "No build-test-force flag detected. Proceeding with changed libraries only."
  FORCE_BUILD_TEST=false
fi

if [ "$FORCE_BUILD_TEST" = false ]; then
  if [ -z "${GITHUB_EVENT_BEFORE}" ] || ! git cat-file -e ${GITHUB_EVENT_BEFORE}^{commit}; then
    echo "No valid previous commit detected, running for all libraries."
    CHANGED_FILES=$(git ls-tree -r HEAD --name-only)
  else
    echo "Checking for changes between commits."
    CHANGED_FILES=$(git diff --name-only ${GITHUB_EVENT_BEFORE} ${GITHUB_SHA})
  fi

  LIBRARIES=()

  # Loop through changed files and detect changed libraries
  for file in $CHANGED_FILES; do
    if [[ "$file" == *".csproj" ]]; then
      LIBRARY=$(dirname "$file" | cut -d'/' -f3)
      
      if [[ ! " ${LIBRARIES[@]} " =~ " ${LIBRARY} " ]]; then
        LIBRARIES+=("$LIBRARY")
      fi
    fi
  done

  if [ ${#LIBRARIES[@]} -eq 0 ]; then
    echo "No changed libraries detected, processing all."
    LIBRARIES=($(find src/* -maxdepth 0 -type d -exec basename {} \;))
  fi
else
  # If the force flag is detected, process all libraries
  echo "Forcing build and test for all libraries."
  LIBRARIES=($(find src/* -maxdepth 0 -type d -exec basename {} \;))
fi

# Write the libraries to libraries.json
echo "${LIBRARIES[@]}" | jq -R -s -c 'split(" ")' > libraries.json

# Output the libraries being processed for debugging
echo "Libraries to be processed: ${LIBRARIES[@]}"
