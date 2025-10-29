namespace StockDataApi.Models.Responses;

public class ChartExchangeRedditMentionsDataDto
{
    public string Subreddit { get; set; }

    public DateTimeOffset Created { get; set; }

    public decimal? Sentiment { get; set; }
    
    public string? ThingId { get; set; }
    
    public string? ThingType { get; set; }

    public string? Author { get; set; }
    
    public string? Text { get; set; }

    public string? Link { get; set; }
}