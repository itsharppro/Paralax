#!/bin/bash

LIBRARIES=$(jq -r '.[]' libraries.json)

for LIBRARY in $LIBRARIES; do
  echo "Processing $LIBRARY"

  echo "Restoring dependencies for $LIBRARY"
  dotnet restore src/$LIBRARY

  echo "Building $LIBRARY"
  dotnet build src/$LIBRARY --configuration Release --no-restore

  echo "Running tests for $LIBRARY"
  dotnet test src/$LIBRARY/tests --configuration Release --no-build \
    --collect:"XPlat Code Coverage" \
    --results-directory TestResults/ \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    --logger "trx;LogFileName=TestResults.trx"

  if [ $? -ne 0 ]; then
    echo "Tests failed for $LIBRARY. Exiting."
    exit 1
  fi

  echo "Uploading test results for $LIBRARY"
  mv TestResults/* src/$LIBRARY/TestResults/
done
