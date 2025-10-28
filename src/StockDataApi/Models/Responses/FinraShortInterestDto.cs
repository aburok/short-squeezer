namespace StockDataApi.Models.Responses;

public class FinraShortInterestDto
{
    public DateTimeOffset Date { get; set; }
    public long ShortInterest { get; set; }
    public long SharesOutstanding { get; set; }
    public decimal ShortInterestPercent { get; set; }
    public decimal Days2Cover { get; set; }
}