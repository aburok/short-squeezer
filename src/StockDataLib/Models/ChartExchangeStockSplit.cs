namespace StockDataLib.Models;

/// <summary>
/// Represents stock split data stored in the database from ChartExchange
/// </summary>
public class ChartExchangeStockSplit : StockDataPoint
{
    public string SplitRatio { get; set; } = string.Empty; // e.g., "2:1", "3:2"
    public decimal SplitFactor { get; set; } // e.g., 2.0 for 2:1 split
    public decimal FromFactor { get; set; } // e.g., 1 for 2:1 split
    public decimal ToFactor { get; set; } // e.g., 2 for 2:1 split
    public DateTime? ExDate { get; set; } // Ex-dividend date
    public DateTime? RecordDate { get; set; }
    public DateTime? PayableDate { get; set; }
    public DateTime? AnnouncementDate { get; set; }
    public string? CompanyName { get; set; }
}