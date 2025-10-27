# Polygon.io Free Tier - Available Data Guide

## Overview
Polygon.io offers a **generous free tier** that provides access to comprehensive market data for U.S. equities, forex, and cryptocurrencies.

## What You Can Get on the Free Plan

### 1. **Stock Data**
- ✅ **End-of-Day Data** - Daily closing prices
- ✅ **Historical Bars** - Up to 2 years of OHLCV data
- ✅ **Minute Aggregates** - Historical minute-level data
- ✅ **All U.S. Tickers** - Access to all stock symbols
- ✅ **Real-Time Quotes** - Live bid/ask prices
- ✅ **Trade Data** - Individual trades
- ✅ **100% Market Coverage** - Complete data for U.S. stocks

### 2. **Options Data**
- ✅ **Options Quotes** - Bid/ask for options
- ✅ **Options Contracts** - All available strikes
- ✅ **Option Chains** - Complete option chains

### 3. **Forex Data**
- ✅ **Currency Pairs** - Major forex pairs
- ✅ **End-of-Day** - Daily closing rates
- ✅ **Historical** - 2 years of historical forex data

### 4. **Crypto Data**
- ✅ **Cryptocurrency Prices** - Major coins
- ✅ **End-of-Day** - Daily closing prices
- ✅ **Historical** - 2 years of crypto history

### 5. **Reference Data**
- ✅ **Ticker Details** - Company information
- ✅ **Market Metadata** - Exchange info, market hours
- ✅ **Ticker News** - Company news and press releases
- ✅ **Company Financials** - Earnings, financial statements

### 6. **Corporate Actions**
- ✅ **Dividends** - Dividend history
- ✅ **Stock Splits** - Split history
- ✅ **Mergers/Acquisitions** - Corporate events

### 7. **Technical Indicators** (via aggregates)
- ✅ **OHLCV Bars** - Open, High, Low, Close, Volume
- ✅ **Time-Based Aggregates** - 1min, 5min, 1hr, 1day bars
- ✅ **Volume Data** - Trading volume

## Free Tier Limitations

### Rate Limits:
- ⚠️ **5 API calls per minute** - Hard limit
- ⚠️ **No real-time streaming** - Only delayed data on free tier
- ⚠️ **2 years historical** - Limited to 2 years back
- ⚠️ **End-of-day only** - Not intraday for free tier (except aggregates)

### What You CAN'T Get on Free:
- ❌ Real-time streaming data
- ❌ Sub-second precision
- ❌ Intraday tick data (but aggregates available)
- ❌ Unlimited API calls

## Available API Endpoints (Free Tier)

### 1. **Aggregates (Bars)** - Best for Historical Data
```
GET /v2/aggs/ticker/{ticker}/range/{multiplier}/{timespan}/{from}/{to}
```
**Returns:** OHLCV data for specified timeframe
- `multiplier`: 1, 5, 15, 30, 60 (minutes), 1 (day), etc.
- `timespan`: minute, hour, day, week, month
- `from/to`: Date range

**Example:**
```bash
GET https://api.polygon.io/v2/aggs/ticker/AAPL/range/1/day/2024-01-01/2024-12-31
```

**Response:**
```json
{
  "resultsCount": 252,
  "results": [
    {
      "t": 1704153600000,  // timestamp in ms
      "o": 150.0,           // open
      "h": 152.5,           // high
      "l": 149.5,           // low
      "c": 151.2,           // close
      "v": 50000000,        // volume
      "vw": 150.5,          // volume weighted average
      "n": 25000            // number of transactions
    }
  ]
}
```

### 2. **Daily Bars**
```
GET /v2/aggs/ticker/{ticker}/range/1/day/{from}/{to}
```
Simplified endpoint for daily data.

### 3. **Previous Close**
```
GET /v2/aggs/ticker/{ticker}/prev
```
Last closing price.

### 4. **Grouped Daily Bars**
```
GET /v2/aggs/grouped/locale/us/market/stocks/{date}
```
All stocks on a specific date.

### 5. **Ticker Details**
```
GET /v3/reference/tickers/{ticker}
```
Company information, market data, etc.

### 6. **Ticker News**
```
GET /v2/reference/news?ticker={ticker}
```
Latest news for a ticker.

### 7. **Dividends**
```
GET /v3/reference/dividends?ticker={ticker}
```
Dividend history.

### 8. **Stock Splits**
```
GET /v3/reference/splits?ticker={ticker}
```
Split history.

### 9. **Options Contracts**
```
GET /v3/reference/options/contracts?underlying_ticker={ticker}
```
All options for a stock.

### 10. **Ticker Search**
```
GET /v3/reference/tickers?search={query}&market=stocks
```
Search for tickers.

## Pricing Comparison

### Free Tier:
- **Cost:** $0/month
- **Rate Limit:** 5 calls/minute
- **Historical:** 2 years
- **Data:** End-of-day for stocks/forex/crypto

### Developer Tier: ($29/month)
- Unlimited API calls
- Real-time data
- Extended historical data
- Priority support

### Starter Tier: ($99/month)
- Everything in Developer
- Higher rate limits
- More data access

## Perfect for Your Project

### What Polygon.io Free Tier Gives You:

1. **✅ Historical Stock Data**
   - Get 2 years of daily OHLCV data
   - Perfect for your database storage
   - Minute-level aggregates available

2. **✅ No Complex Authentication**
   - Just an API key
   - No OAuth issues (like IBKR)
   - Simple REST API

3. **✅ Comprehensive Data**
   - Stocks, options, forex, crypto
   - Corporate actions
   - News and financials

4. **✅ Easy Integration**
   - REST API
   - Clear documentation
   - Good community support

## Getting Started

### 1. Sign Up (Free):
```
https://polygon.io/
- Create account
- No credit card required
- Get API key immediately
```

### 2. Get Your API Key:
- Dashboard → API Keys
- Copy your key

### 3. Update Configuration:
```json
{
  "Polygon": {
    "ApiKey": "YOUR_POLYGON_API_KEY",
    "BaseUrl": "https://api.polygon.io"
  }
}
```

### 4. Make Your First Request:
```bash
curl "https://api.polygon.io/v2/aggs/ticker/AAPL/range/1/day/2024-01-01/2024-12-31?apikey=YOUR_KEY"
```

## Recommendation for Your Project

### Use Polygon.io Because:
1. ✅ **Free tier is generous** - 2 years of data
2. ✅ **No registration issues** - Easy signup
3. ✅ **Works immediately** - No complex setup
4. ✅ **Perfect for your needs** - Historical stock data
5. ✅ **Better than IBKR free** - More reliable, better docs
6. ✅ **Rate limiting manageable** - 5 calls/min is workable for batch jobs

### Implementation Plan:

1. **Create Polygon Service** (similar to what we did for IBKR)
2. **Use existing database structure** - Store in `InteractiveBrokersPriceData` table or create new table
3. **Fetch daily bars** - Perfect for your use case
4. **Store in database** - Historical data storage
5. **Display in charts** - Use your existing chart components

## Example: Get Historical Data

```bash
# Get 6 months of daily data
GET https://api.polygon.io/v2/aggs/ticker/AAPL/range/1/day/2024-06-01/2024-12-31?apikey=YOUR_KEY

# Get previous close
GET https://api.polygon.io/v2/aggs/ticker/AAPL/prev?apikey=YOUR_KEY

# Get ticker info
GET https://api.polygon.io/v3/reference/tickers/AAPL?apikey=YOUR_KEY

# Get news
GET https://api.polygon.io/v2/reference/news?ticker=AAPL&limit=10&apikey=YOUR_KEY
```

## Rate Limit Strategy

Since you can only make **5 calls per minute**, you need to:

1. **Batch Process:** Fetch data during off-hours
2. **Delay Between Calls:** 12 seconds between calls (72 calls/hour)
3. **Prioritize:** Fetch most important data first
4. **Cache:** Store data locally to minimize re-fetching

**Example Timing:**
```
Call 1: 00:00 - AAPL
Call 2: 00:12 - MSFT
Call 3: 00:24 - TSLA
Call 4: 00:36 - NVDA
Call 5: 00:48 - GOOGL
Wait 12 seconds
Call 6: 01:00 - Next symbol
```

## Summary

### What You Get Free from Polygon.io:

**✅ US Stocks:**
- Daily bars (2 years history)
- OHLCV data
- Previous close
- Ticker details
- News
- Dividends
- Splits

**✅ Options:**
- Option contracts
- Strikes and expirations
- Quotes

**✅ Forex:**
- Major currency pairs
- Historical data

**✅ Crypto:**
- Major cryptocurrencies
- Historical prices

**✅ Reference Data:**
- All tickers
- Market metadata
- Company information

### Perfect Match for Your Project:
- Free historical data ✅
- Easy to use ✅
- No complex auth ✅
- Works immediately ✅
- Better than IBKR free tier ✅

Would you like me to implement a Polygon.io service for your project?
