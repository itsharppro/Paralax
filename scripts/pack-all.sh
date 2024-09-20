#!/bin/bash

base_dir="src"
divider="----------------------------------------"

# Get the list of files that were changed in the current commit
CHANGED_FILES=$(git diff --name-only $GITHUB_SHA~1 $GITHUB_SHA)

directory_contains_changes() {
    local dir="$1"
    for file in $CHANGED_FILES; do
        if [[ "$file" == "$dir"* ]]; then
            return 0  # Directory contains changed files
        fi
    done
    return 1  # No changed files in the directory
}

echo "$divider"
echo "Starting the process of packaging modified libraries"
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
        echo "Processing package: $package_name (modified in commit)"
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
echo "Finished processing all modified NuGet packages."
echo "$divider"
