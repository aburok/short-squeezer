# Stock Data API

A .NET service for fetching and serving stock market data including price data and borrow fees.

## Projects

The solution consists of three projects:

1. **StockDataApi**: ASP.NET Core Web API that serves stock data via REST endpoints
2. **StockDataWorker**: Background service that periodically fetches stock data from external sources
3. **StockDataLib**: Shared library containing models, data context, and services

## Features

- Fetch stock price data from Alpha Vantage API
- Fetch borrow fee data from Chart Exchange
- Store data in SQL Server database
- Serve data via REST API endpoints
- Caching for improved performance
- Docker support for easy deployment

## API Endpoints

### Tickers

- `GET /api/Tickers`: Get all available stock tickers
- `GET /api/Tickers/{symbol}`: Get information about a specific ticker
- `POST /api/Tickers`: Add a new ticker
- `DELETE /api/Tickers/{symbol}`: Delete a ticker

### Price Data

- `GET /api/Price/{symbol}`: Get price data for a specific ticker
- `GET /api/Price/{symbol}/latest`: Get the latest price for a specific ticker
- `GET /api/Price/{symbol}?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd`: Get price data for a specific date range

### Borrow Fee Data

- `GET /api/BorrowFee/{symbol}`: Get borrow fee data for a specific ticker
- `GET /api/BorrowFee/{symbol}/latest`: Get the latest borrow fee for a specific ticker
- `GET /api/BorrowFee/{symbol}?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd`: Get borrow fee data for a specific date range

## Configuration

### API Keys

To use the Alpha Vantage API, you need to obtain an API key from [Alpha Vantage](https://www.alphavantage.co/support/#api-key) and add it to the configuration:

```json
"AlphaVantage": {
  "ApiKey": "your-api-key-here"
}
```

### Database

The application uses SQL Server by default. Update the connection string in appsettings.json:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server;Database=StockDataDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

## Running the Application

### Using .NET CLI

1. Ensure you have .NET 9.0 SDK installed
2. Update the connection string in appsettings.json
3. Run the database migrations: `dotnet ef database update`
4. Start the API: `dotnet run --project src/StockDataApi/StockDataApi.csproj`
5. Start the worker service: `dotnet run --project src/StockDataWorker/StockDataWorker.csproj`

### Using Docker

The application can be run using Docker and Docker Compose:

1. Ensure you have Docker and Docker Compose installed
2. Build and start the containers:
   ```
   docker-compose up -d
   ```
3. The API will be available at http://localhost:5000
4. To stop the containers:
   ```
   docker-compose down
   ```

#### Docker Components

- **SQL Server**: Microsoft SQL Server 2022 Express
- **API**: ASP.NET Core Web API
- **Worker**: Background service for data fetching

## Testing

Use the included PowerShell script to test the API endpoints:

```
./test-api.ps1
```

When using Docker, update the base URL in the test script to:
```powershell
$baseUrl = "http://localhost:5000/api"# share-squeezer
# short-squeezer
