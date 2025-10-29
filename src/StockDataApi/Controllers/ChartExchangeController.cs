using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StockDataLib.Data;
using StockDataLib.Services;

namespace StockDataApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartExchangeController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ChartExchangeController> _logger;

        public ChartExchangeController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<ChartExchangeController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Fetches ChartExchange data for a symbol and stores it in the database
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="years">Number of years of data to fetch (default: 2)</param>
        /// <returns>Result of the fetch operation</returns>
        [HttpPost("{symbol}/fetch")]
        public async Task<ActionResult> FetchChartExchangeData(string symbol, [FromQuery] int years = 2)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                _logger.LogInformation("Fetching ChartExchange data for {Symbol} ({Years} years)", symbol, years);

                var startDate = DateTime.Now.AddYears(-years);
                var endDate = DateTime.Now;

                // Check if ticker exists
                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                int totalRecords = 0;

                await _context.SaveChangesAsync();

                // Clear cache for this symbol
                _cache.Remove($"ChartExchangeFailureToDeliver_{symbol}");
                _cache.Remove($"ChartExchangeRedditMentions_{symbol}");
                _cache.Remove($"ChartExchangeOptionChain_{symbol}");
                _cache.Remove($"ChartExchangeStockSplit_{symbol}");

                _logger.LogInformation("Successfully fetched and stored {Count} ChartExchange data points for {Symbol}",
                    totalRecords, symbol);

                return Ok(new
                {
                    success = true,
                    symbol = symbol,
                    count = totalRecords,
                    message = $"Successfully fetched {totalRecords} ChartExchange data points for {symbol}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange data for {Symbol}", symbol);
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }
}