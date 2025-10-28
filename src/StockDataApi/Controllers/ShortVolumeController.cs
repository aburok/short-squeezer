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
    public class ShortVolumeController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ShortVolumeController> _logger;

        public ShortVolumeController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<ShortVolumeController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets short volume data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., BYND)</param>
        /// <param name="startDate">Optional start date filter (format: yyyy-MM-dd)</param>
        /// <param name="endDate">Optional end date filter (format: yyyy-MM-dd)</param>
        /// <returns>A list of short volume data points</returns>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<IEnumerable<ShortVolumeDataDto>>> GetShortVolumeData(
            string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"ShortVolume_{symbol}_{startDate}_{endDate}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out List<ShortVolumeDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached short volume data for {Symbol}", symbol);
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

                // Query for short volume data
                var query = _context.ShortVolumeData
                    .Where(d => d.StockTickerSymbol == symbol);

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
                    .Select(d => new ShortVolumeDataDto
                    {
                        Date = d.Date,
                        ShortVolume = d.ShortVolume,
                        ShortVolumePercent = d.ShortVolumePercent
                    })
                    .ToListAsync();

                // If no data found in database, return empty list
                // Data should be fetched from Polygon.io instead

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} short volume data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving short volume data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets the latest short volume for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., BYND)</param>
        /// <returns>The latest short volume data point</returns>
        [HttpGet("{symbol}/latest")]
        public async Task<ActionResult<ShortVolumeDataDto>> GetLatestShortVolume(string symbol)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"LatestShortVolume_{symbol}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out ShortVolumeDataDto cachedData))
                {
                    _logger.LogInformation("Returning cached latest short volume for {Symbol}", symbol);
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

                // Get the latest short volume data
                var latestData = await _context.ShortVolumeData
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderByDescending(d => d.Date)
                    .Select(d => new ShortVolumeDataDto
                    {
                        Date = d.Date,
                        ShortVolume = d.ShortVolume,
                        ShortVolumePercent = d.ShortVolumePercent
                    })
                    .FirstOrDefaultAsync();

                // If no data found, return null
                // Data should be fetched from Polygon.io instead

                if (latestData == null)
                {
                    _logger.LogWarning("No short volume data found for {Symbol}", symbol);
                    return NotFound($"No short volume data found for {symbol}");
                }

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                _cache.Set(cacheKey, latestData, cacheOptions);

                _logger.LogInformation("Retrieved latest short volume for {Symbol}: {ShortVolumePercent}% on {Date}", 
                    symbol, latestData.ShortVolumePercent, latestData.Date);
                
                return latestData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest short volume for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Refreshes short volume data for a specific symbol (deprecated - use Polygon instead)
        /// </summary>
        /// <param name="symbol">The stock symbol</param>
        /// <returns>Method not available</returns>
        [HttpPost("refresh/{symbol}")]
        public async Task<ActionResult> RefreshShortVolumeData(
            string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            return NotFound("This endpoint is no longer available. Please use Polygon.io endpoints for short volume data.");
        }
    }

    /// <summary>
    /// Data transfer object for short volume data
    /// </summary>
    public class ShortVolumeDataDto
    {
        public DateTime Date { get; set; }
        public long ShortVolume { get; set; }
        public decimal ShortVolumePercent { get; set; }
    }
}

// Made with Bob
