using System;
using System.Collections.Generic;

namespace StockDataApi.Models.Responses
{
    /// <summary>
    /// Unified response containing all stock data types
    /// </summary>
    public class StockDataResponse
    {
        public string Symbol { get; set; } = string.Empty;

        public ChartExchangeDataDto ChartExchangeData { get; set; } = new ChartExchangeDataDto();

        public FinraDataDto FinraData { get; set; } = new FinraDataDto();
    }
}
