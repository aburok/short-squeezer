using System;
using System.Collections.Generic;

namespace StockDataApi.Models.Responses
{
    /// <summary>
    /// Unified response containing all stock data types
    /// </summary>
    public class StockDataResponse
    {
        public string Symbol { get; set; } = string.Empty;
        
        public BorrowFeeDataDto[] BorrowFeeData { get; set; } = Array.Empty<BorrowFeeDataDto>();
        
        public PolygonDataDto PolygonData { get; set; } = new PolygonDataDto();
        
        public FinraDataDto FinraData { get; set; } = new FinraDataDto();
    }

    public class BorrowFeeDataDto
    {
        public DateTime Date { get; set; }
        public decimal Fee { get; set; }
        public decimal? AvailableShares { get; set; }
    }

    public class PolygonDataDto
    {
        public PolygonPriceDataDto[] PriceData { get; set; } = Array.Empty<PolygonPriceDataDto>();
        public PolygonShortInterestDto[] ShortInterestData { get; set; } = Array.Empty<PolygonShortInterestDto>();
        public PolygonShortVolumeDto[] ShortVolumeData { get; set; } = Array.Empty<PolygonShortVolumeDto>();
    }

    public class PolygonPriceDataDto
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }

    public class PolygonShortInterestDto
    {
        public DateTime Date { get; set; }
        public long ShortInterest { get; set; }
        public long AvgDailyVolume { get; set; }
        public decimal DaysToCover { get; set; }
    }

    public class PolygonShortVolumeDto
    {
        public DateTime Date { get; set; }
        public long ShortVolume { get; set; }
        public long TotalVolume { get; set; }
        public decimal ShortVolumeRatio { get; set; }
    }

    public class FinraDataDto
    {
        public FinraShortInterestDto[] ShortInterestData { get; set; } = Array.Empty<FinraShortInterestDto>();
    }

    public class FinraShortInterestDto
    {
        public DateTime Date { get; set; }
        public long ShortInterest { get; set; }
        public long SharesOutstanding { get; set; }
        public decimal ShortInterestPercent { get; set; }
        public decimal Days2Cover { get; set; }
    }
}
