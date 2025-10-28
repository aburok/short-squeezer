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
    /// Command handler for fetching and storing Polygon data
    /// </summary>
    public class FetchPolygonDataCommandHandler
    {
        private readonly StockDataContext _context;
        private readonly IPolygonService _polygonService;
        private readonly ILogger<FetchPolygonDataCommandHandler> _logger;

        public FetchPolygonDataCommandHandler(
            StockDataContext context,
            IPolygonService polygonService,
            ILogger<FetchPolygonDataCommandHandler> logger)
        {
            _context = context;
            _polygonService = polygonService;
            _logger = logger;
        }

        public async Task<FetchPolygonDataCommandResult> Handle(FetchPolygonDataCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var symbol = command.Symbol.ToUpper();
                _logger.LogInformation("Fetching all Polygon data for {Symbol}...", symbol);

                // Ensure ticker exists
                var ticker = await _context.StockTickers.FindAsync(new object[] { symbol }, cancellationToken);
                if (ticker == null)
                {
                    ticker = new StockTicker
                    {
                        Symbol = symbol,
                        Exchange = "NYSE",
                        Name = symbol,
                        LastUpdated = DateTime.Now
                    };
                    _context.StockTickers.Add(ticker);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var today = DateTime.Now.Date;
                var result = new FetchPolygonDataCommandResult
                {
                    Success = true,
                    Symbol = symbol
                };

                // Fetch price data
                result.Prices = await FetchPriceDataAsync(symbol, today, cancellationToken);

                // Fetch short interest data
                result.ShortInterest = await FetchShortInterestDataAsync(symbol, today, cancellationToken);

                // Fetch short volume data
                result.ShortVolume = await FetchShortVolumeDataAsync(symbol, today, cancellationToken);

                _logger.LogInformation("Completed fetching all Polygon data for {Symbol}", symbol);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Polygon data for {Symbol}", command.Symbol);
                return new FetchPolygonDataCommandResult
                {
                    Success = false,
                    Symbol = command.Symbol,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<DataFetchResult> FetchPriceDataAsync(string symbol, DateTime today, CancellationToken cancellationToken)
        {
            bool hasTodayPrice = await _context.PolygonPriceData
                .AnyAsync(d => d.StockTickerSymbol == symbol && d.Date.Date == today, cancellationToken);

            if (hasTodayPrice)
            {
                _logger.LogInformation("Price data for today already exists for {Symbol}, skipping fetch", symbol);
                return new DataFetchResult
                {
                    Skipped = true,
                    Message = "Price data for today already exists"
                };
            }

            _logger.LogInformation("Fetching price data for {Symbol}...", symbol);
            var priceData = await _polygonService.GetHistoricalDataAsync(symbol, 2);
            
            if (!priceData.Any())
            {
                return new DataFetchResult { Fetched = 0, Message = "No price data fetched" };
            }

            var existingDates = await _context.PolygonPriceData
                .Where(d => d.StockTickerSymbol == symbol)
                .Select(d => d.Date)
                .ToListAsync(cancellationToken);

            var newPriceData = priceData
                .Where(d => !existingDates.Contains(d.Date))
                .ToList();

            if (newPriceData.Any())
            {
                _context.PolygonPriceData.AddRange(newPriceData);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Fetched {Count} new price records for {Symbol}", newPriceData.Count, symbol);
            }

            return new DataFetchResult
            {
                Fetched = newPriceData.Count,
                Message = $"Fetched {newPriceData.Count} price records"
            };
        }

        private async Task<DataFetchResult> FetchShortInterestDataAsync(string symbol, DateTime today, CancellationToken cancellationToken)
        {
            bool hasTodayShortInterest = await _context.PolygonShortInterestData
                .AnyAsync(d => d.StockTickerSymbol == symbol && d.Date.Date == today, cancellationToken);

            if (hasTodayShortInterest)
            {
                _logger.LogInformation("Short interest data for today already exists for {Symbol}, skipping fetch", symbol);
                return new DataFetchResult
                {
                    Skipped = true,
                    Message = "Short interest data for today already exists"
                };
            }

            _logger.LogInformation("Fetching short interest data for {Symbol}...", symbol);
            var shortInterestData = await _polygonService.GetShortInterestDataAsync(symbol);
            
            if (!shortInterestData.Any())
            {
                return new DataFetchResult { Fetched = 0, Message = "No short interest data fetched" };
            }

            var existingDates = await _context.PolygonShortInterestData
                .Where(d => d.StockTickerSymbol == symbol)
                .Select(d => d.Date)
                .ToListAsync(cancellationToken);

            var newSI = shortInterestData
                .Where(d => !existingDates.Contains(d.Date))
                .ToList();

            if (newSI.Any())
            {
                _context.PolygonShortInterestData.AddRange(newSI);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Fetched {Count} new short interest records for {Symbol}", newSI.Count, symbol);
            }

            return new DataFetchResult
            {
                Fetched = newSI.Count,
                Message = $"Fetched {newSI.Count} short interest records"
            };
        }

        private async Task<DataFetchResult> FetchShortVolumeDataAsync(string symbol, DateTime today, CancellationToken cancellationToken)
        {
            bool hasTodayShortVolume = await _context.PolygonShortVolumeData
                .AnyAsync(d => d.StockTickerSymbol == symbol && d.Date.Date == today, cancellationToken);

            if (hasTodayShortVolume)
            {
                _logger.LogInformation("Short volume data for today already exists for {Symbol}, skipping fetch", symbol);
                return new DataFetchResult
                {
                    Skipped = true,
                    Message = "Short volume data for today already exists"
                };
            }

            _logger.LogInformation("Fetching short volume data for {Symbol}...", symbol);
            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate.AddYears(-2);

            var shortVolumeData = await _polygonService.GetShortVolumeDataAsync(symbol, startDate, endDate);
            
            if (!shortVolumeData.Any())
            {
                return new DataFetchResult { Fetched = 0, Message = "No short volume data fetched" };
            }

            var existingDates = await _context.PolygonShortVolumeData
                .Where(d => d.StockTickerSymbol == symbol)
                .Select(d => d.Date)
                .ToListAsync(cancellationToken);

            var newSV = shortVolumeData
                .Where(d => !existingDates.Contains(d.Date))
                .ToList();

            if (newSV.Any())
            {
                _context.PolygonShortVolumeData.AddRange(newSV);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Fetched {Count} new short volume records for {Symbol}", newSV.Count, symbol);
            }

            return new DataFetchResult
            {
                Fetched = newSV.Count,
                Message = $"Fetched {newSV.Count} short volume records"
            };
        }
    }
}
