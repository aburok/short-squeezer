namespace StockDataLib.Services;

public interface IServiceOptions
{
    string ApiKey { get; set; }
    string BaseUrl { get; set; }
    int RateLimitPerMinute { get; set; }
    bool IgnoreSslErrors { get; set; }
}