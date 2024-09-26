#!/bin/bash

echo "Running tests and collecting coverage for Paralax.Tracing.Jaeger.RabbitMQ..."

cd src/ParalParalax.Tracing.Jaeger.RabbitMQax/tests/Paralax.Tracing.Jaeger.RabbitMQ.Tests

echo "Restoring NuGet packages..."
dotnet restore

echo "Running tests and generating code coverage report..."
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Check if tests succeeded
if [ $? -ne 0 ]; then
  echo "Tests failed. Exiting..."
  exit 1
fi

