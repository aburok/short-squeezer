using Newtonsoft.Json;

namespace StockData.ChartExchange.Models;

/// <summary>
/// Represents a Reddit mentions data point from ChartExchange API
/// </summary>
public class RedditMentionsData
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonProperty("subreddit")]
    public string Subreddit { get; set; }

    [JsonProperty("created")]
    public string Created { get; set; } = string.Empty; // YYYY-MM-DD format

    [JsonProperty("sentiment")]
    public decimal? Sentiment { get; set; }
    
    [JsonProperty("thing_id")]
    public string? ThingId { get; set; }
    
    [JsonProperty("thing_type")]
    public string? ThingType { get; set; }

    [JsonProperty("author")]
    public string? Author { get; set; }
    
    [JsonProperty("text")]
    public string? Text { get; set; }

    [JsonProperty("link")]
    public string? Link { get; set; }
}