using StockDataLib.Models;

namespace StockData.ChartExchange.DataModels;

/// <summary>
/// Represents daily aggregated ChartExchange Borrow Fee data (OHLC - Open, High, Low, Close)
/// </summary>
public class ChartExchangeBorrowFeeDaily : StockDataPoint
{
    /// <summary>
    /// Opening borrow fee for the day (first recorded value)
    /// </summary>
    public decimal Open { get; set; }

    /// <summary>
    /// Highest borrow fee for the day
    /// </summary>
    public decimal High { get; set; }

    /// <summary>
    /// Lowest borrow fee for the day
    /// </summary>
    public decimal Low { get; set; }

    /// <summary>
    /// Closing borrow fee for the day (last recorded value)
    /// </summary>
    public decimal Close { get; set; }

    /// <summary>
    /// Average borrow fee for the day
    /// </summary>
    public decimal Average { get; set; }

    /// <summary>
    /// Total number of data points aggregated for this day
    /// </summary>
    public int DataPointCount { get; set; }

    /// <summary>
    /// Maximum available shares for the day
    /// </summary>
    public long MaxAvailable { get; set; }

    /// <summary>
    /// Minimum available shares for the day
    /// </summary>
    public long MinAvailable { get; set; }

    /// <summary>
    /// Average available shares for the day
    /// </summary>
    public long AverageAvailable { get; set; }

    /// <summary>
    /// ChartExchange request ID for tracking
    /// </summary>
    public string? ChartExchangeRequestId { get; set; }
}

