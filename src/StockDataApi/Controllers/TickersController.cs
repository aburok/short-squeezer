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
    public class TickersController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TickersController> _logger;
        private readonly ITickerService _tickerService;

        public TickersController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<TickersController> logger,
            ITickerService tickerService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _tickerService = tickerService;
        }

        /// <summary>
        /// Gets all available stock tickers
        /// </summary>
        /// <returns>A list of stock tickers</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TickerDto>>> GetTickers()
        {
            try
            {
                // Create cache key
                string cacheKey = "AllTickers";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out List<TickerDto> cachedData))
                {
                    _logger.LogInformation("Returning cached tickers");
                    return cachedData;
                }

                // Get all tickers
                var tickers = await _context.StockTickers
                    .Select(t => new TickerDto
                    {
                        Symbol = t.Symbol,
                        Exchange = t.Exchange,
                        Name = t.Name,
                        LastUpdated = t.LastUpdated
                    })
                    .ToListAsync();

                // Cache the result for 1 hour
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, tickers, cacheOptions);

                _logger.LogInformation("Retrieved {Count} tickers", tickers.Count);
                return tickers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickers");
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets a specific ticker by symbol
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>The ticker information</returns>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<TickerDto>> GetTicker(string symbol)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();

                // Create cache key
                string cacheKey = $"Ticker_{symbol}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out TickerDto cachedData))
                {
                    _logger.LogInformation("Returning cached ticker for {Symbol}", symbol);
                    return cachedData;
                }

                // Find the ticker
                var ticker = await _context.StockTickers
                    .Where(t => t.Symbol == symbol)
                    .Select(t => new TickerDto
                    {
                        Symbol = t.Symbol,
                        Exchange = t.Exchange,
                        Name = t.Name,
                        LastUpdated = t.LastUpdated
                    })
                    .FirstOrDefaultAsync();

                if (ticker == null)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                // Cache the result for 1 hour
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, ticker, cacheOptions);

                _logger.LogInformation("Retrieved ticker {Symbol}", symbol);
                return ticker;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticker {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Adds a new ticker to the system
        /// </summary>
        /// <param name="tickerDto">The ticker information</param>
        /// <returns>The created ticker</returns>
        [HttpPost]
        public async Task<ActionResult<TickerDto>> AddTicker(TickerDto tickerDto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(tickerDto.Symbol) || string.IsNullOrWhiteSpace(tickerDto.Exchange))
                {
                    return BadRequest("Symbol and Exchange are required");
                }

                // Normalize the symbol
                tickerDto.Symbol = tickerDto.Symbol.ToUpper().Trim();

                // Check if ticker already exists
                var existingTicker = await _context.StockTickers
                    .FirstOrDefaultAsync(t => t.Symbol == tickerDto.Symbol);

                if (existingTicker != null)
                {
                    return Conflict($"Ticker {tickerDto.Symbol} already exists");
                }

                // Create new ticker
                var newTicker = new StockTicker
                {
                    Symbol = tickerDto.Symbol,
                    Exchange = tickerDto.Exchange.ToLower().Trim(),
                    Name = tickerDto.Name,
                    LastUpdated = DateTime.UtcNow
                };

                // Add to database
                _context.StockTickers.Add(newTicker);
                await _context.SaveChangesAsync();

                // Clear cache
                _cache.Remove("AllTickers");

                // Return created ticker
                var createdTickerDto = new TickerDto
                {
                    Symbol = newTicker.Symbol,
                    Exchange = newTicker.Exchange,
                    Name = newTicker.Name,
                    LastUpdated = newTicker.LastUpdated
                };

                _logger.LogInformation("Added new ticker {Symbol}", tickerDto.Symbol);
                return CreatedAtAction(nameof(GetTicker), new { symbol = createdTickerDto.Symbol }, createdTickerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ticker {Symbol}", tickerDto.Symbol);
                return StatusCode(500, "An error occurred while adding the ticker");
            }
        }

        /// <summary>
        /// Deletes a ticker from the system
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>No content</returns>
        [HttpDelete("{symbol}")]
        public async Task<IActionResult> DeleteTicker(string symbol)
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
                    _logger.LogWarning("Ticker {Symbol} not found for deletion", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                // Delete the ticker
                _context.StockTickers.Remove(ticker);
                await _context.SaveChangesAsync();

                // Clear cache
                _cache.Remove("AllTickers");
                _cache.Remove($"Ticker_{symbol}");

                _logger.LogInformation("Deleted ticker {Symbol}", symbol);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticker {Symbol}", symbol);
                return StatusCode(500, "An error occurred while deleting the ticker");
            }
        }

        /// <summary>
        /// Refreshes data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="exchange">The exchange (e.g., nasdaq)</param>
        /// <returns>Success or failure message</returns>
        [HttpPost("refresh/{symbol}")]
        public async Task<IActionResult> RefreshTickerData(
            string symbol,
            [FromQuery] string exchange = "nasdaq",
            [FromQuery] string startDate = null,
            [FromQuery] string endDate = null)
        {
            try
            {
                // Normalize inputs
                symbol = symbol.ToUpper().Trim();
                exchange = exchange.ToLower().Trim();
                
                _logger.LogInformation("Refreshing data for {Symbol} on {Exchange}", symbol, exchange);
                
                // Parse date range if provided
                DateTime? parsedStartDate = null;
                DateTime? parsedEndDate = null;
                
                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime start))
                {
                    parsedStartDate = start;
                }
                
                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime end))
                {
                    parsedEndDate = end;
                }
                
                if (parsedStartDate.HasValue && parsedEndDate.HasValue)
                {
                    _logger.LogInformation("Using date range: {StartDate} to {EndDate}",
                        parsedStartDate.Value.ToString("yyyy-MM-dd"),
                        parsedEndDate.Value.ToString("yyyy-MM-dd"));
                }
                
                bool success = await _tickerService.RefreshTickerDataAsync(symbol, exchange, parsedStartDate, parsedEndDate);
                
                if (success)
                {
                    // Clear cache
                    _cache.Remove($"Ticker_{symbol}");
                    _cache.Remove("AllTickers");
                    
                    _logger.LogInformation("Successfully refreshed data for {Symbol}", symbol);
                    return Ok($"Successfully refreshed data for {symbol}");
                }
                else
                {
                    _logger.LogWarning("Failed to refresh data for {Symbol}", symbol);
                    return BadRequest($"Failed to refresh data for {symbol}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while refreshing the data");
            }
        }

        /// <summary>
        /// Refreshes all tickers from exchanges
        /// </summary>
        /// <returns>Success or failure message</returns>
        [HttpPost("refresh-all")]
        public async Task<IActionResult> RefreshAllTickers()
        {
            try
            {
                _logger.LogInformation("Starting refresh of all tickers from exchanges");
                
                bool success = await _tickerService.RefreshAllTickersAsync();
                
                if (success)
                {
                    // Clear cache
                    _cache.Remove("AllTickers");
                    
                    _logger.LogInformation("Successfully refreshed all tickers");
                    return Ok("Successfully refreshed all tickers");
                }
                else
                {
                    _logger.LogWarning("Failed to refresh all tickers");
                    return BadRequest("Failed to refresh all tickers");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing all tickers");
                return StatusCode(500, "An error occurred while refreshing all tickers");
            }
        }

        /// <summary>
        /// Fetches latest data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>Success or failure message with status</returns>
        [HttpPost("fetch/{symbol}")]
        public async Task<ActionResult<FetchResultDto>> FetchTickerData(string symbol)
        {
            try
            {
                // Normalize the symbol
                symbol = symbol.ToUpper().Trim();
                
                _logger.LogInformation("Fetching latest data for {Symbol}", symbol);
                
                // Find the ticker to get its exchange
                var ticker = await _context.StockTickers
                    .FirstOrDefaultAsync(t => t.Symbol == symbol);
                
                if (ticker == null)
                {
                    _logger.LogWarning("Ticker {Symbol} not found for data fetching", symbol);
                    return NotFound(new FetchResultDto
                    {
                        Symbol = symbol,
                        Success = false,
                        Message = $"Ticker {symbol} not found"
                    });
                }
                
                // Use the ticker service to refresh data
                bool success = await _tickerService.RefreshTickerDataAsync(symbol, ticker.Exchange);
                
                if (success)
                {
                    // Clear cache
                    _cache.Remove($"Ticker_{symbol}");
                    
                    _logger.LogInformation("Successfully fetched latest data for {Symbol}", symbol);
                    return Ok(new FetchResultDto
                    {
                        Symbol = symbol,
                        Success = true,
                        Message = $"Successfully fetched latest data for {symbol}",
                        LastUpdated = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to fetch latest data for {Symbol}", symbol);
                    return BadRequest(new FetchResultDto
                    {
                        Symbol = symbol,
                        Success = false,
                        Message = $"Failed to fetch latest data for {symbol}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest data for {Symbol}", symbol);
                return StatusCode(500, new FetchResultDto
                {
                    Symbol = symbol,
                    Success = false,
                    Message = "An error occurred while fetching the latest data"
                });
            }
        }
        
        /// <summary>
        /// Searches for tickers matching a query string
        /// </summary>
        /// <param name="query">The search query</param>
        /// <returns>List of matching tickers</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TickerDto>>> SearchTickers([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest("Search query must be at least 2 characters");
                }
                
                query = query.ToUpper().Trim();
                
                _logger.LogInformation("Searching for tickers matching '{Query}'", query);
                
                var results = await _context.StockTickers
                    .Where(t => t.Symbol.Contains(query) || (t.Name != null && t.Name.Contains(query)))
                    .OrderBy(t => t.Symbol)
                    .Take(20)
                    .Select(t => new TickerDto
                    {
                        Symbol = t.Symbol,
                        Exchange = t.Exchange,
                        Name = t.Name,
                        LastUpdated = t.LastUpdated
                    })
                    .ToListAsync();
                
                _logger.LogInformation("Found {Count} tickers matching '{Query}'", results.Count, query);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for tickers with query '{Query}'", query);
                return StatusCode(500, "An error occurred while searching for tickers");
            }
        }
    }

    /// <summary>
    /// Data transfer object for stock ticker
    /// </summary>
    public class TickerDto
    {
        public string Symbol { get; set; }
        public string Exchange { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
    }
    
    /// <summary>
    /// Data transfer object for fetch operation result
    /// </summary>
    public class FetchResultDto
    {
        public string Symbol { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}

// Made with Bob
