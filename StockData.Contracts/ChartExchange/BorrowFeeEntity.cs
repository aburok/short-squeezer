namespace StockData.Contracts.ChartExchange;

/// <summary>
/// Represents ChartExchange Borrow Fee data stored in the database
/// </summary>
public class BorrowFeeEntity : StockDataPoint
{
    // Core borrow fee fields
    public long Available { get; set; } // Available shares
    public decimal Fee { get; set; } // Parsed from fee string
    public decimal Rebate { get; set; } // Parsed from rebate string

}

