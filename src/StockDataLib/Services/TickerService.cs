using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockDataLib.Data;
using StockDataLib.Models;

namespace StockDataLib.Services
{
    public interface ITickerService
    {
        Task<List<StockTicker>> GetTickersFromExchangeAsync(string exchange);
        Task<bool> RefreshTickerDataAsync(string symbol, string exchange);
        Task<bool> RefreshAllTickersAsync();
    }

    public class TickerService : ITickerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TickerService> _logger;
        private readonly StockDataContext _context;
        private readonly IChartExchangeService _chartExchangeService;

        public TickerService(
            IHttpClientFactory httpClientFactory, 
            ILogger<TickerService> logger,
            StockDataContext context,
            IChartExchangeService chartExchangeService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _context = context;
            _chartExchangeService = chartExchangeService;
        }

        /// <summary>
        /// Gets a list of tickers from a specific exchange
        /// </summary>
        /// <param name="exchange">The exchange (e.g., nasdaq, nyse)</param>
        /// <returns>A list of stock tickers</returns>
        public async Task<List<StockTicker>> GetTickersFromExchangeAsync(string exchange)
        {
            try
            {
                exchange = exchange.ToLower().Trim();
                string url = "";
                
                switch (exchange)
                {
                    case "nasdaq":
                        url = "https://www.nasdaq.com/market-activity/stocks/screener";
                        break;
                    case "nyse":
                        url = "https://www.nyse.com/listings/stock-screener";
                        break;
                    default:
                        _logger.LogWarning("Unsupported exchange: {Exchange}", exchange);
                        return new List<StockTicker>();
                }

                _logger.LogInformation("Fetching tickers from {Exchange} at {Url}", exchange, url);

                using var httpClient = _httpClientFactory.CreateClient("ExchangeData");
                
                // Add headers to mimic a browser request
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                
                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch tickers: {StatusCode} {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return new List<StockTicker>();
                }

                string html = await response.Content.ReadAsStringAsync();
                return ParseTickersFromHtml(html, exchange);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tickers from {Exchange}", exchange);
                return new List<StockTicker>();
            }
        }

        /// <summary>
        /// Parses tickers from HTML content
        /// </summary>
        /// <param name="html">The HTML content</param>
        /// <param name="exchange">The exchange name</param>
        /// <returns>A list of stock tickers</returns>
        private List<StockTicker> ParseTickersFromHtml(string html, string exchange)
        {
            try
            {
                var tickers = new List<StockTicker>();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Different parsing logic based on exchange
                if (exchange == "nasdaq")
                {
                    // Extract ticker data from NASDAQ page
                    // This is a simplified example - actual implementation would need to handle NASDAQ's specific HTML structure
                    var rows = doc.DocumentNode.SelectNodes("//table[contains(@class, 'nasdaq-screener__table')]/tbody/tr");
                    
                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            var symbolNode = row.SelectSingleNode(".//td[1]/a");
                            var nameNode = row.SelectSingleNode(".//td[2]");
                            
                            if (symbolNode != null && nameNode != null)
                            {
                                string symbol = symbolNode.InnerText.Trim();
                                string name = nameNode.InnerText.Trim();
                                
                                tickers.Add(new StockTicker
                                {
                                    Symbol = symbol,
                                    Name = name,
                                    Exchange = exchange,
                                    LastUpdated = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
                else if (exchange == "nyse")
                {
                    // Extract ticker data from NYSE page
                    // This is a simplified example - actual implementation would need to handle NYSE's specific HTML structure
                    var rows = doc.DocumentNode.SelectNodes("//table[contains(@class, 'nyse-screener')]/tbody/tr");
                    
                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            var symbolNode = row.SelectSingleNode(".//td[1]");
                            var nameNode = row.SelectSingleNode(".//td[2]");
                            
                            if (symbolNode != null && nameNode != null)
                            {
                                string symbol = symbolNode.InnerText.Trim();
                                string name = nameNode.InnerText.Trim();
                                
                                tickers.Add(new StockTicker
                                {
                                    Symbol = symbol,
                                    Name = name,
                                    Exchange = exchange,
                                    LastUpdated = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("Successfully parsed {Count} tickers from {Exchange}", tickers.Count, exchange);
                return tickers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing tickers from HTML for {Exchange}", exchange);
                return new List<StockTicker>();
            }
        }

        /// <summary>
        /// Refreshes data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="exchange">The exchange (e.g., nasdaq)</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RefreshTickerDataAsync(string symbol, string exchange)
        {
            try
            {
                // Normalize inputs
                symbol = symbol.ToUpper().Trim();
                exchange = exchange.ToLower().Trim();
                
                _logger.LogInformation("Refreshing data for {Symbol} on {Exchange}", symbol, exchange);
                
                // Find or create the ticker
                var ticker = await _context.StockTickers
                    .FirstOrDefaultAsync(t => t.Symbol == symbol);
                
                if (ticker == null)
                {
                    _logger.LogInformation("Ticker {Symbol} not found, creating new entry", symbol);
                    ticker = new StockTicker
                    {
                        Symbol = symbol,
                        Exchange = exchange,
                        Name = symbol, // Default to symbol as name
                        LastUpdated = DateTime.UtcNow
                    };
                    
                    _context.StockTickers.Add(ticker);
                    await _context.SaveChangesAsync();
                }
                
                // Fetch short volume data
                var shortVolumeData = await _chartExchangeService.GetShortVolumeDataAsync(symbol, exchange);
                if (shortVolumeData.Any())
                {
                    // Get existing data dates to avoid duplicates
                    var existingDates = await _context.ShortVolumeData
                        .Where(d => d.StockTickerId == ticker.Id)
                        .Select(d => d.Date.Date)
                        .ToListAsync();
                    
                    int addedCount = 0;
                    
                    // Save new data to database
                    foreach (var item in shortVolumeData)
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
                        _logger.LogInformation("Added {Count} new short volume data points for {Symbol}", addedCount, symbol);
                    }
                }
                
                // Fetch short interest data
                var shortInterestData = await _chartExchangeService.GetShortInterestDataAsync(symbol);
                if (shortInterestData.Any())
                {
                    // Get existing data dates to avoid duplicates
                    var existingDates = await _context.ShortInterestData
                        .Where(d => d.StockTickerId == ticker.Id)
                        .Select(d => d.Date.Date)
                        .ToListAsync();
                    
                    int addedCount = 0;
                    
                    // Save new data to database
                    foreach (var item in shortInterestData)
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
                        _logger.LogInformation("Added {Count} new short interest data points for {Symbol}", addedCount, symbol);
                    }
                }
                
                // Fetch borrow fee data
                var borrowFeeData = await _chartExchangeService.GetBorrowFeeDataAsync(symbol, exchange);
                if (borrowFeeData.Any())
                {
                    // Get existing data dates to avoid duplicates
                    var existingDates = await _context.BorrowFeeData
                        .Where(d => d.StockTickerId == ticker.Id)
                        .Select(d => d.Date.Date)
                        .ToListAsync();
                    
                    int addedCount = 0;
                    
                    // Save new data to database
                    foreach (var item in borrowFeeData)
                    {
                        if (!existingDates.Contains(item.Date.Date))
                        {
                            item.StockTickerId = ticker.Id;
                            _context.BorrowFeeData.Add(item);
                            addedCount++;
                        }
                    }
                    
                    if (addedCount > 0)
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Added {Count} new borrow fee data points for {Symbol}", addedCount, symbol);
                    }
                }
                
                // Update last updated timestamp
                ticker.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully refreshed data for {Symbol}", symbol);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing data for {Symbol} on {Exchange}", symbol, exchange);
                return false;
            }
        }

        /// <summary>
        /// Refreshes all tickers in the database
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RefreshAllTickersAsync()
        {
            try
            {
                _logger.LogInformation("Starting refresh of all tickers from exchanges");
                
                // Get tickers from exchanges
                var nasdaqTickers = await GetTickersFromExchangeAsync("nasdaq");
                var nyseTickers = await GetTickersFromExchangeAsync("nyse");
                
                // Combine all tickers
                var allTickers = nasdaqTickers.Concat(nyseTickers).ToList();
                
                if (allTickers.Count == 0)
                {
                    _logger.LogWarning("No tickers found from exchanges");
                    return false;
                }
                
                _logger.LogInformation("Found {Count} tickers from exchanges", allTickers.Count);
                
                // Get existing tickers
                var existingTickers = await _context.StockTickers.ToListAsync();
                var existingSymbols = existingTickers.Select(t => t.Symbol).ToHashSet();
                
                // Add new tickers
                int addedCount = 0;
                foreach (var ticker in allTickers)
                {
                    if (!existingSymbols.Contains(ticker.Symbol))
                    {
                        _context.StockTickers.Add(ticker);
                        addedCount++;
                    }
                }
                
                if (addedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} new tickers to database", addedCount);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing all tickers");
                return false;
            }
        }
    }
}

// Made with Bob
