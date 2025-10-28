namespace StockDataApi.Models.Responses;

public class ChartExchangeStockSplitDataDto
{
    public DateTimeOffset Date { get; set; }
    public string SplitRatio { get; set; } = string.Empty;
    public decimal SplitFactor { get; set; }
    public decimal FromFactor { get; set; }
    public decimal ToFactor { get; set; }
    public DateTimeOffset? ExDate { get; set; }
    public DateTimeOffset? RecordDate { get; set; }
    public DateTimeOffset? PayableDate { get; set; }
    public DateTimeOffset? AnnouncementDate { get; set; }
    public string? CompanyName { get; set; }
}