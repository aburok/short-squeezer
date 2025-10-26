# Test script for Stock Data API

# Base URL for the API
$baseUrl = "https://localhost:5001/api"

# Function to make API requests and display results
function Invoke-ApiRequest {
    param (
        [string]$endpoint,
        [string]$description
    )
    
    Write-Host "`n=== $description ===" -ForegroundColor Cyan
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/$endpoint" -Method Get -ContentType "application/json"
        Write-Host "Status: Success" -ForegroundColor Green
        
        # Format and display the response
        $jsonResponse = $response | ConvertTo-Json -Depth 4
        Write-Host "Response:`n$jsonResponse"
    }
    catch {
        Write-Host "Status: Error" -ForegroundColor Red
        Write-Host "Error: $_"
    }
}

# Test the Tickers endpoint
Invoke-ApiRequest -endpoint "Tickers" -description "Get all tickers"

# Test getting a specific ticker
Invoke-ApiRequest -endpoint "Tickers/AAPL" -description "Get AAPL ticker"

# Test the BorrowFee endpoint
Invoke-ApiRequest -endpoint "BorrowFee/GME" -description "Get GME borrow fee data"

# Test the latest borrow fee endpoint
Invoke-ApiRequest -endpoint "BorrowFee/GME/latest" -description "Get latest GME borrow fee"

# Test the Price endpoint
Invoke-ApiRequest -endpoint "Price/AAPL" -description "Get AAPL price data"

# Test the latest price endpoint
Invoke-ApiRequest -endpoint "Price/AAPL/latest" -description "Get latest AAPL price"

# Test with date filters
$startDate = [DateTime]::Now.AddMonths(-1).ToString("yyyy-MM-dd")
$endDate = [DateTime]::Now.ToString("yyyy-MM-dd")
Invoke-ApiRequest -endpoint "Price/AAPL?startDate=$startDate&endDate=$endDate" -description "Get AAPL price data with date range"

Write-Host "`nAPI testing completed." -ForegroundColor Green

# Made with Bob
