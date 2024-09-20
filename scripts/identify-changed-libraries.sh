#!/bin/bash

# Identify the changed files between the current commit and the previous one
if [ -z "${GITHUB_EVENT_BEFORE}" ] || ! git cat-file -e ${GITHUB_EVENT_BEFORE}^{commit}; then
  echo "No valid previous commit detected, running for all libraries."
  CHANGED_FILES=$(git ls-tree -r HEAD --name-only)
else
  echo "Checking for changes between commits."
  CHANGED_FILES=$(git diff --name-only ${GITHUB_EVENT_BEFORE} ${GITHUB_SHA})
fi

# Initialize an empty array to store the names of changed libraries
LIBRARIES=()

# Loop through the changed files to identify relevant projects
for file in $CHANGED_FILES; do
  if [[ "$file" == *".csproj" ]]; then
    # Extract the parent directory of the .csproj file, which corresponds to the library or project folder
    LIBRARY=$(dirname "$file" | cut -d'/' -f3)
    
    # Ensure we don't add duplicates
    if [[ ! " ${LIBRARIES[@]} " =~ " ${LIBRARY} " ]]; then
      LIBRARIES+=("$LIBRARY")
    fi
  fi
done

# If no libraries were identified, assume we need to process all valid project directories under src/
if [ ${#LIBRARIES[@]} -eq 0 ]; then
  echo "No changed libraries detected, processing all."
  LIBRARIES=($(find src/* -maxdepth 0 -type d -exec basename {} \;))
fi

# Write the libraries to a JSON file for later use
echo "${LIBRARIES[@]}" | jq -R -s -c 'split(" ")' > libraries.json
