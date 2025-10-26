#!/bin/bash

echo "==================================="
echo "Stock Data API - React Setup Script"
echo "==================================="

# Check if Node.js is installed
echo "Checking if Node.js is installed..."
if ! command -v node &> /dev/null; then
    echo "ERROR: Node.js is not installed or not in PATH."
    echo "Please install Node.js from https://nodejs.org/"
    exit 1
fi

echo "Node.js found: $(node --version)"

# Check if npm is installed
echo "Checking if npm is installed..."
if ! command -v npm &> /dev/null; then
    echo "ERROR: npm is not installed or not in PATH."
    echo "Please install npm (it usually comes with Node.js)"
    exit 1
fi

echo "npm found: $(npm --version)"

# Install npm packages
echo "Installing npm packages..."
npm install
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to install npm packages."
    exit 1
fi

# Create directory if it doesn't exist
echo "Creating js/react directory if it doesn't exist..."
mkdir -p wwwroot/js/react

# Backup existing bundle.js if it exists
if [ -f "wwwroot/js/react/bundle.js" ]; then
    echo "Backing up existing bundle.js to bundle.js.bak"
    cp wwwroot/js/react/bundle.js wwwroot/js/react/bundle.js.bak
fi

# Build React bundle
echo "Building React bundle..."
npm run build
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build React bundle."
    # Restore backup if it exists
    if [ -f "wwwroot/js/react/bundle.js.bak" ]; then
        echo "Restoring bundle.js from backup..."
        cp wwwroot/js/react/bundle.js.bak wwwroot/js/react/bundle.js
    fi
    exit 1
fi

# Verify bundle.js was created
if [ ! -f "wwwroot/js/react/bundle.js" ]; then
    echo "ERROR: bundle.js was not created. Check webpack configuration."
    exit 1
fi

# Clean up
if [ -f "wwwroot/js/react/bundle.js.bak" ]; then
    echo "Cleaning up..."
    rm wwwroot/js/react/bundle.js.bak
fi

echo "React setup completed successfully!"
echo "You can now run the application and the React components should be available."
echo ""
echo "NOTE: You may need to clear your browser cache or do a hard refresh (Ctrl+F5)"
echo "to see the changes if you've previously loaded the application."

# Make the script executable
chmod +x "$0"

# Made with Bob
