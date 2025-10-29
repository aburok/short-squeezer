using StockDataLib.Models;

namespace StockData.ChartExchange.DataModels;

/// <summary>
/// Represents ChartExchange Short Interest data stored in the database
/// </summary>
public class ChartExchangeShortInterest : StockDataPoint
{
    // Core short interest fields
    public decimal ShortInterestPercent { get; set; } // Parsed from short_interest string
    public long ShortPosition { get; set; } // Number of shares short
    public decimal DaysToCover { get; set; } // Parsed from days_to_cover string
    public long ChangeNumber { get; set; } // Change in number of shares
    public decimal ChangePercent { get; set; } // Parsed from change_percent string

    // Additional fields
    public string? ChartExchangeRequestId { get; set; }
}