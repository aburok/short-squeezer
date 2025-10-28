using System;
using System.Collections.Generic;

namespace StockDataLib.Models
{
    /// <summary>
    /// Represents a stock ticker with its associated data
    /// </summary>
    public class StockTicker
    {
        public string Symbol { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }

        // Navigation properties
        public ICollection<FinraShortInterestData> FinraShortInterestData { get; set; }

        public ICollection<ChartExchangeFailureToDeliver> ChartExchangeFailureToDeliver { get; set; }
        public ICollection<ChartExchangeRedditMentions> ChartExchangeRedditMentions { get; set; }
        public ICollection<ChartExchangeOptionChain> ChartExchangeOptionChain { get; set; }
        public ICollection<ChartExchangeStockSplit> ChartExchangeStockSplit { get; set; }
        public ICollection<ChartExchangeShortInterest> ChartExchangeShortInterest { get; set; }
        public ICollection<ChartExchangeShortVolume> ChartExchangeShortVolume { get; set; }
        public ICollection<ChartExchangeBorrowFee> ChartExchangeBorrowFee { get; set; }
    }

    /// <summary>
    /// Base class for all time-series stock data
    /// </summary>
    public abstract class StockDataPoint
    {
        public int Id { get; set; }
        public string StockTickerSymbol { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; }

        // Navigation property
        public StockTicker StockTicker { get; set; } = null!;
    }

    /// <summary>
    /// Represents FINRA short interest data for a stock on a specific date
    /// </summary>
    public class FinraShortInterestData : StockDataPoint
    {
        public long ShortInterest { get; set; }
        public decimal ShortInterestPercent { get; set; }
        public decimal MarketValue { get; set; }
        public long SharesOutstanding { get; set; }
        public long AvgDailyVolume { get; set; }
        public decimal Days2Cover { get; set; }
        public DateTime SettlementDate { get; set; }
    }
}

// Made with Bob
