namespace StockData.Contracts;

/// <summary>
/// Base class for all time-series stock data
/// </summary>
public abstract class StockDataPoint
{
    public int Id { get; set; }
    public string StockTickerSymbol { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }

    public string? ChartExchangeRequestId { get; set; }

    // Navigation property
    public StockTicker StockTicker { get; set; } = null!;
}