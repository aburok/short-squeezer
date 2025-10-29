namespace StockDataApi.Models.Responses;

public class ChartExchangeDataDto
{
    public ChartExchangeFailureToDeliverDataDto[] FailureToDeliverData { get; set; } = Array.Empty<ChartExchangeFailureToDeliverDataDto>();
    public ChartExchangeRedditMentionsDataDto[] RedditMentionsData { get; set; } = Array.Empty<ChartExchangeRedditMentionsDataDto>();
    public ChartExchangeOptionChainDataDto[] OptionChainData { get; set; } = Array.Empty<ChartExchangeOptionChainDataDto>();
    public ChartExchangeStockSplitDataDto[] StockSplitData { get; set; } = Array.Empty<ChartExchangeStockSplitDataDto>();
    public ChartExchangeShortInterestDataDto[] ShortInterestData { get; set; } = Array.Empty<ChartExchangeShortInterestDataDto>();
    public ChartExchangeShortVolumeDataDto[] ShortVolumeData { get; set; } = Array.Empty<ChartExchangeShortVolumeDataDto>();
    public ChartExchangeBorrowFeeDataDto[] BorrowFeeData { get; set; } = Array.Empty<ChartExchangeBorrowFeeDataDto>();
    public ChartExchangeBorrowFeeDailyDataDto[] BorrowFeeDailyData { get; set; } = Array.Empty<ChartExchangeBorrowFeeDailyDataDto>();
}