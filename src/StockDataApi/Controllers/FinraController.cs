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
    public class FinraController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FinraController> _logger;
        private readonly IFinraService _finraService;

        public FinraController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<FinraController> logger,
            IFinraService finraService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _finraService = finraService;
        }

        /// <summary>
        /// Gets FINRA short interest data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="startDate">Optional start date filter (format: yyyy-MM-dd)</param>
        /// <param name="endDate">Optional end date filter (format: yyyy-MM-dd)</param>
        /// <returns>A list of FINRA short interest data points</returns>
        [HttpGet("short-interest/{symbol}")]
        public async Task<ActionResult<IEnumerable<FinraShortInterestDataDto>>> GetFinraShortInterestData(
            string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"FinraShortInterest_{symbol}_{startDate}_{endDate}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out List<FinraShortInterestDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached FINRA short interest data for {Symbol}", symbol);
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

                // Query for FINRA short interest data
                var query = _context.FinraShortInterestData
                    .Where(d => d.StockTickerSymbol == symbol);

                // Apply date filters if provided
                if (startDate.HasValue)
                {
                    query = query.Where(d => d.SettlementDate >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(d => d.SettlementDate <= endDate.Value.Date);
                }

                // Execute query and map to DTOs
                var data = await query
                    .OrderBy(d => d.SettlementDate)
                    .Select(d => new FinraShortInterestDataDto
                    {
                        Date = d.Date,
                        SettlementDate = d.SettlementDate,
                        ShortInterest = d.ShortInterest,
                        ShortInterestPercent = d.ShortInterestPercent,
                        MarketValue = d.MarketValue,
                        SharesOutstanding = d.SharesOutstanding,
                        AvgDailyVolume = d.AvgDailyVolume,
                        Days2Cover = d.Days2Cover
                    })
                    .ToListAsync();

                // If no data found, try to fetch from FINRA API
                if (data.Count == 0)
                {
                    _logger.LogInformation("No FINRA short interest data found in database for {Symbol}, fetching from FINRA API", symbol);
                    var finraData = await _finraService.GetShortInterestDataAsync(symbol, startDate, endDate);
                    
                    if (finraData.Any())
                    {
                        // Save to database
                        foreach (var item in finraData)
                        {
                            var dbItem = new FinraShortInterestData
                            {
                                StockTickerSymbol = symbol,
                                Date = item.SettlementDate,
                                SettlementDate = item.SettlementDate,
                                ShortInterest = item.ShortInterest,
                                ShortInterestPercent = item.ShortInterestPercent,
                                MarketValue = item.MarketValue,
                                SharesOutstanding = item.SharesOutstanding,
                                AvgDailyVolume = item.AvgDailyVolume,
                                Days2Cover = item.Days2Cover
                            };
                            _context.FinraShortInterestData.Add(dbItem);
                        }
                        
                        await _context.SaveChangesAsync();
                        
                        // Map to DTOs
                        data = finraData
                            .OrderBy(d => d.SettlementDate)
                            .Select(d => new FinraShortInterestDataDto
                            {
                                Date = d.SettlementDate,
                                SettlementDate = d.SettlementDate,
                                ShortInterest = d.ShortInterest,
                                ShortInterestPercent = d.ShortInterestPercent,
                                MarketValue = d.MarketValue,
                                SharesOutstanding = d.SharesOutstanding,
                                AvgDailyVolume = d.AvgDailyVolume,
                                Days2Cover = d.Days2Cover
                            })
                            .ToList();
                    }
                }

                // Cache the result for 30 minutes (FINRA data is updated twice monthly)
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} FINRA short interest data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FINRA short interest data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets the latest FINRA short interest for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>The latest FINRA short interest data point</returns>
        [HttpGet("short-interest/{symbol}/latest")]
        public async Task<ActionResult<FinraShortInterestDataDto>> GetLatestFinraShortInterest(string symbol)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"LatestFinraShortInterest_{symbol}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out FinraShortInterestDataDto cachedData))
                {
                    _logger.LogInformation("Returning cached latest FINRA short interest for {Symbol}", symbol);
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

                // Get the latest FINRA short interest data
                var latestData = await _context.FinraShortInterestData
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderByDescending(d => d.SettlementDate)
                    .Select(d => new FinraShortInterestDataDto
                    {
                        Date = d.Date,
                        SettlementDate = d.SettlementDate,
                        ShortInterest = d.ShortInterest,
                        ShortInterestPercent = d.ShortInterestPercent,
                        MarketValue = d.MarketValue,
                        SharesOutstanding = d.SharesOutstanding,
                        AvgDailyVolume = d.AvgDailyVolume,
                        Days2Cover = d.Days2Cover
                    })
                    .FirstOrDefaultAsync();

                // If no data found, try to fetch from FINRA API
                if (latestData == null)
                {
                    _logger.LogInformation("No FINRA short interest data found in database for {Symbol}, fetching from FINRA API", symbol);
                    var finraData = await _finraService.GetShortInterestDataAsync(symbol, null, null);
                    
                    if (finraData.Any())
                    {
                        // Save to database
                        foreach (var item in finraData)
                        {
                            var dbItem = new FinraShortInterestData
                            {
                                StockTickerSymbol = symbol,
                                Date = item.SettlementDate,
                                SettlementDate = item.SettlementDate,
                                ShortInterest = item.ShortInterest,
                                ShortInterestPercent = item.ShortInterestPercent,
                                MarketValue = item.MarketValue,
                                SharesOutstanding = item.SharesOutstanding,
                                AvgDailyVolume = item.AvgDailyVolume,
                                Days2Cover = item.Days2Cover
                            };
                            _context.FinraShortInterestData.Add(dbItem);
                        }
                        
                        await _context.SaveChangesAsync();
                        
                        // Get the latest data
                        latestData = finraData
                            .OrderByDescending(d => d.SettlementDate)
                            .Select(d => new FinraShortInterestDataDto
                            {
                                Date = d.SettlementDate,
                                SettlementDate = d.SettlementDate,
                                ShortInterest = d.ShortInterest,
                                ShortInterestPercent = d.ShortInterestPercent,
                                MarketValue = d.MarketValue,
                                SharesOutstanding = d.SharesOutstanding,
                                AvgDailyVolume = d.AvgDailyVolume,
                                Days2Cover = d.Days2Cover
                            })
                            .FirstOrDefault();
                    }
                }

                if (latestData == null)
                {
                    _logger.LogWarning("No FINRA short interest data found for {Symbol}", symbol);
                    return NotFound($"No FINRA short interest data found for {symbol}");
                }

                // Cache the result for 30 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                
                _cache.Set(cacheKey, latestData, cacheOptions);

                _logger.LogInformation("Retrieved latest FINRA short interest for {Symbol}: {ShortInterestPercent}% on {SettlementDate}", 
                    symbol, latestData.ShortInterestPercent, latestData.SettlementDate);
                
                return latestData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest FINRA short interest for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Refreshes FINRA short interest data for a specific symbol
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>The refreshed FINRA short interest data</returns>
        [HttpPost("short-interest/{symbol}/refresh")]
        public async Task<ActionResult<IEnumerable<FinraShortInterestDataDto>>> RefreshFinraShortInterestData(
            string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                
                // Find the ticker
                var ticker = await _context.StockTickers
                    .FirstOrDefaultAsync(t => t.Symbol == symbol);

                if (ticker == null)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                // Fetch data from FINRA API
                var finraData = await _finraService.GetShortInterestDataAsync(symbol, startDate, endDate);
                
                // Log date range if provided
                if (startDate.HasValue && endDate.HasValue)
                {
                    _logger.LogInformation("Using date range: {StartDate} to {EndDate}",
                        startDate.Value.ToString("yyyy-MM-dd"),
                        endDate.Value.ToString("yyyy-MM-dd"));
                }
                
                if (!finraData.Any())
                {
                    _logger.LogWarning("No FINRA short interest data found from FINRA API for {Symbol}", symbol);
                    return NotFound($"No FINRA short interest data found from FINRA API for {symbol}");
                }

                // Get existing data dates to avoid duplicates
                var existingDates = await _context.FinraShortInterestData
                    .Where(d => d.StockTickerSymbol == symbol)
                    .Select(d => d.SettlementDate.Date)
                    .ToListAsync();

                int addedCount = 0;
                
                // Save new data to database
                foreach (var item in finraData)
                {
                    if (!existingDates.Contains(item.SettlementDate.Date))
                    {
                        var dbItem = new FinraShortInterestData
                        {
                            StockTickerSymbol = symbol,
                            Date = item.SettlementDate,
                            SettlementDate = item.SettlementDate,
                            ShortInterest = item.ShortInterest,
                            ShortInterestPercent = item.ShortInterestPercent,
                            MarketValue = item.MarketValue,
                            SharesOutstanding = item.SharesOutstanding,
                            AvgDailyVolume = item.AvgDailyVolume,
                            Days2Cover = item.Days2Cover
                        };
                        _context.FinraShortInterestData.Add(dbItem);
                        addedCount++;
                    }
                }
                
                if (addedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // Clear cache
                _cache.Remove($"FinraShortInterest_{symbol}_{startDate}_{endDate}");
                _cache.Remove($"FinraShortInterest_{symbol}_");
                _cache.Remove($"LatestFinraShortInterest_{symbol}");

                // Map to DTOs
                var data = finraData
                    .OrderBy(d => d.SettlementDate)
                    .Select(d => new FinraShortInterestDataDto
                    {
                        Date = d.SettlementDate,
                        SettlementDate = d.SettlementDate,
                        ShortInterest = d.ShortInterest,
                        ShortInterestPercent = d.ShortInterestPercent,
                        MarketValue = d.MarketValue,
                        SharesOutstanding = d.SharesOutstanding,
                        AvgDailyVolume = d.AvgDailyVolume,
                        Days2Cover = d.Days2Cover
                    })
                    .ToList();

                _logger.LogInformation("Refreshed FINRA short interest data for {Symbol}. Added {AddedCount} new records.", symbol, addedCount);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing FINRA short interest data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while refreshing the data");
            }
        }

        /// <summary>
        /// Gets FINRA short interest data for all tickers (bulk data)
        /// </summary>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="limit">Maximum number of records to return (default: 1000)</param>
        /// <returns>A list of FINRA short interest data points</returns>
        [HttpGet("short-interest")]
        public async Task<ActionResult<IEnumerable<FinraShortInterestDataDto>>> GetAllFinraShortInterestData(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int limit = 1000)
        {
            try
            {
                // Create cache key
                string cacheKey = $"AllFinraShortInterest_{startDate}_{endDate}_{limit}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out List<FinraShortInterestDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached all FINRA short interest data");
                    return cachedData;
                }

                // Query for FINRA short interest data
                var query = _context.FinraShortInterestData.AsQueryable();

                // Apply date filters if provided
                if (startDate.HasValue)
                {
                    query = query.Where(d => d.SettlementDate >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(d => d.SettlementDate <= endDate.Value.Date);
                }

                // Execute query and map to DTOs
                var data = await query
                    .OrderByDescending(d => d.SettlementDate)
                    .ThenBy(d => d.StockTickerSymbol)
                    .Take(limit)
                    .Select(d => new FinraShortInterestDataDto
                    {
                        Date = d.Date,
                        SettlementDate = d.SettlementDate,
                        ShortInterest = d.ShortInterest,
                        ShortInterestPercent = d.ShortInterestPercent,
                        MarketValue = d.MarketValue,
                        SharesOutstanding = d.SharesOutstanding,
                        AvgDailyVolume = d.AvgDailyVolume,
                        Days2Cover = d.Days2Cover,
                        Symbol = d.StockTickerSymbol
                    })
                    .ToListAsync();

                // Cache the result for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} FINRA short interest data points", data.Count);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all FINRA short interest data");
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Fetches and stores FINRA blocks summary data
        /// </summary>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="symbol">Optional symbol (MPID) filter</param>
        /// <returns>A list of blocks summary data points</returns>
        [HttpPost("blocks-summary/fetch")]
        public async Task<ActionResult<object>> FetchBlocksSummaryData(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? symbol = null)
        {
            try
            {
                _logger.LogInformation("Fetching FINRA blocks summary data...");
                
                // Fetch data from FINRA API
                var finraData = await _finraService.GetBlocksSummaryDataAsync(startDate, endDate, symbol);
                
                if (!finraData.Any())
                {
                    _logger.LogWarning("No FINRA blocks summary data found");
                    return new { 
                        success = false, 
                        message = "No data found", 
                        count = 0 
                    };
                }

                _logger.LogInformation("Retrieved {Count} blocks summary data points from FINRA", finraData.Count);
                
                return new { 
                    success = true, 
                    message = $"Successfully fetched {finraData.Count} data points", 
                    count = finraData.Count,
                    data = finraData 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FINRA blocks summary data");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while fetching the data",
                    error = ex.Message 
                });
            }
        }
    }

    /// <summary>
    /// Data transfer object for FINRA short interest data
    /// </summary>
    public class FinraShortInterestDataDto
    {
        public DateTime Date { get; set; }
        public DateTime SettlementDate { get; set; }
        public long ShortInterest { get; set; }
        public decimal ShortInterestPercent { get; set; }
        public decimal MarketValue { get; set; }
        public long SharesOutstanding { get; set; }
        public long AvgDailyVolume { get; set; }
        public decimal Days2Cover { get; set; }
        public string? Symbol { get; set; }
    }
}

// Made with Bob
