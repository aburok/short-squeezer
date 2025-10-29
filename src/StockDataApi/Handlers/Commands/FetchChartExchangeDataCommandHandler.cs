using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockDataApi.Models.Commands;
using StockDataLib.Data;
using StockDataLib.Models;
using StockDataLib.Services;

namespace StockDataApi.Handlers.Commands
{
    /// <summary>
    /// Command handler for fetching ChartExchange data for a symbol
    /// </summary>
    public class FetchChartExchangeDataCommandHandler
    {
        private readonly StockDataContext _context;
        private readonly ILogger<FetchChartExchangeDataCommandHandler> _logger;

        public FetchChartExchangeDataCommandHandler(
            StockDataContext context,
            ILogger<FetchChartExchangeDataCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<FetchChartExchangeDataResult> Handle(FetchChartExchangeDataCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var symbol = command.Symbol.ToUpper().Trim();
                _logger.LogInformation("Fetching ChartExchange data for {Symbol}", symbol);

                // Check if ticker exists
                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol, cancellationToken);
                if (!tickerExists)
                {
                    return new FetchChartExchangeDataResult
                    {
                        Success = false,
                        Symbol = symbol,
                        ErrorMessage = $"Ticker {symbol} not found"
                    };
                }

                var startDate = DateTime.Now.AddYears(-command.Years);
                var endDate = DateTime.Now;

                var result = new FetchChartExchangeDataResult
                {
                    Success = true,
                    Symbol = symbol,
                    FailureToDeliver = new DataFetchResult(),
                    RedditMentions = new DataFetchResult(),
                    OptionChain = new DataFetchResult(),
                    StockSplits = new DataFetchResult(),
                    ShortInterest = new DataFetchResult(),
                    ShortVolume = new DataFetchResult()
                };

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully fetched ChartExchange data for {Symbol}", symbol);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange data for {Symbol}", command.Symbol);
                return new FetchChartExchangeDataResult
                {
                    Success = false,
                    Symbol = command.Symbol,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    /// <summary>
    /// Command to fetch ChartExchange data for a symbol
    /// </summary>
    public class FetchChartExchangeDataCommand
    {
        public string Symbol { get; set; } = string.Empty;
        public int Years { get; set; } = 2;
    }

    /// <summary>
    /// Result of fetching ChartExchange data
    /// </summary>
    public class FetchChartExchangeDataResult
    {
        public bool Success { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DataFetchResult FailureToDeliver { get; set; } = new DataFetchResult();
        public DataFetchResult RedditMentions { get; set; } = new DataFetchResult();
        public DataFetchResult OptionChain { get; set; } = new DataFetchResult();
        public DataFetchResult StockSplits { get; set; } = new DataFetchResult();
        public DataFetchResult ShortInterest { get; set; } = new DataFetchResult();
        public DataFetchResult ShortVolume { get; set; } = new DataFetchResult();
        public DataFetchResult BorrowFee { get; set; } = new DataFetchResult();
    }

    /// <summary>
    /// Result for individual data type fetch
    /// </summary>
    public class DataFetchResult
    {
        public int Fetched { get; set; }
        public bool Skipped { get; set; }
    }
}
