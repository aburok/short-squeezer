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
    /// Represents a short interest data point from Polygon.io API response
    /// </summary>
    public class PolygonShortInterestBarData
    {
        [JsonProperty("ticker")]
        public string Ticker { get; set; } = string.Empty;

        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

        [JsonProperty("short_interest")]
        public long ShortInterest { get; set; }

        [JsonProperty("avg_daily_volume")]
        public long AvgDailyVolume { get; set; }

        [JsonProperty("days_to_cover")]
        public decimal DaysToCover { get; set; }
    }

    /// <summary>
    /// Represents short interest data stored in the database from Polygon.io
    /// </summary>
    public class PolygonShortInterestData : StockDataPoint
    {
        public long ShortInterest { get; set; }
        public long AvgDailyVolume { get; set; }
        public decimal DaysToCover { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string? PolygonRequestId { get; set; }
    }

    /// <summary>
    /// Represents a short volume data point from Polygon.io API response
    /// </summary>
    public class PolygonShortVolumeBarData
    {
        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

        [JsonProperty("short_volume")]
        public long ShortVolume { get; set; }

        [JsonProperty("total_volume")]
        public long TotalVolume { get; set; }

        [JsonProperty("short_volume_ratio")]
        public decimal ShortVolumeRatio { get; set; }
    }

    /// <summary>
    /// Represents short volume data stored in the database from Polygon.io
    /// </summary>
    public class PolygonShortVolumeData : StockDataPoint
    {
        public long ShortVolume { get; set; }
        public long TotalVolume { get; set; }
        public decimal ShortVolumeRatio { get; set; }
        public string? PolygonRequestId { get; set; }
    }
}
