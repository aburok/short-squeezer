using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StockDataApi.Models.Responses;
using StockDataLib.Data;
using StockDataLib.Models;

namespace StockDataApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowFeeController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BorrowFeeController> _logger;

        public BorrowFeeController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<BorrowFeeController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets all borrow fee data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all borrow fee data points</returns>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<IEnumerable<BorrowFeeDataDto>>> GetBorrowFeeData(string symbol)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"BorrowFee_{symbol}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out List<BorrowFeeDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached borrow fee data for {Symbol}", symbol);
                    return cachedData;
                }

                // Find the ticker
                var ticker = await _context.StockTickers
                    .FirstOrDefaultAsync(t => t.Symbol == symbol);

                if (ticker == null)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                // Query for ALL borrow fee data (no date filtering)
                var data = await _context.BorrowFeeData
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new BorrowFeeDataDto
                    {
                        Date = d.Date,
                        Fee = d.Fee,
                        AvailableShares = d.AvailableShares
                    })
                    .ToListAsync();

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} borrow fee data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrow fee data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets the latest borrow fee for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>The latest borrow fee data point</returns>
        [HttpGet("{symbol}/latest")]
        public async Task<ActionResult<BorrowFeeDataDto>> GetLatestBorrowFee(string symbol)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"LatestBorrowFee_{symbol}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out BorrowFeeDataDto cachedData))
                {
                    _logger.LogInformation("Returning cached latest borrow fee for {Symbol}", symbol);
                    return cachedData;
                }

                // Find the ticker
                var ticker = await _context.StockTickers
                    .FirstOrDefaultAsync(t => t.Symbol == symbol);

                if (ticker == null)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                // Get the latest borrow fee data
                var latestData = await _context.BorrowFeeData
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderByDescending(d => d.Date)
                    .Select(d => new BorrowFeeDataDto
                    {
                        Date = d.Date,
                        Fee = d.Fee,
                        AvailableShares = d.AvailableShares
                    })
                    .FirstOrDefaultAsync();

                if (latestData == null)
                {
                    _logger.LogWarning("No borrow fee data found for {Symbol}", symbol);
                    return NotFound($"No borrow fee data found for {symbol}");
                }

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                _cache.Set(cacheKey, latestData, cacheOptions);

                _logger.LogInformation("Retrieved latest borrow fee for {Symbol}: {Fee}% on {Date}",
                    symbol, latestData.Fee, latestData.Date);

                return latestData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest borrow fee for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }
    }
}

// Made with Bob
