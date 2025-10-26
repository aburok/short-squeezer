using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StockDataLib.Data;
using StockDataLib.Models;

namespace StockDataApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriceController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PriceController> _logger;

        public PriceController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<PriceController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets price data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="startDate">Optional start date filter (format: yyyy-MM-dd)</param>
        /// <param name="endDate">Optional end date filter (format: yyyy-MM-dd)</param>
        /// <returns>A list of price data points</returns>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<IEnumerable<PriceDataDto>>> GetPriceData(
            string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"Price_{symbol}_{startDate}_{endDate}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out List<PriceDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached price data for {Symbol}", symbol);
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

                // Query for price data
                var query = _context.PriceData
                    .Where(d => d.StockTickerId == ticker.Id);

                // Apply date filters if provided
                if (startDate.HasValue)
                {
                    query = query.Where(d => d.Date >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(d => d.Date <= endDate.Value.Date);
                }

                // Execute query and map to DTOs
                var data = await query
                    .OrderBy(d => d.Date)
                    .Select(d => new PriceDataDto
                    {
                        Date = d.Date,
                        Open = d.Open,
                        High = d.High,
                        Low = d.Low,
                        Close = d.Close
                    })
                    .ToListAsync();

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} price data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving price data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets the latest price for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>The latest price data point</returns>
        [HttpGet("{symbol}/latest")]
        public async Task<ActionResult<PriceDataDto>> GetLatestPrice(string symbol)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"LatestPrice_{symbol}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out PriceDataDto cachedData))
                {
                    _logger.LogInformation("Returning cached latest price for {Symbol}", symbol);
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

                // Get the latest price data
                var latestData = await _context.PriceData
                    .Where(d => d.StockTickerId == ticker.Id)
                    .OrderByDescending(d => d.Date)
                    .Select(d => new PriceDataDto
                    {
                        Date = d.Date,
                        Open = d.Open,
                        High = d.High,
                        Low = d.Low,
                        Close = d.Close
                    })
                    .FirstOrDefaultAsync();

                if (latestData == null)
                {
                    _logger.LogWarning("No price data found for {Symbol}", symbol);
                    return NotFound($"No price data found for {symbol}");
                }

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                _cache.Set(cacheKey, latestData, cacheOptions);

                _logger.LogInformation("Retrieved latest price for {Symbol}: {Close} on {Date}", 
                    symbol, latestData.Close, latestData.Date);
                
                return latestData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest price for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }
    }

    /// <summary>
    /// Data transfer object for price data
    /// </summary>
    public class PriceDataDto
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
}

// Made with Bob
