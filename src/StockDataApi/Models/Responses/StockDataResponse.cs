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

        public ChartExchangeDataDto ChartExchangeData { get; set; } = new ChartExchangeDataDto();

        public FinraDataDto FinraData { get; set; } = new FinraDataDto();
    }

    public class ChartExchangeDataDto
    {
        public ChartExchangeFailureToDeliverDataDto[] FailureToDeliverData { get; set; } = Array.Empty<ChartExchangeFailureToDeliverDataDto>();
        public ChartExchangeRedditMentionsDataDto[] RedditMentionsData { get; set; } = Array.Empty<ChartExchangeRedditMentionsDataDto>();
        public ChartExchangeOptionChainDataDto[] OptionChainData { get; set; } = Array.Empty<ChartExchangeOptionChainDataDto>();
        public ChartExchangeStockSplitDataDto[] StockSplitData { get; set; } = Array.Empty<ChartExchangeStockSplitDataDto>();
        public ChartExchangeShortInterestDataDto[] ShortInterestData { get; set; } = Array.Empty<ChartExchangeShortInterestDataDto>();
        public ChartExchangeShortVolumeDataDto[] ShortVolumeData { get; set; } = Array.Empty<ChartExchangeShortVolumeDataDto>();
        public ChartExchangeBorrowFeeDataDto[] BorrowFeeData { get; set; } = Array.Empty<ChartExchangeBorrowFeeDataDto>();
    }

    public class ChartExchangeFailureToDeliverDataDto
    {
        public DateTimeOffset Date { get; set; }
        public long FailureToDeliver { get; set; }
        public decimal Price { get; set; }
        public long Volume { get; set; }
        public DateTimeOffset? SettlementDate { get; set; }
        public string? Cusip { get; set; }
        public string? CompanyName { get; set; }
    }

    public class ChartExchangeRedditMentionsDataDto
    {
        public DateTimeOffset Date { get; set; }
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
        public DateTimeOffset Date { get; set; }
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
        public DateTimeOffset Date { get; set; }
        public string SplitRatio { get; set; } = string.Empty;
        public decimal SplitFactor { get; set; }
        public decimal FromFactor { get; set; }
        public decimal ToFactor { get; set; }
        public DateTimeOffset? ExDate { get; set; }
        public DateTimeOffset? RecordDate { get; set; }
        public DateTimeOffset? PayableDate { get; set; }
        public DateTimeOffset? AnnouncementDate { get; set; }
        public string? CompanyName { get; set; }
    }

    public class ChartExchangeShortInterestDataDto
    {
        public DateTimeOffset Date { get; set; }
        public decimal ShortInterestPercent { get; set; } // Parsed from short_interest string
        public long ShortPosition { get; set; } // Number of shares short
        public decimal DaysToCover { get; set; } // Parsed from days_to_cover string
        public long ChangeNumber { get; set; } // Change in number of shares
        public decimal ChangePercent { get; set; } // Parsed from change_percent string
    }

    public class ChartExchangeShortVolumeDataDto
    {
        public DateTimeOffset Date { get; set; }

        // Core volume fields
        public long Rt { get; set; } // Total volume
        public long St { get; set; } // Short volume
        public long Lt { get; set; } // Long volume
        public long Fs { get; set; } // Fail to deliver
        public long Fse { get; set; } // Fail to deliver exempt

        // Exchange-specific volume fields
        public long Xnas { get; set; } // NASDAQ volume
        public long Xphl { get; set; } // Philadelphia volume
        public long Xnys { get; set; } // NYSE volume
        public long Arcx { get; set; } // ARCA volume
        public long Xcis { get; set; } // CBOE volume
        public long Xase { get; set; } // AMEX volume
        public long Xchi { get; set; } // Chicago volume
        public long Edgx { get; set; } // EDGX volume
        public long Bats { get; set; } // BATS volume
        public long Edga { get; set; } // EDGA volume
        public long Baty { get; set; } // BATY volume

        // Calculated fields
        public decimal ShortVolumePercent { get; set; }
    }

    public class ChartExchangeBorrowFeeDataDto
    {
        public DateTimeOffset Date { get; set; }
        public long Available { get; set; } // Available shares
        public decimal Fee { get; set; } // Parsed from fee string
        public decimal Rebate { get; set; } // Parsed from rebate string
    }

    public class FinraDataDto
    {
        public FinraShortInterestDto[] ShortInterestData { get; set; } = Array.Empty<FinraShortInterestDto>();
    }

    public class FinraShortInterestDto
    {
        public DateTimeOffset Date { get; set; }
        public long ShortInterest { get; set; }
        public long SharesOutstanding { get; set; }
        public decimal ShortInterestPercent { get; set; }
        public decimal Days2Cover { get; set; }
    }
}
