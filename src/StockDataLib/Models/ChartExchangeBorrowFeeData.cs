using Newtonsoft.Json;

namespace StockDataLib.Models;

/// <summary>
/// Represents a borrow fee data point from ChartExchange API response
/// </summary>
public class ChartExchangeBorrowFeeData
{
    [JsonProperty("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonProperty("available")]
    public long Available { get; set; } // Available shares

    [JsonProperty("fee")]
    public string Fee { get; set; } = string.Empty; // Fee as string

    [JsonProperty("rebate")]
    public string Rebate { get; set; } = string.Empty; // Rebate as string
}
