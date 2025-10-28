namespace StockDataApi.Models.Commands
{
    /// <summary>
    /// Command to fetch and store Polygon data for a symbol
    /// </summary>
    public class FetchPolygonDataCommand
    {
        public string Symbol { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from the fetch command
    /// </summary>
    public class FetchPolygonDataCommandResult
    {
        public bool Success { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public DataFetchResult Prices { get; set; } = new DataFetchResult();
        public DataFetchResult ShortInterest { get; set; } = new DataFetchResult();
        public DataFetchResult ShortVolume { get; set; } = new DataFetchResult();
        public string? ErrorMessage { get; set; }
    }

    public class DataFetchResult
    {
        public int Fetched { get; set; }
        public bool Skipped { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
