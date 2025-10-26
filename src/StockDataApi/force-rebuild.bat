@echo off
echo =======================================
echo Stock Data API - Force Rebuild Script
echo =======================================

echo Removing node_modules directory...
if exist "node_modules" (
    rmdir /s /q "node_modules"
)

echo Removing existing bundle.js...
if exist "wwwroot\js\react\bundle.js" (
    del "wwwroot\js\react\bundle.js"
)

echo Creating empty bundle.js to prevent 404 errors...
if not exist "wwwroot\js\react" mkdir "wwwroot\js\react"
echo // Temporary bundle > "wwwroot\js\react\bundle.js"

echo Running setup-react.bat...
call setup-react.bat

if %ERRORLEVEL% neq 0 (
    echo ERROR: Setup script failed.
    exit /b 1
)

echo Force rebuild completed successfully!
echo.
echo IMPORTANT: Make sure to:
echo 1. Restart the application
echo 2. Clear your browser cache or open in incognito/private mode
echo 3. If using Docker, rebuild the container

@REM Made with Bob
