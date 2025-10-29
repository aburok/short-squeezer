using Newtonsoft.Json;

namespace StockData.ChartExchange.Models;

/// <summary>
/// Represents a short interest data point from ChartExchange API response
/// </summary>
public class ShortInterestData
{
    [JsonProperty("date")]
    public string Date { get; set; } = string.Empty;

    [JsonProperty("short_interest")]
    public string ShortInterest { get; set; } = string.Empty; // Percentage as string

    [JsonProperty("short_position")]
    public long ShortPosition { get; set; } // Number of shares short

    [JsonProperty("days_to_cover")]
    public string DaysToCover { get; set; } = string.Empty; // Days to cover as string

    [JsonProperty("change_number")]
    public long ChangeNumber { get; set; } // Change in number of shares

    [JsonProperty("change_percent")]
    public string ChangePercent { get; set; } = string.Empty; // Change percentage as string
}