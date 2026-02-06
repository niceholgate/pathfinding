#!/bin/bash

# This script publishes both Release and Debug DLLs for AStar and its dependencies.

# Set the project file path
PROJECT_FILE="AStar.csproj"

# Set the output directory paths
RELEASE_DIR="./publish/Release"
DEBUG_DIR="./publish/Debug"

# Parse arguments
COPY_DEBUG=false
for arg in "$@"
do
    if [ "$arg" == "--copyDebug" ]; then
        COPY_DEBUG=true
    fi
done

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

if [ "$COPY_DEBUG" = true ]; then
    TARGET_DIR="/c/Users/niceh/dev/tag/Assets/Plugins"
    echo "Copying Debug output to $TARGET_DIR..."
    if [ -d "$TARGET_DIR" ]; then
        cp -rf "$DEBUG_DIR"/* "$TARGET_DIR/"
    else
        echo "Error: Target directory $TARGET_DIR does not exist."
        exit 1
    fi
fi

echo "Publishing complete."
echo "Release DLLs are in: $RELEASE_DIR"
echo "Debug DLLs are in: $DEBUG_DIR"
