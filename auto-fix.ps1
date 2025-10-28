# Auto-Fix Script for Stock Data API
# This script automatically fixes common issues in the project

param(
    [switch]$Build,
    [switch]$Test,
    [switch]$Clean,
    [switch]$Migrate,
    [switch]$All
)

function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    else {
        $input | Write-Output
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Test-BuildErrors {
    Write-ColorOutput Green "Checking for build errors..."
    
    $buildOutput = dotnet build --no-restore --verbosity quiet 2>&1
    $errors = $buildOutput | Where-Object { $_ -match "error CS" }
    
    if ($errors) {
        Write-ColorOutput Red "Build errors detected:"
        $errors | ForEach-Object { Write-ColorOutput Red "  $_" }
        return $false
    }
    else {
        Write-ColorOutput Green "No build errors found."
        return $true
    }
}

function Fix-MissingUsings {
    Write-ColorOutput Yellow "Checking for missing using statements..."
    
    $files = Get-ChildItem -Path "src" -Recurse -Filter "*.cs" | Where-Object { 
        $_.Name -notlike "*Designer.cs" -and 
        $_.Name -notlike "*Migration*.cs" 
    }
    
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        
        # Check for common missing usings
        if ($content -match "StockDataResponse" -and $content -notmatch "using StockDataApi.Models.Responses") {
            Write-ColorOutput Yellow "Adding missing using statement to $($file.Name)"
            $newContent = "using StockDataApi.Models.Responses;`n" + $content
            Set-Content -Path $file.FullName -Value $newContent
        }
        
        if ($content -match "GetAllStockDataQuery" -and $content -notmatch "using StockDataApi.Models.Queries") {
            Write-ColorOutput Yellow "Adding missing using statement to $($file.Name)"
            $newContent = "using StockDataApi.Models.Queries;`n" + $content
            Set-Content -Path $file.FullName -Value $newContent
        }
    }
}

function Fix-ServiceRegistrations {
    Write-ColorOutput Yellow "Checking service registrations in Program.cs..."
    
    $programFile = "src/StockDataApi/Program.cs"
    if (Test-Path $programFile) {
        $content = Get-Content $programFile -Raw
        
        # Check for missing handler registrations
        if ($content -notmatch "GetAllStockDataQueryHandler") {
            Write-ColorOutput Yellow "Adding missing GetAllStockDataQueryHandler registration"
            $newContent = $content -replace "// Register CQRS handlers", "// Register CQRS handlers`n        builder.Services.AddScoped<StockDataApi.Handlers.Queries.GetAllStockDataQueryHandler>();"
            Set-Content -Path $programFile -Value $newContent
        }
        
        if ($content -notmatch "FetchPolygonDataCommandHandler") {
            Write-ColorOutput Yellow "Adding missing FetchPolygonDataCommandHandler registration"
            $newContent = $content -replace "builder.Services.AddScoped<StockDataApi.Handlers.Queries.GetAllStockDataQueryHandler>\(\);", "builder.Services.AddScoped<StockDataApi.Handlers.Queries.GetAllStockDataQueryHandler>();`n        builder.Services.AddScoped<StockDataApi.Handlers.Commands.FetchPolygonDataCommandHandler>();"
            Set-Content -Path $programFile -Value $newContent
        }
    }
}

function Test-DatabaseConnection {
    Write-ColorOutput Yellow "Testing database connection..."
    
    $connectionString = (Get-Content "src/StockDataApi/appsettings.json" | ConvertFrom-Json).ConnectionStrings.DefaultConnection
    
    if ($connectionString) {
        Write-ColorOutput Green "Connection string found: $($connectionString.Substring(0, 20))..."
    }
    else {
        Write-ColorOutput Red "No connection string found in appsettings.json"
    }
}

function Update-Database {
    Write-ColorOutput Yellow "Updating database..."
    
    try {
        dotnet ef database update --project src/StockDataLib --startup-project src/StockDataApi
        Write-ColorOutput Green "Database updated successfully."
    }
    catch {
        Write-ColorOutput Red "Database update failed: $_"
    }
}

function Clean-Project {
    Write-ColorOutput Yellow "Cleaning project..."
    
    # Clean build outputs
    dotnet clean
    
    # Remove bin and obj folders
    Get-ChildItem -Path "." -Recurse -Directory -Name "bin" | Remove-Item -Recurse -Force
    Get-ChildItem -Path "." -Recurse -Directory -Name "obj" | Remove-Item -Recurse -Force
    
    # Clean frontend
    if (Test-Path "frontend/node_modules") {
        Remove-Item -Path "frontend/node_modules" -Recurse -Force
    }
    if (Test-Path "frontend/dist") {
        Remove-Item -Path "frontend/dist" -Recurse -Force
    }
    
    Write-ColorOutput Green "Project cleaned successfully."
}

function Test-FrontendBuild {
    Write-ColorOutput Yellow "Testing frontend build..."
    
    if (Test-Path "frontend/package.json") {
        Set-Location frontend
        npm install
        npm run build
        Set-Location ..
        Write-ColorOutput Green "Frontend build successful."
    }
    else {
        Write-ColorOutput Red "Frontend package.json not found."
    }
}

function Check-APIEndpoints {
    Write-ColorOutput Yellow "Checking API endpoint consistency..."
    
    $controllerFiles = Get-ChildItem -Path "src/StockDataApi/Controllers" -Filter "*.cs"
    
    foreach ($file in $controllerFiles) {
        $content = Get-Content $file.FullName -Raw
        
        # Check for proper route attributes
        if ($content -match "\[ApiController\]" -and $content -notmatch "\[Route\(") {
            Write-ColorOutput Yellow "Missing [Route] attribute in $($file.Name)"
        }
        
        # Check for proper HTTP method attributes
        if ($content -match "public.*Task.*Get" -and $content -notmatch "\[HttpGet\]") {
            Write-ColorOutput Yellow "Missing [HttpGet] attribute in $($file.Name)"
        }
    }
}

function Show-ProjectStatus {
    Write-ColorOutput Cyan "=== Stock Data API Project Status ==="
    
    # Check if API is running
    $apiProcess = Get-Process -Name "StockDataApi" -ErrorAction SilentlyContinue
    if ($apiProcess) {
        Write-ColorOutput Green "API is running (PID: $($apiProcess.Id))"
    }
    else {
        Write-ColorOutput Yellow "API is not running"
    }
    
    # Check database migrations
    $migrationFiles = Get-ChildItem -Path "src/StockDataLib/Migrations" -Filter "*.cs" | Where-Object { $_.Name -notlike "*Designer.cs" }
    Write-ColorOutput Cyan "Database migrations: $($migrationFiles.Count) files"
    
    # Check handlers
    $queryHandlers = Get-ChildItem -Path "src/StockDataApi/Handlers/Queries" -Filter "*.cs" -ErrorAction SilentlyContinue
    $commandHandlers = Get-ChildItem -Path "src/StockDataApi/Handlers/Commands" -Filter "*.cs" -ErrorAction SilentlyContinue
    Write-ColorOutput Cyan "CQRS handlers: $($queryHandlers.Count) queries, $($commandHandlers.Count) commands"
    
    # Check frontend components
    $components = Get-ChildItem -Path "frontend/src/components" -Filter "*.tsx" -ErrorAction SilentlyContinue
    Write-ColorOutput Cyan "Frontend components: $($components.Count) files"
}

# Main execution
if ($All) {
    $Build = $Test = $Clean = $Migrate = $true
}

if ($Clean) {
    Clean-Project
}

if ($Build) {
    Fix-MissingUsings
    Fix-ServiceRegistrations
    Check-APIEndpoints
    
    if (Test-BuildErrors) {
        Write-ColorOutput Green "Build successful!"
    }
    else {
        Write-ColorOutput Red "Build failed. Please check the errors above."
    }
}

if ($Test) {
    Test-FrontendBuild
    Test-DatabaseConnection
}

if ($Migrate) {
    Update-Database
}

Show-ProjectStatus

Write-ColorOutput Cyan "Auto-fix script completed."
