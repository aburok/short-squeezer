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
        private readonly IChartExchangeService _chartExchangeService;
        private readonly ILogger<FetchChartExchangeDataCommandHandler> _logger;

        public FetchChartExchangeDataCommandHandler(
            StockDataContext context,
            IChartExchangeService chartExchangeService,
            ILogger<FetchChartExchangeDataCommandHandler> logger)
        {
            _context = context;
            _chartExchangeService = chartExchangeService;
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

                // Fetch and store failure to deliver data
                var failureToDeliverData = await _chartExchangeService.GetFailureToDeliverDataAsync(symbol, startDate, endDate);
                if (failureToDeliverData.Any())
                {
                    var existingDates = await _context.ChartExchangeFailureToDeliver
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync(cancellationToken);

                    var newFailureToDeliverData = failureToDeliverData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newFailureToDeliverData.Any())
                    {
                        _context.ChartExchangeFailureToDeliver.AddRange(newFailureToDeliverData);
                        result.FailureToDeliver.Fetched = newFailureToDeliverData.Count;
                    }
                    else
                    {
                        result.FailureToDeliver.Skipped = true;
                    }
                }

                // Fetch and store Reddit mentions data
                var redditMentionsData = await _chartExchangeService.GetRedditMentionsDataAsync(symbol, startDate, endDate);
                if (redditMentionsData.Any())
                {
                    var existingDates = await _context.ChartExchangeRedditMentions
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync(cancellationToken);

                    var newRedditMentionsData = redditMentionsData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newRedditMentionsData.Any())
                    {
                        _context.ChartExchangeRedditMentions.AddRange(newRedditMentionsData);
                        result.RedditMentions.Fetched = newRedditMentionsData.Count;
                    }
                    else
                    {
                        result.RedditMentions.Skipped = true;
                    }
                }

                // Fetch and store stock split data
                var stockSplitData = await _chartExchangeService.GetStockSplitDataAsync(symbol, startDate, endDate);
                if (stockSplitData.Any())
                {
                    var existingDates = await _context.ChartExchangeStockSplit
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync(cancellationToken);

                    var newStockSplitData = stockSplitData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newStockSplitData.Any())
                    {
                        _context.ChartExchangeStockSplit.AddRange(newStockSplitData);
                        result.StockSplits.Fetched = newStockSplitData.Count;
                    }
                    else
                    {
                        result.StockSplits.Skipped = true;
                    }
                }

                // Fetch and store short interest data
                var shortInterestData = await _chartExchangeService.GetShortInterestDataAsync(symbol, startDate, endDate);
                if (shortInterestData.Any())
                {
                    var existingDates = await _context.ChartExchangeShortInterest
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync(cancellationToken);

                    var newShortInterestData = shortInterestData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newShortInterestData.Any())
                    {
                        _context.ChartExchangeShortInterest.AddRange(newShortInterestData);
                        result.ShortInterest.Fetched = newShortInterestData.Count;
                    }
                    else
                    {
                        result.ShortInterest.Skipped = true;
                    }
                }

                // Fetch and store short volume data
                var shortVolumeData = await _chartExchangeService.GetShortVolumeDataAsync(symbol, startDate, endDate);
                if (shortVolumeData.Any())
                {
                    var existingDates = await _context.ChartExchangeShortVolume
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync(cancellationToken);

                    var newShortVolumeData = shortVolumeData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newShortVolumeData.Any())
                    {
                        _context.ChartExchangeShortVolume.AddRange(newShortVolumeData);
                        result.ShortVolume.Fetched = newShortVolumeData.Count;
                    }
                    else
                    {
                        result.ShortVolume.Skipped = true;
                    }
                }

                // Fetch and store borrow fee data
                var borrowFeeData = await _chartExchangeService.GetBorrowFeeDataAsync(symbol, startDate, endDate);
                if (borrowFeeData.Any())
                {
                    var existingDates = await _context.ChartExchangeBorrowFee
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync(cancellationToken);

                    var newBorrowFeeData = borrowFeeData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newBorrowFeeData.Any())
                    {
                        _context.ChartExchangeBorrowFee.AddRange(newBorrowFeeData);
                        result.BorrowFee.Fetched = newBorrowFeeData.Count;
                    }
                    else
                    {
                        result.BorrowFee.Skipped = true;
                    }
                }

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
