namespace StockData.Contracts.ChartExchange;

/// <summary>
/// Represents ChartExchange Short Volume data stored in the database
/// </summary>
public class ShortVolumeEntity : StockDataPoint
{
    // Core volume fields
    public long Rt { get; set; } // Total volume
    public long St { get; set; } // Short volume
    public long Lt { get; set; } // Long volume
    public long Fs { get; set; } // Fail to deliver
    public long Fse { get; set; } // Fail to deliver exempt

    // Exchange-specific volume fields
    public long Xnas { get; set; } // NASDAQ volume
    public long Xphl { get; set; } // Philadelphia volume
    public long Xnys { get; set; } // NYSE volume
    public long Arcx { get; set; } // ARCA volume
    public long Xcis { get; set; } // CBOE volume
    public long Xase { get; set; } // AMEX volume
    public long Xchi { get; set; } // Chicago volume
    public long Edgx { get; set; } // EDGX volume
    public long Bats { get; set; } // BATS volume
    public long Edga { get; set; } // EDGA volume
    public long Baty { get; set; } // BATY volume

    // Calculated fields
    public decimal ShortVolumePercent { get; set; }
    public string? ChartExchangeRequestId { get; set; }
}