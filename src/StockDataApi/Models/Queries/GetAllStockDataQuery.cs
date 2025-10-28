using System;

namespace StockDataApi.Models.Queries
{
    /// <summary>
    /// Query to retrieve all stock data for a ticker
    /// </summary>
    public class GetAllStockDataQuery
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Flags to control which data to include
        public bool IncludeBorrowFee { get; set; } = true;
        public bool IncludePolygon { get; set; } = true;
        public bool IncludeFinra { get; set; } = false;
    }
}
