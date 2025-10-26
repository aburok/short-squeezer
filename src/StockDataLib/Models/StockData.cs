using System;
using System.Collections.Generic;

namespace StockDataLib.Models
{
    /// <summary>
    /// Represents a stock ticker with its associated data
    /// </summary>
    public class StockTicker
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Exchange { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
        
        // Navigation properties
        public ICollection<PriceData> PriceData { get; set; }
        public ICollection<VolumeData> VolumeData { get; set; }
        public ICollection<ShortVolumeData> ShortVolumeData { get; set; }
        public ICollection<ShortPositionData> ShortPositionData { get; set; }
        public ICollection<ShortInterestData> ShortInterestData { get; set; }
        public ICollection<BorrowFeeData> BorrowFeeData { get; set; }
        public ICollection<RedditMentionData> RedditMentionData { get; set; }
    }

    /// <summary>
    /// Base class for all time-series stock data
    /// </summary>
    public abstract class StockDataPoint
    {
        public int Id { get; set; }
        public int StockTickerId { get; set; }
        public DateTime Date { get; set; }
        
        // Navigation property
        public StockTicker StockTicker { get; set; }
    }

    /// <summary>
    /// Represents price data for a stock on a specific date
    /// </summary>
    public class PriceData : StockDataPoint
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }

    /// <summary>
    /// Represents volume data for a stock on a specific date
    /// </summary>
    public class VolumeData : StockDataPoint
    {
        public long Volume { get; set; }
    }

    /// <summary>
    /// Represents short volume data for a stock on a specific date
    /// </summary>
    public class ShortVolumeData : StockDataPoint
    {
        public long ShortVolume { get; set; }
        public decimal ShortVolumePercent { get; set; }
    }

    /// <summary>
    /// Represents short position change data for a stock on a specific date
    /// </summary>
    public class ShortPositionData : StockDataPoint
    {
        public long PositionChange { get; set; }
    }

    /// <summary>
    /// Represents short interest data for a stock on a specific date
    /// </summary>
    public class ShortInterestData : StockDataPoint
    {
        public decimal ShortInterest { get; set; }
        public long SharesShort { get; set; }
    }

    /// <summary>
    /// Represents borrow fee data for a stock on a specific date
    /// </summary>
    public class BorrowFeeData : StockDataPoint
    {
        public decimal Fee { get; set; }
        public decimal? AvailableShares { get; set; }
    }

    /// <summary>
    /// Represents Reddit mention data for a stock on a specific date
    /// </summary>
    public class RedditMentionData : StockDataPoint
    {
        public int Mentions { get; set; }
        public string? TopSubreddit { get; set; }
        public string? Sentiment { get; set; }
    }
}

// Made with Bob
