using Newtonsoft.Json;

namespace StockDataLib.Models;

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