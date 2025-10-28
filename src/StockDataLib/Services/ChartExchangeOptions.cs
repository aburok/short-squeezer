namespace StockDataLib.Services;

/// <summary>
/// Configuration options for ChartExchange API
/// </summary>
public class ChartExchangeOptions : IServiceOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://chartexchange.com";
    public int RateLimitPerMinute { get; set; } = 60;
    public bool IgnoreSslErrors { get; set; } = false;
}