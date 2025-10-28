namespace StockDataApi.Models.Responses;

public class ChartExchangeBorrowFeeDataDto
{
    public DateTimeOffset Date { get; set; }
    public long Available { get; set; } // Available shares
    public decimal Fee { get; set; } // Parsed from fee string
    public decimal Rebate { get; set; } // Parsed from rebate string
}