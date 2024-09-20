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

    if [ "$FORCE_BUILD_TEST" = true ] || [ -n "$(git diff --name-only HEAD~1 HEAD | grep "src/$LIBRARY")" ]; then
      echo "Restoring dependencies for $LIBRARY"
      dotnet restore "src/$LIBRARY/src"

      echo "Building $LIBRARY"
      dotnet build "src/$LIBRARY/src" --configuration Release --no-restore

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

        echo "Uploading test results for $LIBRARY"
        mv TestResults/* "src/$LIBRARY/TestResults/"

        echo "Uploading coverage report to Codecov for $LIBRARY"
        bash <(curl -s https://codecov.io/bash) -t "$CODECOV_TOKEN" -f "src/$LIBRARY/TestResults/coverage.cobertura.xml" -F $LIBRARY
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
