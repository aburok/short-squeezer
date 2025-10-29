namespace StockDataApi.Models.Responses;

/// <summary>
/// DTO for daily aggregated ChartExchange Borrow Fee data (OHLC)
/// </summary>
public class ChartExchangeBorrowFeeDailyDataDto
{
    public DateTimeOffset Date { get; set; }
    public decimal Open { get; set; }      // Opening borrow fee for the day
    public decimal High { get; set; }       // Highest borrow fee for the day
    public decimal Low { get; set; }        // Lowest borrow fee for the day
    public decimal Close { get; set; }      // Closing borrow fee for the day
    public decimal Average { get; set; }   // Average borrow fee for the day
    public int DataPointCount { get; set; } // Number of data points aggregated
    public long MaxAvailable { get; set; }  // Maximum available shares
    public long MinAvailable { get; set; }  // Minimum available shares
    public long AverageAvailable { get; set; } // Average available shares
}

