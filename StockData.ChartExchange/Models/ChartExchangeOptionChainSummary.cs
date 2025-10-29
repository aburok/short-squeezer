using Newtonsoft.Json;

namespace StockData.ChartExchange.Models;

/// <summary>
/// Represents aggregated option chain summary data
/// </summary>
public class ChartExchangeOptionChainSummary
{
    [JsonProperty("total_call_volume")]
    public long TotalCallVolume { get; set; }

    [JsonProperty("total_put_volume")]
    public long TotalPutVolume { get; set; }

    [JsonProperty("total_call_open_interest")]
    public long TotalCallOpenInterest { get; set; }

    [JsonProperty("total_put_open_interest")]
    public long TotalPutOpenInterest { get; set; }

    [JsonProperty("put_call_volume_ratio")]
    public decimal PutCallVolumeRatio { get; set; }

    [JsonProperty("put_call_open_interest_ratio")]
    public decimal PutCallOpenInterestRatio { get; set; }

    [JsonProperty("max_pain")]
    public decimal? MaxPain { get; set; }

    [JsonProperty("total_implied_volatility")]
    public decimal? TotalImpliedVolatility { get; set; }
}