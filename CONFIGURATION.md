# Stock Data API - Configuration Guide

## Configuration Sources

The Stock Data API supports multiple configuration sources in the following order of precedence (highest to lowest):

1. **Environment Variables** (highest precedence)
2. **Command Line Arguments**
3. **appsettings.{Environment}.json**
4. **appsettings.json** (lowest precedence)

## Environment Variables

### Database Configuration
```bash
# Override database connection
set ConnectionStrings__DefaultConnection="Server=localhost;Database=MyStockDb;Trusted_Connection=True"

# Or use environment-specific format
set STOCKDATA_DB_SERVER=localhost
set STOCKDATA_DB_NAME=MyStockDb
```

### API Keys
```bash
# Override Alpha Vantage API key
set AlphaVantage__ApiKey="your-real-api-key-here"
```

### Application Settings
```bash
# Set environment
set ASPNETCORE_ENVIRONMENT=Production

# Set URLs
set ASPNETCORE_URLS="http://localhost:5000;https://localhost:5001"
```

## Configuration Files

### appsettings.json (Base Configuration)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StockDataDb;Trusted_Connection=True"
  },
  "AlphaVantage": {
    "ApiKey": "demo"
  }
}
```

### appsettings.Development.json (Development Overrides)
```json
{
  "Logging": {
    "LogLevel": {
      "StockDataApi": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StockDataDb_Dev;Trusted_Connection=True"
  }
}
```

### appsettings.Production.json (Production Overrides)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=StockDataDb_Prod;User Id=prod_user;Password=***"
  }
}
```

## Docker Environment Variables

When using Docker, you can set environment variables in your docker-compose.yml:

```yaml
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=StockDataDb;User Id=sa;Password=StrongPassword123!;TrustServerCertificate=True;
      - AlphaVantage__ApiKey=your-production-api-key
```

## Configuration Debug Output

The application displays comprehensive configuration information on startup:

- 📄 **AppSettings Configuration**: Shows all configuration sections
- 🗄️ **Database Configuration**: Shows connection strings (masked)
- 🌍 **Environment Variables**: Shows relevant environment variables
- 📋 **Configuration Sources**: Shows the order of configuration sources
- 🌐 **Server URLs**: Shows available endpoints
- ⚙️ **Registered Services**: Shows all registered services

## Security Notes

- Sensitive values (passwords, API keys) are automatically masked in debug output
- Use environment variables for production secrets
- Never commit real API keys or passwords to source control
- Use Azure Key Vault or similar services for production secret management

## Examples

### Local Development
```bash
# Set development environment
set ASPNETCORE_ENVIRONMENT=Development

# Run the application
dotnet run --project src/StockDataApi
```

### Production with Environment Variables
```bash
# Set production environment
set ASPNETCORE_ENVIRONMENT=Production

# Override database connection
set ConnectionStrings__DefaultConnection="Server=prod-server;Database=StockDataDb;User Id=prod_user;Password=***"

# Override API key
set AlphaVantage__ApiKey="your-production-api-key"

# Run the application
dotnet run --project src/StockDataApi
```

### Docker Compose
```bash
# Start with Docker Compose (uses environment variables from docker-compose.yml)
docker-compose up
```


