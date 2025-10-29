namespace StockDataLib;

public interface ITickerDataSynchronizationService
{
    Task Synchronize(string symbol);
}