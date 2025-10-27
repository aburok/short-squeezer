# Polygon.io Models - Detailed Plan

## Existing Models Analysis

You already have:
- ✅ `PriceData` - Has Open, High, Low, Close
- ✅ `VolumeData` - Has Volume
- ✅ `StockDataPoint` - Base class with Id, Symbol, Date

## New Models to Add

### Option 1: Separate Polygon Models (Recommended)

Create **3 new classes** in `PolygonModels.cs`:

#### 1. **PolygonBarData** (API Response)
```csharp
public class PolygonBarData
{
    public long T { get; set; }  // Unix timestamp in ms
    public decimal O { get; set; } // Open
    public decimal H { get; set; } // High
    public decimal L { get; set; } // Low
    public decimal C { get; set; } // Close
    public long V { get; set; }    // Volume
    public decimal? Vw { get; set; } // Volume-weighted avg price (optional)
    public int? N { get; set; }     // Number of transactions (optional)
}
```

#### 2. **PolygonDailyBarResponse** (API Response Wrapper)
```csharp
public class PolygonDailyBarResponse
{
    public string Ticker { get; set; }
    public int ResultsCount { get; set; }
    public List<PolygonBarData> Results { get; set; }
    public string Status { get; set; }
    public string RequestId { get; set; }
    public int Count { get; set; }
}
```

#### 3. **PolygonPriceData** (Database Entity)
```csharp
public class PolygonPriceData : StockDataPoint
{
    // OHLC data (same as PriceData but separate table)
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    
    // Additional Polygon-specific fields
    public decimal? VolumeWeightedPrice { get; set; }
    public int? NumberOfTransactions { get; set; }
    public string? PolygonRequestId { get; set; } // For tracking
}
```

#### Navigation Property Addition:
Add to `StockTicker` in `StockData.cs`:
```csharp
public ICollection<PolygonPriceData> PolygonPriceData { get; set; }
```

### Option 2: Reuse Existing Models (Simpler)

Reuse existing `PriceData` and `VolumeData` tables:

**Pros:**
- ✅ Less code
- ✅ Fewer tables
- ✅ Consolidates price data

**Cons:**
- ❌ Can't track which data source (Polygon vs Alpha Vantage)
- ❌ Loses Polygon-specific metadata
- ❌ Mixes different API formats

**Models Needed:** Just API response models
```csharp
// For parsing API response only
public class PolygonBarData { ... }
public class PolygonDailyBarResponse { ... }
```

## Recommendation: Option 1 (Separate Models)

### Why separate models:
1. ✅ **Data source tracking** - Know where data came from
2. ✅ **Polygon-specific fields** - Volume-weighted price, transaction count
3. ✅ **Clean separation** - Polygon data in own table
4. ✅ **Easier queries** - Query Polygon data separately
5. ✅ **Metadata preservation** - Request IDs and additional fields

### Database Impact:

**New Table:** `PolygonPriceData`
```
- Id (PK, int)
- StockTickerSymbol (FK, string)
- Date (DateTime)
- Open (decimal)
- High (decimal)
- Low (decimal)
- Close (decimal)
- Volume (bigint)
- VolumeWeightedPrice (decimal?, nullable)
- NumberOfTransactions (int?, nullable)
- PolygonRequestId (string?, nullable)
```

**Relationship:**
- One StockTicker → Many PolygonPriceData records
- Foreign key on StockTickerSymbol

## Complete Model Structure

### File: `src/StockDataLib/Models/PolygonModels.cs`
```csharp
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StockDataLib.Models
{
    /// <summary>
    /// Represents a single bar (OHLCV) from Polygon.io API response
    /// </summary>
    public class PolygonBarData
    {
        [JsonProperty("t")]
        public long Timestamp { get; set; } // Unix timestamp in milliseconds

        [JsonProperty("o")]
        public decimal Open { get; set; }

        [JsonProperty("h")]
        public decimal High { get; set; }

        [JsonProperty("l")]
        public decimal Low { get; set; }

        [JsonProperty("c")]
        public decimal Close { get; set; }

        [JsonProperty("v")]
        public long Volume { get; set; }

        [JsonProperty("vw")]
        public decimal? VolumeWeightedPrice { get; set; }

        [JsonProperty("n")]
        public int? NumberOfTransactions { get; set; }

        /// <summary>
        /// Converts Unix timestamp to DateTime
        /// </summary>
        public DateTime GetDateTime()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).UtcDateTime;
        }
    }

    /// <summary>
    /// Represents the full response from Polygon.io aggregates API
    /// </summary>
    public class PolygonDailyBarResponse
    {
        [JsonProperty("ticker")]
        public string? Ticker { get; set; }

        [JsonProperty("resultsCount")]
        public int ResultsCount { get; set; }

        [JsonProperty("results")]
        public List<PolygonBarData> Results { get; set; } = new List<PolygonBarData>();

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("request_id")]
        public string? RequestId { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    /// <summary>
    /// Represents Polygon.io price data stored in the database
    /// </summary>
    public class PolygonPriceData : StockDataPoint
    {
        // OHLCV data
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }

        // Additional Polygon-specific fields
        public decimal? VolumeWeightedPrice { get; set; }
        public int? NumberOfTransactions { get; set; }
        public string? PolygonRequestId { get; set; }
    }

    /// <summary>
    /// Represents ticker information from Polygon.io
    /// </summary>
    public class PolygonTickerInfo
    {
        public string Ticker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Active { get; set; }
        public string Market { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
        public string PrimaryExchange { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public DateTime? ListDate { get; set; }
    }
}
```

### Modification: `src/StockDataLib/Models/StockData.cs`
Add to `StockTicker` class:
```csharp
public ICollection<PolygonPriceData> PolygonPriceData { get; set; }
```

## Summary

**New Models (3 classes):**
1. `PolygonBarData` - API response parsing
2. `PolygonDailyBarResponse` - API wrapper
3. `PolygonPriceData` - Database entity

**Modified:**
- `StockTicker` - Add navigation property

**Why separate from PriceData?**
- Tracks data source (Polygon vs others)
- Stores Polygon-specific metadata
- Clean separation of concerns
- Easy to query by source

This keeps your data organized and traceable!
