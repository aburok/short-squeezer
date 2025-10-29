using StockDataLib.Models;

namespace StockData.ChartExchange.DataModels;

/// <summary>
/// Represents failure to deliver data stored in the database from ChartExchange
/// </summary>
public class ChartExchangeFailureToDeliver : StockDataPoint
{
    public long FailureToDeliver { get; set; }
    public decimal Price { get; set; }
    public long Volume { get; set; }
    public DateTime? SettlementDate { get; set; }
    public string? Cusip { get; set; }
    public string? CompanyName { get; set; }
}