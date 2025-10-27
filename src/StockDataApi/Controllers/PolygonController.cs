using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockDataLib.Data;
using StockDataLib.Models;
using StockDataLib.Services;

namespace StockDataApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolygonController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IPolygonService _polygonService;
        private readonly ILogger<PolygonController> _logger;

        public PolygonController(
            StockDataContext context,
            IPolygonService polygonService,
            ILogger<PolygonController> logger)
        {
            _context = context;
            _polygonService = polygonService;
            _logger = logger;
        }

        /// <summary>
        /// Fetches and stores all Polygon data (price, short interest, short volume) for a symbol
        /// Checks if latest data is from today to avoid unnecessary API calls
        /// </summary>
        [HttpPost("{symbol}/fetch-all")]
        public async Task<ActionResult<object>> FetchAllPolygonData([FromRoute] string symbol)
        {
            try
            {
                _logger.LogInformation("Fetching all Polygon data for {Symbol}...", symbol);

                // Ensure ticker exists
                var ticker = await _context.StockTickers.FindAsync(symbol.ToUpper());
                if (ticker == null)
                {
                    ticker = new StockTicker
                    {
                        Symbol = symbol.ToUpper(),
                        Exchange = "NYSE",
                        Name = symbol.ToUpper(),
                        LastUpdated = DateTime.Now
                    };
                    _context.StockTickers.Add(ticker);
                    await _context.SaveChangesAsync();
                }

                var today = DateTime.Now.Date;
                int pricesFetched = 0;
                int shortInterestFetched = 0;
                int shortVolumeFetched = 0;
                bool pricesSkipped = false;
                bool shortInterestSkipped = false;
                bool shortVolumeSkipped = false;

                // Check and fetch price data if not today
                bool hasTodayPrice = await _context.PolygonPriceData
                    .AnyAsync(d => d.StockTickerSymbol == symbol.ToUpper() && d.Date.Date == today);
                
                if (hasTodayPrice)
                {
                    pricesSkipped = true;
                    _logger.LogInformation("Price data for today already exists for {Symbol}, skipping fetch", symbol);
                }
                else
                {
                    _logger.LogInformation("Fetching price data for {Symbol}...", symbol);
                    var priceData = await _polygonService.GetHistoricalDataAsync(symbol, 2);
                    if (priceData.Any())
                    {
                        var existingDates = await _context.PolygonPriceData
                            .Where(d => d.StockTickerSymbol == symbol.ToUpper())
                            .Select(d => d.Date)
                            .ToListAsync();
                        
                        var newPriceData = priceData
                            .Where(d => !existingDates.Contains(d.Date))
                            .ToList();
                        
                        if (newPriceData.Any())
                        {
                            _context.PolygonPriceData.AddRange(newPriceData);
                            await _context.SaveChangesAsync();
                            pricesFetched = newPriceData.Count;
                            _logger.LogInformation("Fetched {Count} new price records for {Symbol}", pricesFetched, symbol);
                        }
                    }
                }

                // Check and fetch short interest data if not today
                bool hasTodayShortInterest = await _context.PolygonShortInterestData
                    .AnyAsync(d => d.StockTickerSymbol == symbol.ToUpper() && d.Date.Date == today);
                
                if (hasTodayShortInterest)
                {
                    shortInterestSkipped = true;
                    _logger.LogInformation("Short interest data for today already exists for {Symbol}, skipping fetch", symbol);
                }
                else
                {
                    _logger.LogInformation("Fetching short interest data for {Symbol}...", symbol);
                    var shortInterestData = await _polygonService.GetShortInterestDataAsync(symbol);
                    if (shortInterestData.Any())
                    {
                        var existingDates = await _context.PolygonShortInterestData
                            .Where(d => d.StockTickerSymbol == symbol.ToUpper())
                            .Select(d => d.Date)
                            .ToListAsync();
                        
                        var newSI = shortInterestData
                            .Where(d => !existingDates.Contains(d.Date))
                            .ToList();
                        
                        if (newSI.Any())
                        {
                            _context.PolygonShortInterestData.AddRange(newSI);
                            await _context.SaveChangesAsync();
                            shortInterestFetched = newSI.Count;
                            _logger.LogInformation("Fetched {Count} new short interest records for {Symbol}", shortInterestFetched, symbol);
                        }
                    }
                }

                // Check and fetch short volume data if not today
                bool hasTodayShortVolume = await _context.PolygonShortVolumeData
                    .AnyAsync(d => d.StockTickerSymbol == symbol.ToUpper() && d.Date.Date == today);
                
                if (hasTodayShortVolume)
                {
                    shortVolumeSkipped = true;
                    _logger.LogInformation("Short volume data for today already exists for {Symbol}, skipping fetch", symbol);
                }
                else
                {
                    _logger.LogInformation("Fetching short volume data for {Symbol}...", symbol);
                    DateTime endDate = DateTime.Now;
                    DateTime startDate = endDate.AddYears(-2);
                    
                    var shortVolumeData = await _polygonService.GetShortVolumeDataAsync(symbol, startDate, endDate);
                    if (shortVolumeData.Any())
                    {
                        var existingDates = await _context.PolygonShortVolumeData
                            .Where(d => d.StockTickerSymbol == symbol.ToUpper())
                            .Select(d => d.Date)
                            .ToListAsync();
                        
                        var newSV = shortVolumeData
                            .Where(d => !existingDates.Contains(d.Date))
                            .ToList();
                        
                        if (newSV.Any())
                        {
                            _context.PolygonShortVolumeData.AddRange(newSV);
                            await _context.SaveChangesAsync();
                            shortVolumeFetched = newSV.Count;
                            _logger.LogInformation("Fetched {Count} new short volume records for {Symbol}", shortVolumeFetched, symbol);
                        }
                    }
                }

                var result = new
                {
                    success = true,
                    symbol = symbol.ToUpper(),
                    prices = new { fetched = pricesFetched, skipped = pricesSkipped, message = pricesSkipped ? "Price data for today already exists" : $"Fetched {pricesFetched} price records" },
                    shortInterest = new { fetched = shortInterestFetched, skipped = shortInterestSkipped, message = shortInterestSkipped ? "Short interest data for today already exists" : $"Fetched {shortInterestFetched} short interest records" },
                    shortVolume = new { fetched = shortVolumeFetched, skipped = shortVolumeSkipped, message = shortVolumeSkipped ? "Short volume data for today already exists" : $"Fetched {shortVolumeFetched} short volume records" }
                };

                _logger.LogInformation("Completed fetching all Polygon data for {Symbol}", symbol);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Polygon data for {Symbol}", symbol);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Fetches and stores 2 years of historical price data for a symbol from Polygon.io
        /// </summary>
        [HttpPost("{symbol}/fetch")]
        public async Task<ActionResult<object>> FetchPolygonPriceData(
            [FromRoute] string symbol,
            [FromQuery] int years = 2)
        {
            try
            {
                _logger.LogInformation("Fetching {Years} years of Polygon price data for {Symbol}...", years, symbol);

                // Ensure ticker exists
                var ticker = await _context.StockTickers.FindAsync(symbol.ToUpper());
                if (ticker == null)
                {
                    ticker = new StockTicker
                    {
                        Symbol = symbol.ToUpper(),
                        Exchange = "NYSE", // Default
                        Name = symbol.ToUpper(),
                        LastUpdated = DateTime.Now
                    };
                    _context.StockTickers.Add(ticker);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Created new ticker {Symbol}", symbol);
                }

                // Fetch data from Polygon.io
                var polygonData = await _polygonService.GetHistoricalDataAsync(symbol, years);

                if (!polygonData.Any())
                {
                    _logger.LogWarning("No Polygon data retrieved for {Symbol}", symbol);
                    return new { success = false, message = "No data retrieved from Polygon", count = 0 };
                }

                // Get existing dates to avoid duplicates
                var existingDates = await _context.PolygonPriceData
                    .Where(d => d.StockTickerSymbol == symbol.ToUpper())
                    .Select(d => d.Date)
                    .ToListAsync();

                // Filter out duplicates
                var newData = polygonData
                    .Where(d => !existingDates.Contains(d.Date))
                    .ToList();

                if (!newData.Any())
                {
                    _logger.LogInformation("All data for {Symbol} already exists in database", symbol);
                    return new
                    {
                        success = true,
                        message = $"All {polygonData.Count} data points already exist",
                        count = 0,
                        totalFromApi = polygonData.Count
                    };
                }

                // Store in database
                _context.PolygonPriceData.AddRange(newData);
                await _context.SaveChangesAsync();

                // Update ticker last updated
                ticker.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully stored {Count} new Polygon data points for {Symbol}", 
                    newData.Count, symbol);

                return new
                {
                    success = true,
                    message = $"Successfully fetched and stored {newData.Count} new data points (of {polygonData.Count} total) for {symbol}",
                    count = newData.Count,
                    totalFromApi = polygonData.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Polygon data for {Symbol}", symbol);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching Polygon data",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets stored Polygon price data for a symbol
        /// </summary>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<IEnumerable<PolygonPriceData>>> GetPolygonData(
            [FromRoute] string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.PolygonPriceData
                    .Where(d => d.StockTickerSymbol == symbol.ToUpper());

                if (startDate.HasValue)
                {
                    query = query.Where(d => d.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(d => d.Date <= endDate.Value);
                }

                var data = await query.OrderBy(d => d.Date).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Polygon data for {Symbol}", symbol);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the latest Polygon price data for a symbol
        /// </summary>
        [HttpGet("{symbol}/latest")]
        public async Task<ActionResult<PolygonPriceData>> GetLatestPolygonData([FromRoute] string symbol)
        {
            try
            {
                var latestData = await _context.PolygonPriceData
                    .Where(d => d.StockTickerSymbol == symbol.ToUpper())
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefaultAsync();

                if (latestData == null)
                {
                    return NotFound(new { message = $"No Polygon data found for {symbol}" });
                }

                return Ok(latestData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest Polygon data for {Symbol}", symbol);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets statistics for stored Polygon data
        /// </summary>
        [HttpGet("{symbol}/stats")]
        public async Task<ActionResult<object>> GetPolygonStats([FromRoute] string symbol)
        {
            try
            {
                var data = await _context.PolygonPriceData
                    .Where(d => d.StockTickerSymbol == symbol.ToUpper())
                    .ToListAsync();

                if (!data.Any())
                {
                    return NotFound(new { message = $"No Polygon data found for {symbol}" });
                }

                var stats = new
                {
                    symbol = symbol.ToUpper(),
                    totalRecords = data.Count,
                    dateRange = new
                    {
                        startDate = data.Min(d => d.Date).ToString("yyyy-MM-dd"),
                        endDate = data.Max(d => d.Date).ToString("yyyy-MM-dd")
                    },
                    priceRange = new
                    {
                        minLow = data.Min(d => d.Low),
                        maxHigh = data.Max(d => d.High),
                        avgClose = data.Average(d => (double)d.Close)
                    },
                    volume = new
                    {
                        total = data.Sum(d => d.Volume),
                        average = data.Average(d => (double)d.Volume)
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Polygon stats for {Symbol}", symbol);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Fetches and stores short interest data for a symbol from Polygon.io
        /// </summary>
        [HttpPost("{symbol}/fetch-short-interest")]
        public async Task<ActionResult<object>> FetchPolygonShortInterest([FromRoute] string symbol)
        {
            try
            {
                _logger.LogInformation("Fetching Polygon short interest data for {Symbol}...", symbol);

                // Ensure ticker exists
                var ticker = await _context.StockTickers.FindAsync(symbol.ToUpper());
                if (ticker == null)
                {
                    ticker = new StockTicker
                    {
                        Symbol = symbol.ToUpper(),
                        Exchange = "NYSE",
                        Name = symbol.ToUpper(),
                        LastUpdated = DateTime.Now
                    };
                    _context.StockTickers.Add(ticker);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Created new ticker {Symbol}", symbol);
                }

                // Fetch data from Polygon.io
                var polygonData = await _polygonService.GetShortInterestDataAsync(symbol);

                if (!polygonData.Any())
                {
                    _logger.LogWarning("No Polygon short interest data retrieved for {Symbol}", symbol);
                    return new { success = false, message = "No short interest data retrieved from Polygon", count = 0 };
                }

                // Get existing dates to avoid duplicates
                var existingDates = await _context.PolygonShortInterestData
                    .Where(d => d.StockTickerSymbol == symbol.ToUpper())
                    .Select(d => d.Date)
                    .ToListAsync();

                // Filter out duplicates
                var newData = polygonData
                    .Where(d => !existingDates.Contains(d.Date))
                    .ToList();

                if (!newData.Any())
                {
                    _logger.LogInformation("All short interest data for {Symbol} already exists in database", symbol);
                    return new
                    {
                        success = true,
                        message = $"All {polygonData.Count} short interest records already exist",
                        count = 0,
                        totalFromApi = polygonData.Count
                    };
                }

                // Store in database
                _context.PolygonShortInterestData.AddRange(newData);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully stored {Count} new Polygon short interest records for {Symbol}", 
                    newData.Count, symbol);

                return new
                {
                    success = true,
                    message = $"Successfully fetched and stored {newData.Count} new short interest records (of {polygonData.Count} total) for {symbol}",
                    count = newData.Count,
                    totalFromApi = polygonData.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Polygon short interest data for {Symbol}", symbol);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching Polygon short interest data",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets stored Polygon short interest data for a symbol
        /// </summary>
        [HttpGet("{symbol}/short-interest")]
        public async Task<ActionResult<IEnumerable<PolygonShortInterestData>>> GetPolygonShortInterest(
            [FromRoute] string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.PolygonShortInterestData
                    .Where(d => d.StockTickerSymbol == symbol.ToUpper());

                if (startDate.HasValue)
                {
                    query = query.Where(d => d.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(d => d.Date <= endDate.Value);
                }

                var data = await query.OrderBy(d => d.Date).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Polygon short interest data for {Symbol}", symbol);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Fetches and stores short volume data for a symbol from Polygon.io
        /// </summary>
        [HttpPost("{symbol}/fetch-short-volume")]
        public async Task<ActionResult<object>> FetchPolygonShortVolume(
            [FromRoute] string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Fetching Polygon short volume data for {Symbol}...", symbol);

                // Set default date range (last 2 years if not specified)
                if (!startDate.HasValue) startDate = DateTime.Now.AddYears(-2);
                if (!endDate.HasValue) endDate = DateTime.Now;

                // Ensure ticker exists
                var ticker = await _context.StockTickers.FindAsync(symbol.ToUpper());
                if (ticker == null)
                {
                    ticker = new StockTicker
                    {
                        Symbol = symbol.ToUpper(),
                        Exchange = "NYSE",
                        Name = symbol.ToUpper(),
                        LastUpdated = DateTime.Now
                    };
                    _context.StockTickers.Add(ticker);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Created new ticker {Symbol}", symbol);
                }

                // Fetch data from Polygon.io
                var polygonData = await _polygonService.GetShortVolumeDataAsync(symbol, startDate.Value, endDate.Value);

                if (!polygonData.Any())
                {
                    _logger.LogWarning("No Polygon short volume data retrieved for {Symbol}", symbol);
                    return new { success = false, message = "No short volume data retrieved from Polygon", count = 0 };
                }

                // Get existing dates to avoid duplicates
                var existingDates = await _context.PolygonShortVolumeData
                    .Where(d => d.StockTickerSymbol == symbol.ToUpper())
                    .Select(d => d.Date)
                    .ToListAsync();

                // Filter out duplicates
                var newData = polygonData
                    .Where(d => !existingDates.Contains(d.Date))
                    .ToList();

                if (!newData.Any())
                {
                    _logger.LogInformation("All short volume data for {Symbol} already exists in database", symbol);
                    return new
                    {
                        success = true,
                        message = $"All {polygonData.Count} short volume records already exist",
                        count = 0,
                        totalFromApi = polygonData.Count
                    };
                }

                // Store in database
                _context.PolygonShortVolumeData.AddRange(newData);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully stored {Count} new Polygon short volume records for {Symbol}", 
                    newData.Count, symbol);

                return new
                {
                    success = true,
                    message = $"Successfully fetched and stored {newData.Count} new short volume records (of {polygonData.Count} total) for {symbol}",
                    count = newData.Count,
                    totalFromApi = polygonData.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Polygon short volume data for {Symbol}", symbol);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching Polygon short volume data",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets stored Polygon short volume data for a symbol
        /// </summary>
        [HttpGet("{symbol}/short-volume")]
        public async Task<ActionResult<IEnumerable<PolygonShortVolumeData>>> GetPolygonShortVolume(
            [FromRoute] string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.PolygonShortVolumeData
                    .Where(d => d.StockTickerSymbol == symbol.ToUpper());

                if (startDate.HasValue)
                {
                    query = query.Where(d => d.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(d => d.Date <= endDate.Value);
                }

                var data = await query.OrderBy(d => d.Date).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Polygon short volume data for {Symbol}", symbol);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
