using Newtonsoft.Json;

namespace StockDataLib.Models;

/// <summary>
/// Represents a stock split data point from ChartExchange API
/// </summary>
public class ChartExchangeStockSplitData
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("date")]
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

    [JsonProperty("split_ratio")]
    public string SplitRatio { get; set; } = string.Empty; // e.g., "2:1", "3:2"

    [JsonProperty("split_factor")]
    public decimal SplitFactor { get; set; } // e.g., 2.0 for 2:1 split

    [JsonProperty("from_factor")]
    public decimal FromFactor { get; set; } // e.g., 1 for 2:1 split

    [JsonProperty("to_factor")]
    public decimal ToFactor { get; set; } // e.g., 2 for 2:1 split

    [JsonProperty("ex_date")]
    public string? ExDate { get; set; } // Ex-dividend date

    [JsonProperty("record_date")]
    public string? RecordDate { get; set; }

    [JsonProperty("payable_date")]
    public string? PayableDate { get; set; }

    [JsonProperty("announcement_date")]
    public string? AnnouncementDate { get; set; }

    [JsonProperty("company_name")]
    public string? CompanyName { get; set; }
}