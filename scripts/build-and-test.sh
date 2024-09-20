#!/bin/bash

# Read the libraries to build and test from the libraries.json file
LIBRARIES=$(jq -r '.[]' libraries.json)

# Loop through each library and run the build and test commands
for LIBRARY in $LIBRARIES; do
  echo "Processing $LIBRARY"

  # Ensure we are working with valid library paths
  if [ -d "src/$LIBRARY/src" ]; then
    # Restore dependencies for the library
    echo "Restoring dependencies for $LIBRARY"
    dotnet restore "src/$LIBRARY/src"

    # Build the library
    echo "Building $LIBRARY"
    dotnet build "src/$LIBRARY/src" --configuration Release --no-restore

    # Run tests for the library
    if [ -d "src/$LIBRARY/tests" ]; then
      echo "Running tests for $LIBRARY"
      dotnet test "src/$LIBRARY/tests" --configuration Release --no-build \
        --collect:"XPlat Code Coverage" \
        --results-directory TestResults/ \
        /p:CollectCoverage=true \
        /p:CoverletOutputFormat=cobertura \
        --logger "trx;LogFileName=TestResults.trx"

      # Check if the tests succeeded
      if [ $? -ne 0 ]; then
        echo "Tests failed for $LIBRARY. Exiting."
        exit 1
      fi

      # Upload test results and coverage report
      echo "Uploading test results for $LIBRARY"
      mv TestResults/* "src/$LIBRARY/TestResults/"
    else
      echo "No test project found for $LIBRARY"
    fi
  else
    echo "Library $LIBRARY not found."
  fi
done
