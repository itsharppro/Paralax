#!/bin/bash

base_dir="src"
divider="----------------------------------------"

# Get the commit message of the latest commit
COMMIT_MESSAGE=$(git log -1 --pretty=%B)

# Check if the commit message contains [pack-all-force]
if [[ "$COMMIT_MESSAGE" == *"[pack-all-force]"* ]]; then
    echo "$divider"
    echo "Commit message contains [pack-all-force]. Forcing packaging of all libraries."
    echo "$divider"
    FORCE_PACK_ALL=true
else
    FORCE_PACK_ALL=false
fi

# Check if this is a pull request and if it's targeting the main branch
if [[ "$GITHUB_EVENT_NAME" == "pull_request" && "$GITHUB_BASE_REF" == "main" ]]; then
    echo "$divider"
    echo "Pull request detected targeting the main branch."
    echo "Comparing changes between the 'dev' and 'main' branches..."
    echo "$divider"
    # Compare changes between dev and main branches
    CHANGED_FILES=$(git diff --name-only origin/main origin/dev)
else
    echo "$divider"
    echo "No pull request detected or not targeting main. Checking commit differences."
    echo "$divider"
    # Get the list of files that were changed in the last commit
    CHANGED_FILES=$(git diff --name-only $GITHUB_SHA~1 $GITHUB_SHA)
fi

# Function to check if a directory contains changed files
directory_contains_changes() {
    local dir="$1"
    if $FORCE_PACK_ALL; then
        return 0  # If force packing is enabled, mark all directories as changed
    fi
    for file in $CHANGED_FILES; do
        if [[ "$file" == "$dir"* ]]; then
            return 0  # Directory contains changed files
        fi
    done
    return 1  # No changed files in the directory
}

echo "$divider"
echo "Starting the process of packaging libraries"
echo "$divider"

for dir in "$base_dir"/*/
do
    dir=${dir%*/}
    package_name=${dir##*/}
    script_path="$dir/scripts/build-and-pack.sh"
    
    echo "$divider"
    echo "Checking package: $package_name"
    echo "$divider"
    
    if directory_contains_changes "$dir"; then
        echo "Processing package: $package_name"
        echo "$divider"

        if [ -f "$script_path" ]; then
            echo "Found packaging script for: $package_name"
            echo "$divider"

            chmod +x "$script_path"

            echo "Executing packaging script for: $package_name"
            echo "$divider"
            "$script_path"

            if [ $? -ne 0 ]; then
                echo "$divider"
                echo "Error: Packaging failed for $package_name"
                echo "$divider"
                exit 1
            fi

            echo "$divider"
            echo "Successfully packed and published: $package_name"
            echo "$divider"
        else
            echo "$divider"
            echo "Warning: No packaging script found for $package_name"
            echo "$divider"
        fi
    else
        echo "$divider"
        echo "Skipping package: $package_name (no changes detected)"
        echo "$divider"
    fi
done

echo "$divider"
echo "Finished processing all NuGet packages."
echo "$divider"
