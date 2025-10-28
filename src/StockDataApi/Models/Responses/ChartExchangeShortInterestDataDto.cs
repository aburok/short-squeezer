namespace StockDataApi.Models.Responses;

public class ChartExchangeShortInterestDataDto
{
    public DateTimeOffset Date { get; set; }
    public decimal ShortInterestPercent { get; set; } // Parsed from short_interest string
    public long ShortPosition { get; set; } // Number of shares short
    public decimal DaysToCover { get; set; } // Parsed from days_to_cover string
    public long ChangeNumber { get; set; } // Change in number of shares
    public decimal ChangePercent { get; set; } // Parsed from change_percent string
}