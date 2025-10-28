namespace StockDataLib.Models;

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