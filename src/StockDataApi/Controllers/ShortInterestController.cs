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
using StockDataLib.Services;

namespace StockDataApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortInterestController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ShortInterestController> _logger;
        private readonly IChartExchangeService _chartExchangeService;

        public ShortInterestController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<ShortInterestController> logger,
            IChartExchangeService chartExchangeService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _chartExchangeService = chartExchangeService;
        }

        /// <summary>
        /// Gets short interest data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., SPY)</param>
        /// <param name="startDate">Optional start date filter (format: yyyy-MM-dd)</param>
        /// <param name="endDate">Optional end date filter (format: yyyy-MM-dd)</param>
        /// <returns>A list of short interest data points</returns>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<IEnumerable<ShortInterestDataDto>>> GetShortInterestData(
            string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"ShortInterest_{symbol}_{startDate}_{endDate}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out List<ShortInterestDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached short interest data for {Symbol}", symbol);
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

                // Query for short interest data
                var query = _context.ShortInterestData
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
                    .Select(d => new ShortInterestDataDto
                    {
                        Date = d.Date,
                        ShortInterest = d.ShortInterest,
                        SharesShort = d.SharesShort
                    })
                    .ToListAsync();

                // If no data found, try to fetch from Chart Exchange
                if (data.Count == 0 && symbol.Equals("SPY", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("No short interest data found in database for SPY, fetching from Chart Exchange");
                    var chartExchangeData = await _chartExchangeService.GetShortInterestDataAsync(symbol);
                    
                    if (chartExchangeData.Any())
                    {
                        // Save to database
                        foreach (var item in chartExchangeData)
                        {
                            item.StockTickerId = ticker.Id;
                            _context.ShortInterestData.Add(item);
                        }
                        
                        await _context.SaveChangesAsync();
                        
                        // Map to DTOs
                        data = chartExchangeData
                            .OrderBy(d => d.Date)
                            .Select(d => new ShortInterestDataDto
                            {
                                Date = d.Date,
                                ShortInterest = d.ShortInterest,
                                SharesShort = d.SharesShort
                            })
                            .ToList();
                    }
                }

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} short interest data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving short interest data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets the latest short interest for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., SPY)</param>
        /// <returns>The latest short interest data point</returns>
        [HttpGet("{symbol}/latest")]
        public async Task<ActionResult<ShortInterestDataDto>> GetLatestShortInterest(string symbol)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"LatestShortInterest_{symbol}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out ShortInterestDataDto cachedData))
                {
                    _logger.LogInformation("Returning cached latest short interest for {Symbol}", symbol);
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

                // Get the latest short interest data
                var latestData = await _context.ShortInterestData
                    .Where(d => d.StockTickerId == ticker.Id)
                    .OrderByDescending(d => d.Date)
                    .Select(d => new ShortInterestDataDto
                    {
                        Date = d.Date,
                        ShortInterest = d.ShortInterest,
                        SharesShort = d.SharesShort
                    })
                    .FirstOrDefaultAsync();

                // If no data found, try to fetch from Chart Exchange for SPY
                if (latestData == null && symbol.Equals("SPY", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("No short interest data found in database for SPY, fetching from Chart Exchange");
                    var chartExchangeData = await _chartExchangeService.GetShortInterestDataAsync(symbol);
                    
                    if (chartExchangeData.Any())
                    {
                        // Save to database
                        foreach (var item in chartExchangeData)
                        {
                            item.StockTickerId = ticker.Id;
                            _context.ShortInterestData.Add(item);
                        }
                        
                        await _context.SaveChangesAsync();
                        
                        // Get the latest data
                        latestData = chartExchangeData
                            .OrderByDescending(d => d.Date)
                            .Select(d => new ShortInterestDataDto
                            {
                                Date = d.Date,
                                ShortInterest = d.ShortInterest,
                                SharesShort = d.SharesShort
                            })
                            .FirstOrDefault();
                    }
                }

                if (latestData == null)
                {
                    _logger.LogWarning("No short interest data found for {Symbol}", symbol);
                    return NotFound($"No short interest data found for {symbol}");
                }

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                _cache.Set(cacheKey, latestData, cacheOptions);

                _logger.LogInformation("Retrieved latest short interest for {Symbol}: {ShortInterest}% on {Date}", 
                    symbol, latestData.ShortInterest, latestData.Date);
                
                return latestData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest short interest for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Refreshes short interest data for SPY from Chart Exchange
        /// </summary>
        /// <returns>The refreshed short interest data</returns>
        [HttpPost("refresh/spy")]
        public async Task<ActionResult<IEnumerable<ShortInterestDataDto>>> RefreshSpyShortInterestData()
        {
            try
            {
                string symbol = "SPY";
                
                // Find the ticker
                var ticker = await _context.StockTickers
                    .FirstOrDefaultAsync(t => t.Symbol == symbol);

                if (ticker == null)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                // Fetch data from Chart Exchange
                var chartExchangeData = await _chartExchangeService.GetShortInterestDataAsync(symbol);
                
                if (!chartExchangeData.Any())
                {
                    _logger.LogWarning("No short interest data found from Chart Exchange for {Symbol}", symbol);
                    return NotFound($"No short interest data found from Chart Exchange for {symbol}");
                }

                // Get existing data dates to avoid duplicates
                var existingDates = await _context.ShortInterestData
                    .Where(d => d.StockTickerId == ticker.Id)
                    .Select(d => d.Date.Date)
                    .ToListAsync();

                int addedCount = 0;
                
                // Save new data to database
                foreach (var item in chartExchangeData)
                {
                    if (!existingDates.Contains(item.Date.Date))
                    {
                        item.StockTickerId = ticker.Id;
                        _context.ShortInterestData.Add(item);
                        addedCount++;
                    }
                }
                
                if (addedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // Clear cache
                _cache.Remove($"ShortInterest_{symbol}_");
                _cache.Remove($"LatestShortInterest_{symbol}");

                // Map to DTOs
                var data = chartExchangeData
                    .OrderBy(d => d.Date)
                    .Select(d => new ShortInterestDataDto
                    {
                        Date = d.Date,
                        ShortInterest = d.ShortInterest,
                        SharesShort = d.SharesShort
                    })
                    .ToList();

                _logger.LogInformation("Refreshed short interest data for SPY. Added {AddedCount} new records.", addedCount);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing short interest data for SPY");
                return StatusCode(500, "An error occurred while refreshing the data");
            }
        }
    }

    /// <summary>
    /// Data transfer object for short interest data
    /// </summary>
    public class ShortInterestDataDto
    {
        public DateTime Date { get; set; }
        public decimal ShortInterest { get; set; }
        public long SharesShort { get; set; }
    }
}

// Made with Bob
