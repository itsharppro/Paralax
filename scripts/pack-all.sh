#!/bin/bash

base_dir="src"
divider="----------------------------------------"

# Get the latest commit message
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

# # Check for pull request to main or else detect changes
# if [[ "$GITHUB_EVENT_NAME" == "pull_request" && "$GITHUB_BASE_REF" == "main" ]]; then
#     echo "$divider"
#     echo "Pull request detected targeting the main branch."
#     echo "Comparing changes between the 'dev' and 'main' branches..."
#     echo "$divider"

#     # Compare changes between 'dev' and 'main'
#     CHANGED_FILES=$(git diff --name-only origin/main origin/dev)
# else
#     echo "$divider"
#     echo "No pull request detected or not targeting main. Checking commit differences."
#     echo "$divider"

#     # Use the current commit if SHA is missing
#     if [ -z "$GITHUB_SHA" ]; then
#         GITHUB_SHA=$(git rev-parse HEAD)
#     fi

#     # Check for changed files in the last commit
#     CHANGED_FILES=$(git diff --name-only "$GITHUB_SHA~1" "$GITHUB_SHA" || echo "")
# fi

# Function to detect if a directory contains any changes
directory_contains_changes() {
    local dir="$1"
    if $FORCE_PACK_ALL; then
        return 0
    fi
    for file in $CHANGED_FILES; do
        if [[ "$file" == "$dir"* ]]; then
            return 0  # Directory contains changes
        fi
    done
    return 1  # No changes detected
}

# Function to get a list of dependencies from the .csproj file using 'dotnet list package'
get_dependencies() {
    local project_file="$1"
    if [ -f "$project_file" ]; then
        dotnet list "$project_file" package --include-transitive | awk '/>/' | awk '{print $2}'
    fi
}

# Dictionary to store project build orders
declare -A project_build_order
build_counter=0

# Resolve build order for dependencies using a topological-like sorting method
resolve_build_order() {
    local project_dir="$1"
    local project_file="$project_dir/*.csproj"
    
    if [[ ! -f $project_file ]]; then
        return
    fi
    
    # Get project dependencies
    local dependencies=$(get_dependencies "$project_file")
    
    # Recursively process dependencies
    for dependency in $dependencies; do
        dependency_dir=$(find "$base_dir" -name "$dependency.csproj" -exec dirname {} \;)
        if [[ -n "$dependency_dir" ]]; then
            resolve_build_order "$dependency_dir"
        fi
    done
    
    # Add the current project to the build order if not already added
    if [[ -z "${project_build_order[$project_dir]}" ]]; then
        project_build_order[$project_dir]=$build_counter
        build_counter=$((build_counter + 1))
    fi
}

# Determine build order based on project dependencies
echo "$divider"
echo "Determining build order based on dependencies"
echo "$divider"

for dir in "$base_dir"/*/
do
    dir=${dir%*/}
    resolve_build_order "$dir"
done

# Sort projects by their build order
sorted_projects=$(for dir in "${!project_build_order[@]}"; do echo "${project_build_order[$dir]}:$dir"; done | sort -n | cut -d':' -f2)

echo "$divider"
echo "Starting the process of packaging libraries in the correct order"
echo "$divider"

# Process projects in the correct order
for dir in $sorted_projects
do
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
