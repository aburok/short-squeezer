using Newtonsoft.Json;

namespace StockData.ChartExchange.Models;

/// <summary>
/// Base class for ChartExchange API responses
/// </summary>
public abstract class PagedResponse<TItem> : IChartExchangeResponse
{
    [JsonProperty("data")]
    public List<TItem>? Data { get; set; }
    
    [JsonProperty("results")]
    public List<TItem>? Results { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("page")]
    public int Page { get; set; }

    [JsonProperty("total_pages")]
    public int TotalPages { get; set; }
}