#!/bin/bash

if [ -z "${GITHUB_EVENT_BEFORE}" ] || ! git cat-file -e ${GITHUB_EVENT_BEFORE}^{commit}; then
  echo "No valid previous commit detected, running for all libraries."
  CHANGED_FILES=$(git ls-tree -r HEAD --name-only)
else
  echo "Checking for changes between commits."
  CHANGED_FILES=$(git diff --name-only ${GITHUB_EVENT_BEFORE} ${GITHUB_SHA})
fi

LIBRARIES=()

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

echo "${LIBRARIES[@]}" | jq -R -s -c 'split(" ")' > libraries.json
