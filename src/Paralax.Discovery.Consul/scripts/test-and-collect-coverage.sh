#!/bin/bash

echo "Running tests and collecting coverage for Paralax.Discovery.Consul..."

cd src/Paralax.Discovery.Consul/src/Paralax.Discovery.Consul

echo "Restoring NuGet packages..."
dotnet restore

echo "Running tests and generating code coverage report..."
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Check if tests succeeded
if [ $? -ne 0 ]; then
  echo "Tests failed. Exiting..."
  exit 1
fi

