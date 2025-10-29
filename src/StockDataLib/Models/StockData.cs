using System;
using System.Collections.Generic;
using StockData.Contracts;

namespace StockDataLib.Models
{
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
