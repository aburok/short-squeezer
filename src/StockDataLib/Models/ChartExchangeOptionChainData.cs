using Newtonsoft.Json;

namespace StockDataLib.Models;

/// <summary>
/// Represents an option chain summary data point from ChartExchange API
/// </summary>
public class ChartExchangeOptionChainData
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("date")]
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD format

    [JsonProperty("expiration_date")]
    public string ExpirationDate { get; set; } = string.Empty;

    [JsonProperty("strike_price")]
    public decimal StrikePrice { get; set; }

    [JsonProperty("option_type")]
    public string OptionType { get; set; } = string.Empty; // "call" or "put"

    [JsonProperty("volume")]
    public long Volume { get; set; }

    [JsonProperty("open_interest")]
    public long OpenInterest { get; set; }

    [JsonProperty("bid")]
    public decimal? Bid { get; set; }

    [JsonProperty("ask")]
    public decimal? Ask { get; set; }

    [JsonProperty("last_price")]
    public decimal? LastPrice { get; set; }

    [JsonProperty("implied_volatility")]
    public decimal? ImpliedVolatility { get; set; }

    [JsonProperty("delta")]
    public decimal? Delta { get; set; }

    [JsonProperty("gamma")]
    public decimal? Gamma { get; set; }

    [JsonProperty("theta")]
    public decimal? Theta { get; set; }

    [JsonProperty("vega")]
    public decimal? Vega { get; set; }
}