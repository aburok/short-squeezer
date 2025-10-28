using Newtonsoft.Json;

namespace StockDataLib.Models;

/// <summary>
/// Represents a short volume data point from ChartExchange API response
/// </summary>
public class ChartExchangeShortVolumeData
{
    [JsonProperty("date")]
    public string Date { get; set; } = string.Empty;

    [JsonProperty("rt")]
    public long Rt { get; set; } // Total volume

    [JsonProperty("st")]
    public long St { get; set; } // Short volume

    [JsonProperty("lt")]
    public long Lt { get; set; } // Long volume

    [JsonProperty("fs")]
    public long Fs { get; set; } // Fail to deliver

    [JsonProperty("fse")]
    public long Fse { get; set; } // Fail to deliver exempt

    [JsonProperty("xnas")]
    public long Xnas { get; set; } // NASDAQ volume

    [JsonProperty("xphl")]
    public long Xphl { get; set; } // Philadelphia volume

    [JsonProperty("xnys")]
    public long Xnys { get; set; } // NYSE volume

    [JsonProperty("arcx")]
    public long Arcx { get; set; } // ARCA volume

    [JsonProperty("xcis")]
    public long Xcis { get; set; } // CBOE volume

    [JsonProperty("xase")]
    public long Xase { get; set; } // AMEX volume

    [JsonProperty("xchi")]
    public long Xchi { get; set; } // Chicago volume

    [JsonProperty("edgx")]
    public long Edgx { get; set; } // EDGX volume

    [JsonProperty("bats")]
    public long Bats { get; set; } // BATS volume

    [JsonProperty("edga")]
    public long Edga { get; set; } // EDGA volume

    [JsonProperty("baty")]
    public long Baty { get; set; } // BATY volume
}
