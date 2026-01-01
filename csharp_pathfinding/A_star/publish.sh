#!/bin/bash

# This script publishes both Release and Debug DLLs for AStar and its dependencies.

# Set the project file path
PROJECT_FILE="AStar.csproj"

# Set the output directory paths
RELEASE_DIR="./publish/Release"
DEBUG_DIR="./publish/Debug"

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf ./publish

# Publish for Release configuration
echo "Publishing Release version..."
dotnet publish "$PROJECT_FILE" -c Release -o "$RELEASE_DIR"

# Check if the publish command was successful
if [ $? -ne 0 ]; then
    echo "Error: Release publish failed."
    exit 1
fi

# Publish for Debug configuration
echo "Publishing Debug version..."
dotnet publish "$PROJECT_FILE" -c Debug -o "$DEBUG_DIR"

# Check if the publish command was successful
if [ $? -ne 0 ]; then
    echo "Error: Debug publish failed."
    exit 1
fi

echo "Publishing complete."
echo "Release DLLs are in: $RELEASE_DIR"
echo "Debug DLLs are in: $DEBUG_DIR"
