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

        public ChartExchangeDataDto ChartExchangeData { get; set; } = new ChartExchangeDataDto();

        public FinraDataDto FinraData { get; set; } = new FinraDataDto();
    }

    public class BorrowFeeDataDto
    {
        public DateTime Date { get; set; }
        public decimal Fee { get; set; }
        public decimal? AvailableShares { get; set; }
    }

    public class ChartExchangeDataDto
    {
        public ChartExchangePriceDataDto[] PriceData { get; set; } = Array.Empty<ChartExchangePriceDataDto>();
        public ChartExchangeFailureToDeliverDataDto[] FailureToDeliverData { get; set; } = Array.Empty<ChartExchangeFailureToDeliverDataDto>();
        public ChartExchangeRedditMentionsDataDto[] RedditMentionsData { get; set; } = Array.Empty<ChartExchangeRedditMentionsDataDto>();
        public ChartExchangeOptionChainDataDto[] OptionChainData { get; set; } = Array.Empty<ChartExchangeOptionChainDataDto>();
        public ChartExchangeStockSplitDataDto[] StockSplitData { get; set; } = Array.Empty<ChartExchangeStockSplitDataDto>();
    }

    public class ChartExchangePriceDataDto
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal? AdjustedClose { get; set; }
        public decimal? DividendAmount { get; set; }
        public decimal? SplitCoefficient { get; set; }
    }

    public class ChartExchangeFailureToDeliverDataDto
    {
        public DateTime Date { get; set; }
        public long FailureToDeliver { get; set; }
        public decimal Price { get; set; }
        public long Volume { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string? Cusip { get; set; }
        public string? CompanyName { get; set; }
    }

    public class ChartExchangeRedditMentionsDataDto
    {
        public DateTime Date { get; set; }
        public int Mentions { get; set; }
        public decimal? SentimentScore { get; set; }
        public string? SentimentLabel { get; set; }
        public string? Subreddit { get; set; }
        public int? Upvotes { get; set; }
        public int? Comments { get; set; }
        public decimal? EngagementScore { get; set; }
    }

    public class ChartExchangeOptionChainDataDto
    {
        public DateTime Date { get; set; }
        public string ExpirationDate { get; set; } = string.Empty;
        public decimal StrikePrice { get; set; }
        public string OptionType { get; set; } = string.Empty;
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

    public class ChartExchangeStockSplitDataDto
    {
        public DateTime Date { get; set; }
        public string SplitRatio { get; set; } = string.Empty;
        public decimal SplitFactor { get; set; }
        public decimal FromFactor { get; set; }
        public decimal ToFactor { get; set; }
        public DateTime? ExDate { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? PayableDate { get; set; }
        public DateTime? AnnouncementDate { get; set; }
        public string? CompanyName { get; set; }
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
