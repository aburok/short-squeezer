namespace StockDataApi.Models.Responses;

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