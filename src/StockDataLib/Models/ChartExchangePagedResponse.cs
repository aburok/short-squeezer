using Newtonsoft.Json;

namespace StockDataLib.Models;

/// <summary>
/// Base class for ChartExchange API responses
/// </summary>
public abstract class ChartExchangePagedResponse<TItem> : ChartExchangeResponse
{
    [JsonProperty("data")]
    public List<TItem> Data { get; set; } = new();

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("page")]
    public int Page { get; set; }

    [JsonProperty("total_pages")]
    public int TotalPages { get; set; }
}