#!/bin/bash

echo "======================================="
echo "Stock Data API - Force Rebuild Script"
echo "======================================="

echo "Removing node_modules directory..."
if [ -d "node_modules" ]; then
    rm -rf "node_modules"
fi

echo "Removing existing bundle.js..."
if [ -f "wwwroot/js/react/bundle.js" ]; then
    rm "wwwroot/js/react/bundle.js"
fi

echo "Creating empty bundle.js to prevent 404 errors..."
mkdir -p "wwwroot/js/react"
echo "// Temporary bundle" > "wwwroot/js/react/bundle.js"

echo "Running setup-react.sh..."
bash ./setup-react.sh

if [ $? -ne 0 ]; then
    echo "ERROR: Setup script failed."
    exit 1
fi

echo "Force rebuild completed successfully!"
echo ""
echo "IMPORTANT: Make sure to:"
echo "1. Restart the application"
echo "2. Clear your browser cache or open in incognito/private mode"
echo "3. If using Docker, rebuild the container"

# Make the script executable
chmod +x "$0"

# Made with Bob
