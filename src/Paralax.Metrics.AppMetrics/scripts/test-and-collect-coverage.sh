#!/bin/bash

echo "Running tests and collecting coverage for Paralax.Metrics.AppMetrics..."

cd src/Paralax.Metrics.AppMetrics/tests/Paralax.Metrics.AppMetrics.Tests

echo "Restoring NuGet packages..."
dotnet restore

echo "Running tests and generating code coverage report..."
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Check if tests succeeded
if [ $? -ne 0 ]; then
  echo "Tests failed. Exiting..."
  exit 1
fi

