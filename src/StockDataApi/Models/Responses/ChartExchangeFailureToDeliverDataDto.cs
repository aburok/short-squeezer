namespace StockDataApi.Models.Responses;

public class ChartExchangeFailureToDeliverDataDto
{
    public DateTimeOffset Date { get; set; }
    public long FailureToDeliver { get; set; }
    public decimal Price { get; set; }
    public long Volume { get; set; }
    public DateTimeOffset? SettlementDate { get; set; }
    public string? Cusip { get; set; }
    public string? CompanyName { get; set; }
}