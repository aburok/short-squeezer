using Newtonsoft.Json;

namespace StockData.ChartExchange.Models;

/// <summary>
/// Represents a borrow fee data point from ChartExchange API response
/// </summary>
public class BorrowFeeData
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

