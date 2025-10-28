# ChartExchange API Integration Documentation

## Overview

This document describes the ChartExchange API integration that has replaced the previous Polygon.io integration. The ChartExchange integration provides access to comprehensive stock market data including price data, failure to deliver information, Reddit mentions, option chain summaries, and stock splits.

## Architecture

The ChartExchange integration follows a CQRS (Command Query Responsibility Segregation) pattern with the following components:

### Backend Components

1. **Models** (`src/StockDataLib/Models/ChartExchangeModels.cs`)
   - `ChartExchangePrice` - Price data (OHLCV)
   - `ChartExchangeFailureToDeliver` - Failure to deliver data
   - `ChartExchangeRedditMentions` - Social media sentiment data
   - `ChartExchangeOptionChain` - Options data
   - `ChartExchangeStockSplit` - Stock split history

2. **Service** (`src/StockDataLib/Services/ChartExchangeService.cs`)
   - `IChartExchangeService` interface
   - `ChartExchangeService` implementation
   - HTTP client factory integration
   - Rate limiting support

3. **Controllers** (`src/StockDataApi/Controllers/ChartExchangeController.cs`)
   - RESTful API endpoints for all data types
   - Caching support
   - Error handling

4. **CQRS Handlers**
   - `FetchChartExchangeDataCommandHandler` - Command handler for data fetching
   - `GetAllStockDataQueryHandler` - Query handler for data retrieval

5. **Database Context** (`src/StockDataLib/Data/StockDataContext.cs`)
   - Entity Framework Core DbSets
   - Navigation properties
   - Model configurations

### Frontend Components

1. **Chart Components**
   - `FailureToDeliverChart.tsx` - Failure to deliver visualization
   - `RedditMentionsChart.tsx` - Social sentiment visualization
   - `OptionChainChart.tsx` - Options data visualization
   - `StockSplitChart.tsx` - Stock split history visualization

2. **Dashboard Integration** (`frontend/src/components/Dashboard.tsx`)
   - Updated to use ChartExchange endpoints
   - Client-side date filtering
   - Real-time data fetching

## API Endpoints

### ChartExchange Controller Endpoints

#### Price Data
```
GET /api/ChartExchange/price/{symbol}
```
Returns all price data for a specific ticker.

#### Failure to Deliver Data
```
GET /api/ChartExchange/failure-to-deliver/{symbol}
```
Returns failure to deliver data for a specific ticker.

#### Reddit Mentions Data
```
GET /api/ChartExchange/reddit-mentions/{symbol}
```
Returns Reddit mentions and sentiment data for a specific ticker.

#### Option Chain Data
```
GET /api/ChartExchange/option-chain/{symbol}
```
Returns option chain summaries for a specific ticker.

#### Stock Splits Data
```
GET /api/ChartExchange/stock-splits/{symbol}
```
Returns stock split history for a specific ticker.

#### Short Interest Data
```
GET /api/ChartExchange/short-interest/{symbol}
```
Returns short interest data for a specific ticker.

#### Short Volume Data
```
GET /api/ChartExchange/short-volume/{symbol}
```
Returns short volume data for a specific ticker.

#### Borrow Fee Data
```
GET /api/ChartExchange/borrow-fee/{symbol}
```
Returns borrow fee data for a specific ticker.

#### Fetch Data
```
POST /api/ChartExchange/{symbol}/fetch?years=2
```
Fetches and stores ChartExchange data for a symbol (default: 2 years).

### Unified Stock Data Endpoint

#### Get All Stock Data
```
GET /api/StockData/{symbol}?includeChartExchange=true&includeBorrowFee=false
```
Returns unified stock data including ChartExchange data.

#### Fetch ChartExchange Data
```
POST /api/StockData/{symbol}/fetch-chartexchange
```
Fetches all ChartExchange data types for a symbol.

## Data Models

### ChartExchangePrice
```csharp
public class ChartExchangePrice : StockDataPoint
{
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public decimal? AdjustedClose { get; set; }
    public decimal? DividendAmount { get; set; }
    public decimal? SplitCoefficient { get; set; }
}
```

### ChartExchangeFailureToDeliver
```csharp
public class ChartExchangeFailureToDeliver : StockDataPoint
{
    public long FailureToDeliver { get; set; }
    public decimal Price { get; set; }
    public long Volume { get; set; }
    public DateTime? SettlementDate { get; set; }
    public string? Cusip { get; set; }
    public string? CompanyName { get; set; }
}
```

### ChartExchangeRedditMentions
```csharp
public class ChartExchangeRedditMentions : StockDataPoint
{
    public int Mentions { get; set; }
    public decimal? SentimentScore { get; set; }
    public string? SentimentLabel { get; set; }
    public string? Subreddit { get; set; }
    public int? Upvotes { get; set; }
    public int? Comments { get; set; }
    public decimal? EngagementScore { get; set; }
}
```

### ChartExchangeOptionChain
```csharp
public class ChartExchangeOptionChain : StockDataPoint
{
    public string ExpirationDate { get; set; } = string.Empty;
    public decimal StrikePrice { get; set; }
    public string OptionType { get; set; } = string.Empty; // Call or Put
    public long Volume { get; set; }
    public long OpenInterest { get; set; }
    public decimal? Bid { get; set; }
    public decimal? Ask { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? ImpliedVolatility { get; set; }
    public decimal? Delta { get; set; }
    public decimal? Gamma { get; set; }
    public decimal? Theta { get; set; }
    public decimal? Vega { get; set; }
}
```

### ChartExchangeStockSplit
```csharp
public class ChartExchangeStockSplit : StockDataPoint
{
    public string SplitRatio { get; set; } = string.Empty; // e.g., "2:1"
    public decimal SplitFactor { get; set; } // e.g., 2.0
    public decimal FromFactor { get; set; }
    public decimal ToFactor { get; set; }
    public DateTime? ExDate { get; set; }
    public DateTime? RecordDate { get; set; }
    public DateTime? PayableDate { get; set; }
    public DateTime? AnnouncementDate { get; set; }
    public string? CompanyName { get; set; }
}
```

## Configuration

### appsettings.json
```json
{
  "ChartExchange": {
    "ApiKey": "your-chartexchange-api-key-here",
    "BaseUrl": "https://api.chartexchange.com",
    "RateLimitPerMinute": 60
  }
}
```

### Service Registration
```csharp
// Add HTTP client for ChartExchange
builder.Services.AddHttpClient("ChartExchange");

// Register services
builder.Services.AddScoped<IChartExchangeService, ChartExchangeService>();

// Configure ChartExchange options
builder.Services.Configure<ChartExchangeOptions>(
    builder.Configuration.GetSection("ChartExchange"));
```

## Database Migration

The migration `ReplacePolygonWithChartExchange` removes all Polygon-related tables and creates new ChartExchange tables:

- Removes: `PolygonPriceData`, `PolygonShortInterestData`, `PolygonShortVolumeData`
- Adds: `ChartExchangePrice`, `ChartExchangeFailureToDeliver`, `ChartExchangeRedditMentions`, `ChartExchangeOptionChain`, `ChartExchangeStockSplit`

## Frontend Integration

### Dashboard Updates
The main dashboard has been updated to:
- Use ChartExchange endpoints instead of Polygon
- Display ChartExchange data in dedicated chart sections
- Support client-side date filtering for all data types
- Provide action buttons for fetching ChartExchange data

### New Chart Components
Four new specialized chart components have been created:
1. **FailureToDeliverChart** - Multi-axis chart showing failure to deliver, price, and volume
2. **RedditMentionsChart** - Sentiment analysis with mentions count and engagement metrics
3. **OptionChainChart** - Call/put volume and open interest visualization
4. **StockSplitChart** - Historical stock split data with key dates

## Usage Examples

### Fetching ChartExchange Data
```javascript
// Fetch all ChartExchange data for AAPL
const response = await fetch('/api/StockData/AAPL/fetch-chartexchange', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' }
});

const result = await response.json();
console.log(result);
```

### Getting Specific Data Types
```javascript
// Get failure to deliver data
const failureData = await fetch('/api/ChartExchange/failure-to-deliver/AAPL');
const failureResult = await failureData.json();

// Get Reddit mentions
const redditData = await fetch('/api/ChartExchange/reddit-mentions/AAPL');
const redditResult = await redditData.json();
```

## Error Handling

All endpoints include comprehensive error handling:
- Input validation
- Database connection errors
- API rate limiting
- Data not found scenarios
- Malformed requests

## Performance Considerations

- **Caching**: All endpoints use 15-minute memory caching
- **Rate Limiting**: Configurable rate limiting per minute
- **Client-side Filtering**: Reduces server load by filtering data on the frontend
- **Async Operations**: All database operations are asynchronous

## Security

- API keys are stored in configuration files
- Sensitive values are masked in logs
- Input validation prevents injection attacks
- CORS policies are properly configured

## Monitoring and Logging

- Comprehensive logging at all levels
- Performance metrics tracking
- Error rate monitoring
- API usage statistics

## Migration from Polygon

The migration from Polygon.io to ChartExchange includes:
1. ✅ Data model updates
2. ✅ Service layer replacement
3. ✅ Controller updates
4. ✅ Database migration
5. ✅ Frontend component updates
6. ✅ Configuration updates
7. ✅ CQRS handler updates
8. ✅ Documentation updates

## Future Enhancements

Potential future enhancements:
- Real-time data streaming
- Additional data sources
- Advanced analytics
- Machine learning integration
- Mobile app support
- API versioning

## Support

For issues or questions regarding the ChartExchange integration:
1. Check the logs for detailed error messages
2. Verify API key configuration
3. Ensure database migrations are applied
4. Test individual endpoints for debugging
5. Review the configuration settings

---

*Last updated: $(Get-Date)*

