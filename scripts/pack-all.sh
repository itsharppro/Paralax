#!/bin/bash

base_dir="src"
divider="----------------------------------------"

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

if [[ "$GITHUB_EVENT_NAME" == "pull_request" && "$GITHUB_BASE_REF" == "main" ]]; then
    echo "$divider"
    echo "Pull request detected targeting the main branch."
    echo "Comparing changes between the 'dev' and 'main' branches..."
    echo "$divider"

    CHANGED_FILES=$(git diff --name-only origin/main origin/dev)
else
    echo "$divider"
    echo "No pull request detected or not targeting main. Checking commit differences."
    echo "$divider"
    # Get the list of files that were changed in the last commit
    CHANGED_FILES=$(git diff --name-only $GITHUB_SHA~1 $GITHUB_SHA)
fi

# Function to detect changed files in a directory
directory_contains_changes() {
    local dir="$1"
    if $FORCE_PACK_ALL; then
        return 0 
    fi
    for file in $CHANGED_FILES; do
        if [[ "$file" == "$dir"* ]]; then
            return 0  
        fi
    done
    return 1  # No changed files in the directory
}

# Function to parse .csproj and extract dependencies
extract_dependencies() {
    local project_file="$1"
    grep -oP '(?<=<PackageReference Include=").+?(?=")' "$project_file"
}

# Dictionary to store the order of project builds
declare -A project_build_order
build_counter=0

# Topological sort to determine the build order based on dependencies
determine_build_order() {
    local project_dir="$1"
    local project_file="$project_dir/*.csproj"
    
    if [[ ! -f $project_file ]]; then
        return
    fi
    
    # Extract dependencies from the .csproj file
    local dependencies=$(extract_dependencies "$project_file")
    
    # If the project has dependencies, process them first
    for dependency in $dependencies; do
        dependency_dir=$(find "$base_dir" -name "$dependency.csproj" -exec dirname {} \;)
        if [[ -n "$dependency_dir" ]]; then
            determine_build_order "$dependency_dir"
        fi
    done
    
    # Add project to the build order if it hasn't been added yet
    if [[ -z "${project_build_order[$project_dir]}" ]]; then
        project_build_order[$project_dir]=$build_counter
        build_counter=$((build_counter + 1))
    fi
}

# First, determine the build order of all projects based on dependencies
echo "$divider"
echo "Determining build order based on dependencies"
echo "$divider"

for dir in "$base_dir"/*/
do
    dir=${dir%*/}
    determine_build_order "$dir"
done

# Sort the projects by their determined build order
sorted_projects=$(for dir in "${!project_build_order[@]}"; do echo "${project_build_order[$dir]}:$dir"; done | sort -n | cut -d':' -f2)

echo "$divider"
echo "Starting the process of packaging libraries in the correct order"
echo "$divider"

# Now, process the projects in the correct order
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
