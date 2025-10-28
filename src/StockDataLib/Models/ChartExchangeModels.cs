using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StockDataLib.Models
{
    public interface IChartExchangeResponse
    {

    }
    
    public abstract class ChartExchangeArrayResponse<TItem> : List<TItem>, IChartExchangeResponse
    {

    }
    
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
    /// Represents the failure to deliver API response from ChartExchange
    /// </summary>
    public class ChartExchangeFailureToDeliverResponse : ChartExchangeArrayResponse<ChartExchangeBorrowFeeData>
    {
    }

    /// <summary>
    /// Represents the Reddit mentions API response from ChartExchange
    /// </summary>
    public class ChartExchangeRedditMentionsResponse : ChartExchangePagedResponse<ChartExchangeRedditMentionsData>
    {

    }

    /// <summary>
    /// Represents the option chain summary API response from ChartExchange
    /// </summary>
    public class ChartExchangeOptionChainResponse : ChartExchangePagedResponse<ChartExchangeOptionChainData>
    {
        [JsonProperty("summary")]
        public ChartExchangeOptionChainSummary? Summary { get; set; }
    }

    /// <summary>
    /// Represents the stock split API response from ChartExchange
    /// </summary>
    public class ChartExchangeStockSplitResponse : ChartExchangePagedResponse<ChartExchangeStockSplitData>
    {

    }

    /// <summary>
    /// Represents the short interest API response from ChartExchange (simple array)
    /// </summary>
    public class ChartExchangeShortInterestResponse : List<ChartExchangeShortInterestData>
    {
    }

    /// <summary>
    /// Represents the short volume API response from ChartExchange (simple array)
    /// </summary>
    public class ChartExchangeShortVolumeResponse : List<ChartExchangeShortVolumeData>
    {
    }

    /// <summary>
    /// Represents the borrow fee API response from ChartExchange (paged response)
    /// </summary>
    public class ChartExchangeBorrowFeeResponse : ChartExchangePagedResponse<ChartExchangeBorrowFeeData>
    {
    }
}
