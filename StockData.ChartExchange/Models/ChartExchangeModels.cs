using Newtonsoft.Json;
using StockDataLib.Models;

namespace StockData.ChartExchange.Models
{
    public interface IChartExchangeResponse
    {

    }
    
    public abstract class ArrayResponse<TItem> : List<TItem>, IChartExchangeResponse
    {

    }
    
    /// <summary>
    /// Base class for ChartExchange API responses
    /// </summary>
    public abstract class Response  
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
    public class FailureToDeliverResponse : ArrayResponse<BorrowFeeData>
    {
    }

    /// <summary>
    /// Represents the option chain summary API response from ChartExchange
    /// </summary>
    public class OptionChainResponse : PagedResponse<OptionChainData>
    {
        [JsonProperty("summary")]
        public ChartExchangeOptionChainSummary? Summary { get; set; }
    }

    /// <summary>
    /// Represents the stock split API response from ChartExchange
    /// </summary>
    public class StockSplitResponse : PagedResponse<StockSplitData>
    {

    }

    /// <summary>
    /// Represents the short interest API response from ChartExchange (simple array)
    /// </summary>
    public class ShortInterestResponse : List<ShortInterestData>
    {
    }

    /// <summary>
    /// Represents the short volume API response from ChartExchange (simple array)
    /// </summary>
    public class ShortVolumeResponse : PagedResponse<ShortVolumeData>
    {
    }

    /// <summary>
    /// Represents the borrow fee API response from ChartExchange (paged response)
    /// </summary>
    public class BorrowFeeResponse : PagedResponse<BorrowFeeData>
    {
    }
}
