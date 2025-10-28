using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StockDataLib.Models
{
    /// <summary>
    /// Base class for ChartExchange API responses
    /// </summary>
    public abstract class ChartExchangeResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("timestamp")]
        public long? Timestamp { get; set; }
    }

    /// <summary>
    /// Represents a failure to deliver data point from ChartExchange API
    /// </summary>
    public class ChartExchangeFailureToDeliverData
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

        [JsonProperty("failure_to_deliver")]
        public long FailureToDeliver { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("volume")]
        public long Volume { get; set; }

        [JsonProperty("settlement_date")]
        public string? SettlementDate { get; set; }

        [JsonProperty("cusip")]
        public string? Cusip { get; set; }

        [JsonProperty("company_name")]
        public string? CompanyName { get; set; }
    }

    /// <summary>
    /// Represents the failure to deliver API response from ChartExchange
    /// </summary>
    public class ChartExchangeFailureToDeliverResponse : ChartExchangeResponse
    {
        [JsonProperty("data")]
        public List<ChartExchangeFailureToDeliverData> Data { get; set; } = new List<ChartExchangeFailureToDeliverData>();

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Represents failure to deliver data stored in the database from ChartExchange
    /// </summary>
    public class ChartExchangeFailureToDeliver : StockDataPoint
    {
        public long FailureToDeliver { get; set; }
        public decimal Price { get; set; }
        public long Volume { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string? Cusip { get; set; }
        public string? CompanyName { get; set; }
        public string? ChartExchangeRequestId { get; set; }
    }

    /// <summary>
    /// Represents a Reddit mentions data point from ChartExchange API
    /// </summary>
    public class ChartExchangeRedditMentionsData
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

        [JsonProperty("mentions")]
        public int Mentions { get; set; }

        [JsonProperty("sentiment_score")]
        public decimal? SentimentScore { get; set; }

        [JsonProperty("sentiment_label")]
        public string? SentimentLabel { get; set; }

        [JsonProperty("subreddit")]
        public string? Subreddit { get; set; }

        [JsonProperty("upvotes")]
        public int? Upvotes { get; set; }

        [JsonProperty("comments")]
        public int? Comments { get; set; }

        [JsonProperty("engagement_score")]
        public decimal? EngagementScore { get; set; }
    }

    /// <summary>
    /// Represents the Reddit mentions API response from ChartExchange
    /// </summary>
    public class ChartExchangeRedditMentionsResponse : ChartExchangeResponse
    {
        [JsonProperty("data")]
        public List<ChartExchangeRedditMentionsData> Data { get; set; } = new List<ChartExchangeRedditMentionsData>();

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Represents Reddit mentions data stored in the database from ChartExchange
    /// </summary>
    public class ChartExchangeRedditMentions : StockDataPoint
    {
        public int Mentions { get; set; }
        public decimal? SentimentScore { get; set; }
        public string? SentimentLabel { get; set; }
        public string? Subreddit { get; set; }
        public int? Upvotes { get; set; }
        public int? Comments { get; set; }
        public decimal? EngagementScore { get; set; }
        public string? ChartExchangeRequestId { get; set; }
    }

    /// <summary>
    /// Represents an option chain summary data point from ChartExchange API
    /// </summary>
    public class ChartExchangeOptionChainData
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

        [JsonProperty("expiration_date")]
        public string ExpirationDate { get; set; } = string.Empty;

        [JsonProperty("strike_price")]
        public decimal StrikePrice { get; set; }

        [JsonProperty("option_type")]
        public string OptionType { get; set; } = string.Empty; // "call" or "put"

        [JsonProperty("volume")]
        public long Volume { get; set; }

        [JsonProperty("open_interest")]
        public long OpenInterest { get; set; }

        [JsonProperty("bid")]
        public decimal? Bid { get; set; }

        [JsonProperty("ask")]
        public decimal? Ask { get; set; }

        [JsonProperty("last_price")]
        public decimal? LastPrice { get; set; }

        [JsonProperty("implied_volatility")]
        public decimal? ImpliedVolatility { get; set; }

        [JsonProperty("delta")]
        public decimal? Delta { get; set; }

        [JsonProperty("gamma")]
        public decimal? Gamma { get; set; }

        [JsonProperty("theta")]
        public decimal? Theta { get; set; }

        [JsonProperty("vega")]
        public decimal? Vega { get; set; }
    }

    /// <summary>
    /// Represents the option chain summary API response from ChartExchange
    /// </summary>
    public class ChartExchangeOptionChainResponse : ChartExchangeResponse
    {
        [JsonProperty("data")]
        public List<ChartExchangeOptionChainData> Data { get; set; } = new List<ChartExchangeOptionChainData>();

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty("summary")]
        public ChartExchangeOptionChainSummary? Summary { get; set; }
    }

    /// <summary>
    /// Represents option chain summary data stored in the database from ChartExchange
    /// </summary>
    public class ChartExchangeOptionChain : StockDataPoint
    {
        public string ExpirationDate { get; set; } = string.Empty;
        public decimal StrikePrice { get; set; }
        public string OptionType { get; set; } = string.Empty; // "call" or "put"
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
        public string? ChartExchangeRequestId { get; set; }
    }

    /// <summary>
    /// Represents aggregated option chain summary data
    /// </summary>
    public class ChartExchangeOptionChainSummary
    {
        [JsonProperty("total_call_volume")]
        public long TotalCallVolume { get; set; }

        [JsonProperty("total_put_volume")]
        public long TotalPutVolume { get; set; }

        [JsonProperty("total_call_open_interest")]
        public long TotalCallOpenInterest { get; set; }

        [JsonProperty("total_put_open_interest")]
        public long TotalPutOpenInterest { get; set; }

        [JsonProperty("put_call_volume_ratio")]
        public decimal PutCallVolumeRatio { get; set; }

        [JsonProperty("put_call_open_interest_ratio")]
        public decimal PutCallOpenInterestRatio { get; set; }

        [JsonProperty("max_pain")]
        public decimal? MaxPain { get; set; }

        [JsonProperty("total_implied_volatility")]
        public decimal? TotalImpliedVolatility { get; set; }
    }

    /// <summary>
    /// Represents a stock split data point from ChartExchange API
    /// </summary>
    public class ChartExchangeStockSplitData
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

        [JsonProperty("split_ratio")]
        public string SplitRatio { get; set; } = string.Empty; // e.g., "2:1", "3:2"

        [JsonProperty("split_factor")]
        public decimal SplitFactor { get; set; } // e.g., 2.0 for 2:1 split

        [JsonProperty("from_factor")]
        public decimal FromFactor { get; set; } // e.g., 1 for 2:1 split

        [JsonProperty("to_factor")]
        public decimal ToFactor { get; set; } // e.g., 2 for 2:1 split

        [JsonProperty("ex_date")]
        public string? ExDate { get; set; } // Ex-dividend date

        [JsonProperty("record_date")]
        public string? RecordDate { get; set; }

        [JsonProperty("payable_date")]
        public string? PayableDate { get; set; }

        [JsonProperty("announcement_date")]
        public string? AnnouncementDate { get; set; }

        [JsonProperty("company_name")]
        public string? CompanyName { get; set; }
    }

    /// <summary>
    /// Represents the stock split API response from ChartExchange
    /// </summary>
    public class ChartExchangeStockSplitResponse : ChartExchangeResponse
    {
        [JsonProperty("data")]
        public List<ChartExchangeStockSplitData> Data { get; set; } = new List<ChartExchangeStockSplitData>();

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Represents stock split data stored in the database from ChartExchange
    /// </summary>
    public class ChartExchangeStockSplit : StockDataPoint
    {
        public string SplitRatio { get; set; } = string.Empty; // e.g., "2:1", "3:2"
        public decimal SplitFactor { get; set; } // e.g., 2.0 for 2:1 split
        public decimal FromFactor { get; set; } // e.g., 1 for 2:1 split
        public decimal ToFactor { get; set; } // e.g., 2 for 2:1 split
        public DateTime? ExDate { get; set; } // Ex-dividend date
        public DateTime? RecordDate { get; set; }
        public DateTime? PayableDate { get; set; }
        public DateTime? AnnouncementDate { get; set; }
        public string? CompanyName { get; set; }
        public string? ChartExchangeRequestId { get; set; }
    }

    /// <summary>
    /// Represents price data from ChartExchange API (replacing Polygon price data)
    /// </summary>
    public class ChartExchangePriceData
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

        [JsonProperty("open")]
        public decimal Open { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("close")]
        public decimal Close { get; set; }

        [JsonProperty("volume")]
        public long Volume { get; set; }

        [JsonProperty("adjusted_close")]
        public decimal? AdjustedClose { get; set; }

        [JsonProperty("dividend_amount")]
        public decimal? DividendAmount { get; set; }

        [JsonProperty("split_coefficient")]
        public decimal? SplitCoefficient { get; set; }
    }

    /// <summary>
    /// Represents the price data API response from ChartExchange
    /// </summary>
    public class ChartExchangePriceResponse : ChartExchangeResponse
    {
        [JsonProperty("data")]
        public List<ChartExchangePriceData> Data { get; set; } = new List<ChartExchangePriceData>();

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Represents price data stored in the database from ChartExchange
    /// </summary>
    public class ChartExchangePrice : StockDataPoint
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal? AdjustedClose { get; set; }
        public decimal? DividendAmount { get; set; }
        public decimal? SplitCoefficient { get; set; }
        public string? ChartExchangeRequestId { get; set; }
    }
}
