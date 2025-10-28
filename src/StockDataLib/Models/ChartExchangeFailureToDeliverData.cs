using Newtonsoft.Json;

namespace StockDataLib.Models;

/// <summary>
/// Represents a failure to deliver data point from ChartExchange API
/// </summary>
public class ChartExchangeFailureToDeliverData
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("date")]
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

    [JsonProperty("failure_to_deliver")]
    public long FailureToDeliver { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("volume")]
    public long Volume { get; set; }

    [JsonProperty("settlement_date")]
    public string? SettlementDate { get; set; }

    [JsonProperty("cusip")]
    public string? Cusip { get; set; }

    [JsonProperty("company_name")]
    public string? CompanyName { get; set; }
}