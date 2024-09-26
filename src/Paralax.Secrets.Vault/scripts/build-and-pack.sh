#!/bin/bash

echo "Executing post-success scripts for branch $GITHUB_REF_NAME"
echo "Starting build and NuGet package creation for Paralax framework..."

cd src/Paralax.Secrets.Vault/src/Paralax.Secrets.Vault

echo "Restoring NuGet packages..."
dotnet restore

PACKAGE_VERSION="1.0.$GITHUB_RUN_NUMBER"
echo "Building and packing the Paralax.Secrets.Vault library..."
dotnet pack -c release /p:PackageVersion=$PACKAGE_VERSION --no-restore -o ./nupkg

PACKAGE_PATH="./nupkg/Paralax.Secrets.Vault.$PACKAGE_VERSION.nupkg"

if [ -f "$PACKAGE_PATH" ]; then
  echo "Checking if the package is already signed..."
  if dotnet nuget verify "$PACKAGE_PATH" | grep -q 'Package is signed'; then
    echo "Package is already signed, skipping signing."
  else
    echo "Signing the NuGet package..."
    dotnet nuget sign "$PACKAGE_PATH" \
      --certificate-path "$CERTIFICATE_PATH" \
      --timestamper http://timestamp.digicert.com 
  fi

  echo "Uploading Paralax.Secrets.Vault package to NuGet..."
  dotnet nuget push "$PACKAGE_PATH" -k "$NUGET_API_KEY" \
    -s https://api.nuget.org/v3/index.json --skip-duplicate
  echo "Package uploaded to NuGet."
else
  echo "Error: Package $PACKAGE_PATH not found."
  exit 1
fi
