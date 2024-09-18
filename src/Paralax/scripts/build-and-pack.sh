#!/bin/bash

echo "Executing post-success scripts for branch $GITHUB_REF_NAME"
echo "Starting build and NuGet package creation for Paralax framework..."

cd src/Paralax/src

echo "Restoring NuGet packages..."
dotnet restore

PACKAGE_VERSION="1.0.$GITHUB_RUN_NUMBER"
echo "Building and packing the Paralax library..."
dotnet pack -c release /p:PackageVersion=$PACKAGE_VERSION --no-restore -o ./nupkg

echo "Package created for branch $GITHUB_REF_NAME"

case "$GITHUB_REF_NAME" in
  "dev")
    echo "Preparing to sign the package for NuGet upload..."
    PACKAGE_PATH="./nupkg/Paralax.$PACKAGE_VERSION.nupkg"

    if [ -f "$PACKAGE_PATH" ]; then
      echo "Package found: $PACKAGE_PATH"

      echo "Signing the package..."
      signtool sign /fd SHA256 /a /t http://timestamp.digicert.com /v "$PACKAGE_PATH"

      if [ $? -eq 0 ]; then
        echo "Package successfully signed."

        echo "Uploading the signed Paralax package to NuGet..."
        dotnet nuget push "$PACKAGE_PATH" -k "$NUGET_API_KEY" -s https://api.nuget.org/v3/index.json

        if [ $? -eq 0 ]; then
          echo "Package uploaded to NuGet."
        else
          echo "Error uploading the package to NuGet."
          exit 1
        fi
      else
        echo "Error signing the package."
        exit 1
      fi
    else
      echo "Error: Package $PACKAGE_PATH not found."
      exit 1
    fi
    ;;
  *)
    echo "Not on main branch, skipping NuGet package upload."
    ;;
esac
