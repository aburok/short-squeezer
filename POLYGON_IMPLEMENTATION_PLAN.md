# Polygon.io Integration - Implementation Plan

## Overview
Implement Polygon.io API integration to fetch and store 2 years of historical stock data with single-symbol fetch capability.

## Architecture

### 1. **Data Flow**
```
UI Button Click
    ↓
API Controller (/api/Polygon/{symbol}/fetch)
    ↓
PolygonService.GetHistoricalDataAsync(symbol, years=2)
    ↓
HTTP Request to Polygon.io API
    ↓
Parse Response → Store in Database
    ↓
Return Success/Count to UI
```

### 2. **Components to Create/Modify**

#### **A. Service Layer** (New)
- `src/StockDataLib/Services/PolygonService.cs`
- `IPolygonService` interface
- Methods:
  - `GetHistoricalDataAsync(string symbol, int years = 2)`
  - `GetDailyBarsAsync(string symbol, DateTime startDate, DateTime endDate)`
  - `GetTickerDetailsAsync(string symbol)`
- Rate limiting: 5 calls/min with delays
- Error handling and logging

#### **B. Database Models** (New)
- `src/StockDataLib/Models/PolygonModels.cs`
- `PolygonDailyBarData` - API response model
- `PolygonPriceData` - Database entity (extends StockDataPoint)
- Navigation property in `StockTicker`

#### **C. Database Context** (Modify)
- Add `DbSet<PolygonPriceData>` to `StockDataContext.cs`
- Configure relationships in `OnModelCreating`

#### **D. Configuration** (Modify)
- Add `Polygon` section to both `appsettings.json` files
- API Key and Base URL configuration
- Optional: Rate limit settings

#### **E. API Controller** (New)
- `src/StockDataApi/Controllers/PolygonController.cs`
- Endpoints:
  - `POST /api/Polygon/{symbol}/fetch` - Fetch and store 2 years
  - `GET /api/Polygon/{symbol}` - Get stored data
  - `GET /api/Polygon/{symbol}/latest` - Get latest point
  - `GET /api/Polygon/{symbol}/info` - Get ticker info

#### **F. Service Registration** (Modify)
- Register service in `Program.cs` (API and Worker)
- Configure options from configuration

#### **G. UI Integration** (Modify)
- Add "Fetch Polygon Data" button to Dashboard
- Loading state management
- Success/error messaging
- Optional: Progress indicator for long fetches

## Implementation Steps

### Step 1: Create Models
**File:** `src/StockDataLib/Models/PolygonModels.cs`

**Models Needed:**
1. `PolygonDailyBarResponse` - API response wrapper
2. `PolygonBarData` - Individual bar (OHLCV + timestamp)
3. `PolygonPriceData` - Database entity
4. `PolygonTickerInfo` - Company information

### Step 2: Create Service
**File:** `src/StockDataLib/Services/PolygonService.cs`

**Features:**
- HTTP client with rate limiting
- Get 2 years of daily bars
- Automatically handle rate limit (5 calls/min)
- Parse timestamps and convert to DateTime
- Error handling with retries
- Logging

**Key Methods:**
```csharp
Task<List<PolygonPriceData>> GetHistoricalDataAsync(string symbol, int years = 2)
Task<PolygonDailyBarResponse> GetDailyBarsAsync(string symbol, DateTime start, DateTime end)
Task<PolygonTickerInfo> GetTickerDetailsAsync(string symbol)
```

**Rate Limiting Logic:**
```csharp
// Wait 12 seconds between API calls to respect 5 calls/min limit
await Task.Delay(TimeSpan.FromSeconds(12));
```

### Step 3: Database Migration
**Action:** Update `StockDataContext.cs`

Add:
- `DbSet<PolygonPriceData> PolygonPriceData { get; set; }`
- Configure in `OnModelCreating`
- Navigation property relationship

**Migration Command:**
```bash
dotnet ef migrations add AddPolygonPriceData --project src/StockDataLib --startup-project src/StockDataApi
dotnet ef database update --project src/StockDataLib --startup-project src/StockDataApi
```

### Step 4: Configuration
**Files:** `src/StockDataApi/appsettings.json` and `src/StockDataWorker/appsettings.json`

Add:
```json
{
  "Polygon": {
    "ApiKey": "YOUR_POLYGON_API_KEY",
    "BaseUrl": "https://api.polygon.io",
    "RateLimitCallsPerMinute": 5,
    "DelayBetweenCallsSeconds": 12
  }
}
```

### Step 5: API Controller
**File:** `src/StockDataApi/Controllers/PolygonController.cs`

**Endpoints:**
1. **Fetch Data:**
   - `POST /api/Polygon/AAPL/fetch?years=2`
   - Calls service to fetch
   - Stores in database
   - Returns: `{ success: true, count: 252, message: "..." }`

2. **Get Stored Data:**
   - `GET /api/Polygon/AAPL?startDate=2024-01-01&endDate=2024-12-31`
   - Query database
   - Return stored bars

3. **Get Latest:**
   - `GET /api/Polygon/AAPL/latest`
   - Most recent data point

4. **Get Info:**
   - `GET /api/Polygon/AAPL/info`
   - Company details

### Step 6: Service Registration
**Files:** `src/StockDataApi/Program.cs` and `src/StockDataWorker/Program.cs`

```csharp
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.Configure<PolygonOptions>(configuration.GetSection("Polygon"));
```

### Step 7: UI Integration
**File:** `frontend/src/components/Dashboard.tsx`

**Add:**
- `isFetchingPolygon` state
- `handleFetchPolygonData()` function
- "Fetch Polygon Data (2 Years)" button
- Loading indicator
- Success/error toast

**Button Code:**
```tsx
<button 
  onClick={handleFetchPolygonData}
  disabled={isFetchingPolygon}
>
  {isFetchingPolygon ? 'Fetching...' : 'Fetch Polygon Data (2 Years)'}
</button>
```

## Technical Details

### API Endpoint to Call
```
GET https://api.polygon.io/v2/aggs/ticker/AAPL/range/1/day/2022-12-31/2024-12-31
```

**Request Headers:**
```
Authorization: Bearer YOUR_API_KEY
Accept: application/json
```

**Response Format:**
```json
{
  "resultsCount": 252,
  "results": [
    {
      "t": 1704153600000,  // Unix timestamp in ms
      "o": 150.0,           // Open
      "h": 152.5,           // High
      "l": 149.5,           // Low
      "c": 151.2,           // Close
      "v": 50000000,        // Volume
      "vw": 150.5,          // Volume-weighted price
      "n": 25000            // Number of transactions
    }
  ],
  "status": "OK",
  "request_id": "...",
  "count": 252
}
```

### Database Schema

**Table:** `PolygonPriceData`
```
Id (PK)
StockTickerSymbol (FK to StockTicker)
Date (DateTime)
Open (decimal)
High (decimal)
Low (decimal)
Close (decimal)
Volume (bigint)
NumberOfTransactions (bigint, nullable)
VolumeWeightedPrice (decimal, nullable)
```

### Rate Limiting Strategy

**Polygon Free Tier:**
- 5 API calls per minute
- Need to wait 12 seconds between calls
- For 2 years of data: Can get up to 730 days in one call
- Single call is sufficient for 2 years of daily data

**Implementation:**
```csharp
// Fetch 2 years in one call (within limit)
var startDate = DateTime.Now.AddYears(-2);
var endDate = DateTime.Now;

// Only one API call needed for 2 years
var response = await GetDailyBarsAsync(symbol, startDate, endDate);
```

### Error Handling

**Possible Errors:**
1. Invalid API key → 401 Unauthorized
2. Invalid symbol → 404 Not Found
3. Rate limit exceeded → 429 Too Many Requests
4. Network error → Retry with backoff
5. Database error → Log and return error

**Handling Strategy:**
- Try-catch blocks
- Retry logic (3 attempts)
- Exponential backoff
- Detailed logging
- User-friendly error messages

### Testing Plan

1. **Unit Tests** (Future):
   - Service layer tests
   - Model serialization
   - Rate limiting logic

2. **Integration Tests:**
   - API endpoint tests
   - Database storage tests
   - UI interaction tests

3. **Manual Testing:**
   - Fetch data for AAPL
   - Verify database storage
   - Check UI response
   - Test error cases

## Implementation Order

1. ✅ Create models (`PolygonModels.cs`)
2. ✅ Create service (`PolygonService.cs`)
3. ✅ Update database context and migration
4. ✅ Add configuration
5. ✅ Register services
6. ✅ Create API controller
7. ✅ Add UI integration
8. ✅ Test end-to-end

## Estimated Time

- Models: 15 minutes
- Service: 30 minutes
- Database: 15 minutes
- Configuration: 10 minutes
- Controller: 20 minutes
- UI: 20 minutes
- Testing: 20 minutes
- **Total: ~2 hours**

## Success Criteria

- ✅ Can fetch 2 years of data for any symbol
- ✅ Data stored correctly in database
- ✅ API endpoints return correct data
- ✅ UI button works and shows feedback
- ✅ Rate limiting respected (no errors)
- ✅ Error handling works gracefully
- ✅ Logging provides useful information

## Next Steps

After implementation:
1. Get Polygon.io API key from https://polygon.io
2. Update `appsettings.json` with API key
3. Run database migration
4. Test with real API call
5. Monitor rate limits
6. Optionally upgrade to paid tier if needed

## Files Summary

**New Files:**
- `src/StockDataLib/Models/PolygonModels.cs`
- `src/StockDataLib/Services/PolygonService.cs`
- `src/StockDataApi/Controllers/PolygonController.cs`

**Modified Files:**
- `src/StockDataLib/Data/StockDataContext.cs`
- `src/StockDataApi/Program.cs`
- `src/StockDataWorker/Program.cs`
- `src/StockDataApi/appsettings.json`
- `src/StockDataWorker/appsettings.json`
- `frontend/src/components/Dashboard.tsx`
- (New migration file)

## Dependencies

**NuGet Packages:**
- Already have: `System.Text.Json` or `Newtonsoft.Json`
- Already have: `Microsoft.Extensions.Http`
- Already have: `Microsoft.Extensions.Options`

**Frontend:**
- Already have: React, fetch API
- No new packages needed

Ready to implement! This plan provides a complete Polygon.io integration with historical data fetching and storage.
