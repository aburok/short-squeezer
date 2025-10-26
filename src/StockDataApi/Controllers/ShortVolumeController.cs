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
    public class ShortVolumeController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ShortVolumeController> _logger;
        private readonly IChartExchangeService _chartExchangeService;

        public ShortVolumeController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<ShortVolumeController> logger,
            IChartExchangeService chartExchangeService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _chartExchangeService = chartExchangeService;
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
                    .Select(d => new ShortVolumeDataDto
                    {
                        Date = d.Date,
                        ShortVolume = d.ShortVolume,
                        ShortVolumePercent = d.ShortVolumePercent
                    })
                    .ToListAsync();

                // If no data found, try to fetch from Chart Exchange
                if (data.Count == 0)
                {
                    _logger.LogInformation("No short volume data found in database for {Symbol}, fetching from Chart Exchange", symbol);
                    
                    // Determine exchange (simplified - in a real app you'd have a more robust way to determine this)
                    string exchange = DetermineExchange(symbol);
                    
                    var chartExchangeData = await _chartExchangeService.GetShortVolumeDataAsync(symbol, exchange, startDate, endDate);
                    
                    if (chartExchangeData.Any())
                    {
                        // Save to database
                        foreach (var item in chartExchangeData)
                        {
                            item.StockTickerId = ticker.Id;
                            _context.ShortVolumeData.Add(item);
                        }
                        
                        await _context.SaveChangesAsync();
                        
                        // Map to DTOs
                        data = chartExchangeData
                            .OrderBy(d => d.Date)
                            .Select(d => new ShortVolumeDataDto
                            {
                                Date = d.Date,
                                ShortVolume = d.ShortVolume,
                                ShortVolumePercent = d.ShortVolumePercent
                            })
                            .ToList();
                    }
                }

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
                    .Where(d => d.StockTickerId == ticker.Id)
                    .OrderByDescending(d => d.Date)
                    .Select(d => new ShortVolumeDataDto
                    {
                        Date = d.Date,
                        ShortVolume = d.ShortVolume,
                        ShortVolumePercent = d.ShortVolumePercent
                    })
                    .FirstOrDefaultAsync();

                // If no data found, try to fetch from Chart Exchange
                if (latestData == null)
                {
                    _logger.LogInformation("No short volume data found in database for {Symbol}, fetching from Chart Exchange", symbol);
                    
                    // Determine exchange (simplified - in a real app you'd have a more robust way to determine this)
                    string exchange = DetermineExchange(symbol);
                    
                    var chartExchangeData = await _chartExchangeService.GetShortVolumeDataAsync(symbol, exchange, null, null);
                    
                    if (chartExchangeData.Any())
                    {
                        // Save to database
                        foreach (var item in chartExchangeData)
                        {
                            item.StockTickerId = ticker.Id;
                            _context.ShortVolumeData.Add(item);
                        }
                        
                        await _context.SaveChangesAsync();
                        
                        // Get the latest data
                        latestData = chartExchangeData
                            .OrderByDescending(d => d.Date)
                            .Select(d => new ShortVolumeDataDto
                            {
                                Date = d.Date,
                                ShortVolume = d.ShortVolume,
                                ShortVolumePercent = d.ShortVolumePercent
                            })
                            .FirstOrDefault();
                    }
                }

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
        /// Refreshes short volume data for a specific symbol from Chart Exchange
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., BYND)</param>
        /// <returns>The refreshed short volume data</returns>
        [HttpPost("refresh/{symbol}")]
        public async Task<ActionResult<IEnumerable<ShortVolumeDataDto>>> RefreshShortVolumeData(
            string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();
                
                // Find the ticker
                var ticker = await _context.StockTickers
                    .FirstOrDefaultAsync(t => t.Symbol == symbol);

                if (ticker == null)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                // Determine exchange (simplified - in a real app you'd have a more robust way to determine this)
                string exchange = DetermineExchange(symbol);
                
                // Fetch data from Chart Exchange
                var chartExchangeData = await _chartExchangeService.GetShortVolumeDataAsync(symbol, exchange, startDate, endDate);
                
                // Log date range if provided
                if (startDate.HasValue && endDate.HasValue)
                {
                    _logger.LogInformation("Using date range: {StartDate} to {EndDate}",
                        startDate.Value.ToString("yyyy-MM-dd"),
                        endDate.Value.ToString("yyyy-MM-dd"));
                }
                
                if (!chartExchangeData.Any())
                {
                    _logger.LogWarning("No short volume data found from Chart Exchange for {Symbol}", symbol);
                    return NotFound($"No short volume data found from Chart Exchange for {symbol}");
                }

                // Get existing data dates to avoid duplicates
                var existingDates = await _context.ShortVolumeData
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
                        _context.ShortVolumeData.Add(item);
                        addedCount++;
                    }
                }
                
                if (addedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // Clear cache
                _cache.Remove($"ShortVolume_{symbol}_{startDate}_{endDate}");
                _cache.Remove($"ShortVolume_{symbol}_");
                _cache.Remove($"LatestShortVolume_{symbol}");

                // Map to DTOs
                var data = chartExchangeData
                    .OrderBy(d => d.Date)
                    .Select(d => new ShortVolumeDataDto
                    {
                        Date = d.Date,
                        ShortVolume = d.ShortVolume,
                        ShortVolumePercent = d.ShortVolumePercent
                    })
                    .ToList();

                _logger.LogInformation("Refreshed short volume data for {Symbol}. Added {AddedCount} new records.", symbol, addedCount);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing short volume data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while refreshing the data");
            }
        }

        /// <summary>
        /// Determines the exchange for a given symbol (simplified implementation)
        /// </summary>
        /// <param name="symbol">The stock symbol</param>
        /// <returns>The exchange name</returns>
        private string DetermineExchange(string symbol)
        {
            // This is a simplified implementation
            // In a real application, you would have a more robust way to determine the exchange
            // For example, you might have a database table mapping symbols to exchanges
            
            // For now, we'll use some common patterns
            if (symbol == "BYND")
            {
                return "nasdaq";
            }
            else if (symbol == "SPY" || symbol == "GME" || symbol == "AMC")
            {
                return "nyse";
            }
            
            // Default to nasdaq
            return "nasdaq";
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
