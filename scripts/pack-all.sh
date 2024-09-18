#!/bin/bash

base_dir="src"

for dir in "$base_dir"/*/
do
    dir=${dir%*/}
    package_name=${dir##*/}
    
    echo "Processing package: $package_name"
    
    script_path="$dir/scripts/build-and-pack.sh"
    
    if [ -f "$script_path" ]; then
        echo "Found packaging script for: $package_name"

        chmod +x "$script_path"
        
        echo "Executing packaging script for: $package_name"
        "$script_path"
        
        if [ $? -ne 0 ]; then
            echo "Error: Packaging failed for $package_name"
            exit 1
        fi

        echo "Successfully packed and published: $package_name"
    else
        echo "Warning: No packaging script found for $package_name"
    fi
done

echo "Finished publishing all NuGet packages."
