#!/bin/bash

# Check if the commit message contains "build-test-force"
if git log -1 --pretty=%B | grep -q "\[build-test-force\]"; then
  echo "build-test-force flag detected, forcing build and test for all libraries."
  FORCE_BUILD_TEST=true
else
  echo "No build-test-force flag detected. Proceeding with standard build and test."
  FORCE_BUILD_TEST=false
fi

LIBRARIES=$(jq -r '.[]' libraries.json)

for LIBRARY in $LIBRARIES; do
  echo "Processing $LIBRARY"

  # Ensure we are working with valid library paths
  if [ -d "src/$LIBRARY/src" ]; then

    # Either force build/test or run only if changes detected
    if [ "$FORCE_BUILD_TEST" = true ] || [ -n "$(git diff --name-only HEAD~1 HEAD | grep "src/$LIBRARY")" ]; then
      echo "Restoring dependencies for $LIBRARY"
      dotnet restore "src/$LIBRARY/src"

      echo "Building $LIBRARY"
      dotnet build "src/$LIBRARY/src" --configuration Release --no-restore

      if [ -d "src/$LIBRARY/tests" ]; then
        echo "Running tests for $LIBRARY"
        dotnet test "src/$LIBRARY/tests" --configuration Release \
          --collect:"XPlat Code Coverage" \
          --results-directory "src/$LIBRARY/TestResults/" \
          /p:CollectCoverage=true \
          /p:CoverletOutputFormat=cobertura \
          /p:CoverletOutput="src/$LIBRARY/TestResults/coverage.cobertura.xml" \
          --logger "trx;LogFileName=TestResults.trx"

        # Check if the tests succeeded
        if [ $? -ne 0 ]; then
          echo "Tests failed for $LIBRARY. Exiting."
          exit 1
        fi

        # Dynamically find the coverage report in the TestResults folder
        COVERAGE_REPORT=$(find src/$LIBRARY/TestResults/ -name "coverage.cobertura.xml" | head -n 1)
        if [ -f "$COVERAGE_REPORT" ]; then
          echo "Uploading coverage report to Codecov for $LIBRARY"
          bash <(curl -s https://codecov.io/bash) -t "$CODECOV_TOKEN" -f "$COVERAGE_REPORT" -F $LIBRARY
        else
          echo "Coverage report not found for $LIBRARY. Expected at $COVERAGE_REPORT"
        fi

        echo "Uploading test results for $LIBRARY"
        # Move all test result files in the subfolders to the right directory
        mv src/$LIBRARY/TestResults/*/TestResults.trx "src/$LIBRARY/TestResults/"
      else
        echo "No test project found for $LIBRARY"
      fi
    else
      echo "Skipping $LIBRARY, no changes detected."
    fi
  else
    echo "Library $LIBRARY not found."
  fi
done
