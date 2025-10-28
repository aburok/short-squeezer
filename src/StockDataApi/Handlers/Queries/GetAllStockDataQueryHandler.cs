using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StockDataApi.Models.Queries;
using StockDataApi.Models.Responses;
using StockDataLib.Data;

namespace StockDataApi.Handlers.Queries
{
    /// <summary>
    /// Query handler for retrieving all stock data for a ticker
    /// </summary>
    public class GetAllStockDataQueryHandler
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GetAllStockDataQueryHandler> _logger;

        public GetAllStockDataQueryHandler(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<GetAllStockDataQueryHandler> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<StockDataResponse> Handle(GetAllStockDataQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                var symbol = query.Symbol.ToUpper().Trim();

                var cacheKey = $"StockData_{symbol}_{query.StartDate}_{query.EndDate}_{query.IncludeBorrowFee}_{query.IncludePolygon}_{query.IncludeFinra}";
                
                if (_cache.TryGetValue(cacheKey, out StockDataResponse cachedData))
                {
                    _logger.LogInformation("Returning cached stock data for {Symbol}", symbol);
                    return cachedData;
                }

                var response = new StockDataResponse { Symbol = symbol };

                // Fetch borrow fee data
                if (query.IncludeBorrowFee)
                {
                    response.BorrowFeeData = await GetBorrowFeeDataAsync(symbol, query.StartDate, query.EndDate, cancellationToken);
                }

                // Fetch Polygon data
                if (query.IncludePolygon)
                {
                    response.PolygonData = await GetPolygonDataAsync(symbol, query.StartDate, query.EndDate, cancellationToken);
                }

                // Fetch FINRA data
                if (query.IncludeFinra)
                {
                    response.FinraData = await GetFinraDataAsync(symbol, query.StartDate, query.EndDate, cancellationToken);
                }

                // Cache for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                _cache.Set(cacheKey, response, cacheOptions);

                _logger.LogInformation("Retrieved stock data for {Symbol}", symbol);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock data for {Symbol}", query.Symbol);
                throw;
            }
        }

        private async Task<BorrowFeeDataDto[]> GetBorrowFeeDataAsync(string symbol, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var query = _context.BorrowFeeData.Where(d => d.StockTickerSymbol == symbol);

            if (startDate.HasValue)
                query = query.Where(d => d.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(d => d.Date <= endDate.Value.Date);

            return await query
                .OrderBy(d => d.Date)
                .Select(d => new BorrowFeeDataDto
                {
                    Date = d.Date,
                    Fee = d.Fee,
                    AvailableShares = d.AvailableShares
                })
                .ToArrayAsync(cancellationToken);
        }

        private async Task<PolygonDataDto> GetPolygonDataAsync(string symbol, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var polygonData = new PolygonDataDto();

            // Price data
            var priceQuery = _context.PolygonPriceData.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) priceQuery = priceQuery.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) priceQuery = priceQuery.Where(d => d.Date <= endDate.Value.Date);

            polygonData.PriceData = await priceQuery
                .OrderBy(d => d.Date)
                .Select(d => new PolygonPriceDataDto
                {
                    Date = d.Date,
                    Open = d.Open,
                    High = d.High,
                    Low = d.Low,
                    Close = d.Close,
                    Volume = d.Volume
                })
                .ToArrayAsync(cancellationToken);

            // Short interest data
            var shortInterestQuery = _context.PolygonShortInterestData.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) shortInterestQuery = shortInterestQuery.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) shortInterestQuery = shortInterestQuery.Where(d => d.Date <= endDate.Value.Date);

            polygonData.ShortInterestData = await shortInterestQuery
                .OrderBy(d => d.Date)
                .Select(d => new PolygonShortInterestDto
                {
                    Date = d.Date,
                    ShortInterest = d.ShortInterest,
                    AvgDailyVolume = d.AvgDailyVolume,
                    DaysToCover = d.DaysToCover
                })
                .ToArrayAsync(cancellationToken);

            // Short volume data
            var shortVolumeQuery = _context.PolygonShortVolumeData.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) shortVolumeQuery = shortVolumeQuery.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) shortVolumeQuery = shortVolumeQuery.Where(d => d.Date <= endDate.Value.Date);

            polygonData.ShortVolumeData = await shortVolumeQuery
                .OrderBy(d => d.Date)
                .Select(d => new PolygonShortVolumeDto
                {
                    Date = d.Date,
                    ShortVolume = d.ShortVolume,
                    TotalVolume = d.TotalVolume,
                    ShortVolumeRatio = d.ShortVolumeRatio
                })
                .ToArrayAsync(cancellationToken);

            return polygonData;
        }

        private async Task<FinraDataDto> GetFinraDataAsync(string symbol, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var finraData = new FinraDataDto();

            var query = _context.FinraShortInterestData.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) query = query.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(d => d.Date <= endDate.Value.Date);

            finraData.ShortInterestData = await query
                .OrderBy(d => d.Date)
                .Select(d => new FinraShortInterestDto
                {
                    Date = d.Date,
                    ShortInterest = d.ShortInterest,
                    SharesOutstanding = d.SharesOutstanding,
                    ShortInterestPercent = d.ShortInterestPercent,
                    Days2Cover = d.Days2Cover
                })
                .ToArrayAsync(cancellationToken);

            return finraData;
        }
    }
}
