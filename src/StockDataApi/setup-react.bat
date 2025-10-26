@echo off
echo ===================================
echo Stock Data API - React Setup Script
echo ===================================

echo Checking if Node.js is installed...
where node >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: Node.js is not installed or not in PATH.
    echo Please install Node.js from https://nodejs.org/
    exit /b 1
)

echo Node.js found:
node --version

echo Checking if npm is installed...
where npm >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: npm is not installed or not in PATH.
    echo Please install npm (it usually comes with Node.js)
    exit /b 1
)

echo npm found:
npm --version

echo Installing npm packages...
call npm install
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to install npm packages.
    exit /b 1
)

echo Creating js/react directory if it doesn't exist...
if not exist "wwwroot\js\react" mkdir "wwwroot\js\react"

echo Backing up placeholder bundle.js...
if exist "wwwroot\js\react\bundle.js" (
    echo Backing up existing bundle.js to bundle.js.bak
    copy "wwwroot\js\react\bundle.js" "wwwroot\js\react\bundle.js.bak" >nul
)

echo Building React bundle...
call npm run build
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to build React bundle.
    if exist "wwwroot\js\react\bundle.js.bak" (
        echo Restoring bundle.js from backup...
        copy "wwwroot\js\react\bundle.js.bak" "wwwroot\js\react\bundle.js" >nul
    )
    exit /b 1
)

echo Verifying bundle.js was created...
if not exist "wwwroot\js\react\bundle.js" (
    echo ERROR: bundle.js was not created. Check webpack configuration.
    exit /b 1
)

echo Cleaning up...
if exist "wwwroot\js\react\bundle.js.bak" (
    del "wwwroot\js\react\bundle.js.bak" >nul
)

echo React setup completed successfully!
echo You can now run the application and the React components should be available.
echo.
echo NOTE: You may need to clear your browser cache or do a hard refresh (Ctrl+F5)
echo to see the changes if you've previously loaded the application.

@REM Made with Bob
