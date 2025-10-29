using StockData.Contracts.ChartExchange;

namespace StockData.Contracts;

/// <summary>
/// Represents a stock ticker with its associated data
/// </summary>
public class StockTicker
{
    public string Symbol { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    // public ICollection<FinraShortInterestData> FinraShortInterestData { get; set; }

    public ICollection<FailureToDeliverEntity> ChartExchangeFailureToDeliver { get; set; }
    public ICollection<RedditMentionsEntity> ChartExchangeRedditMentions { get; set; }
    public ICollection<OptionChain> ChartExchangeOptionChain { get; set; }
    public ICollection<StockSplitEntity> ChartExchangeStockSplit { get; set; }
    public ICollection<ShortInterestEntity> ChartExchangeShortInterest { get; set; }
    public ICollection<ShortVolumeEntity> ChartExchangeShortVolume { get; set; }
    public ICollection<BorrowFeeEntity> ChartExchangeBorrowFee { get; set; }
    public ICollection<ChartExchangeBorrowFeeDaily> ChartExchangeBorrowFeeDaily { get; set; }
}