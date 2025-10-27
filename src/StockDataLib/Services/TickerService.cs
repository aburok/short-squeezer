using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockDataLib.Data;
using StockDataLib.Models;

namespace StockDataLib.Services
{
    public interface ITickerService
    {
        Task<List<StockTicker>> GetTickersFromExchangeAsync(string exchange);
        Task<bool> RefreshTickerDataAsync(string symbol, string exchange, DateTime? startDate = null, DateTime? endDate = null);
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
                    case "nyse":
                    case "amex":
                    case "all":
                        // Try SEC first, then fallback to alternative sources
                        return await TrySecEndpoint(exchange);
                    default:
                        _logger.LogWarning("Unsupported exchange: {Exchange}", exchange);
                        return new List<StockTicker>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tickers from {Exchange}", exchange);
                return new List<StockTicker>();
            }
        }

        /// <summary>
        /// Tries to fetch tickers from SEC endpoint with fallback options
        /// </summary>
        /// <param name="exchange">The exchange filter</param>
        /// <returns>A list of stock tickers</returns>
        private async Task<List<StockTicker>> TrySecEndpoint(string exchange)
        {
            try
            {
                _logger.LogInformation("Attempting to fetch tickers from SEC endpoint for {Exchange}", exchange);
                
                // Add a small delay to be respectful to SEC servers
                await Task.Delay(1000);

                using var httpClient = _httpClientFactory.CreateClient("ExchangeData");
                
                // Add headers required by SEC.gov for automated access
                httpClient.DefaultRequestHeaders.Add("User-Agent", "StockDataApi/1.0 (https://github.com/your-repo/stock-data-api; contact@yourdomain.com)");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "cross-site");
                
                var response = await httpClient.GetAsync("https://www.sec.gov/files/company_tickers.json");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("SEC endpoint failed: {StatusCode} {ReasonPhrase}. Content: {Content}",
                        response.StatusCode, response.ReasonPhrase, errorContent);
                    
                    // Check for specific SEC access issues
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && 
                        errorContent.Contains("Privacy and Security Policy"))
                    {
                        _logger.LogError("SEC access denied. Please ensure your application complies with SEC.gov's Privacy and Security Policy. " +
                            "Consider adding proper User-Agent header and rate limiting.");
                    }
                    
                    // Try fallback to NASDAQ endpoint
                    return await TryNasdaqFallback(exchange);
                }

                string content = await response.Content.ReadAsStringAsync();
                
                // Use SEC JSON parser for all exchanges
                return ParseSecTickers(content, exchange);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tickers from SEC endpoint for {Exchange}", exchange);
                
                // Try fallback to NASDAQ endpoint
                return await TryNasdaqFallback(exchange);
            }
        }

        /// <summary>
        /// Fallback method to fetch NASDAQ tickers if SEC endpoint fails
        /// </summary>
        /// <param name="exchange">The exchange filter</param>
        /// <returns>A list of stock tickers</returns>
        private async Task<List<StockTicker>> TryNasdaqFallback(string exchange)
        {
            try
            {
                _logger.LogInformation("Trying NASDAQ fallback for {Exchange}", exchange);
                
                using var httpClient = _httpClientFactory.CreateClient("ExchangeData");
                
                // Add headers for NASDAQ endpoint
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/plain, */*");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                
                var response = await httpClient.GetAsync("https://www.nasdaqtrader.com/dynamic/symdir/nasdaqlisted.txt");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("NASDAQ fallback also failed: {StatusCode} {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return new List<StockTicker>();
                }

                string content = await response.Content.ReadAsStringAsync();
                
                // Parse NASDAQ format
                return ParseNasdaqTickers(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tickers from NASDAQ fallback for {Exchange}", exchange);
                return new List<StockTicker>();
            }
        }

        /// <summary>
        /// Parses tickers from NASDAQ pipe-delimited format (fallback method)
        /// </summary>
        /// <param name="content">The NASDAQ data content</param>
        /// <returns>A list of stock tickers</returns>
        private List<StockTicker> ParseNasdaqTickers(string content)
        {
            try
            {
                var tickers = new List<StockTicker>();
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                // Skip the header line and process each line
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    
                    var parts = line.Split('|');
                    
                    // NASDAQ format: Symbol|Security Name|Market Category|Test Issue|Financial Status|Round Lot Size|ETF|NextShares
                    if (parts.Length >= 2)
                    {
                        string symbol = parts[0].Trim();
                        string name = parts[1].Trim();
                        string testIssue = parts.Length > 3 ? parts[3].Trim() : "";
                        
                        // Skip test issues and invalid symbols
                        if (testIssue == "Y" || string.IsNullOrEmpty(symbol) || symbol.Length > 5)
                            continue;
                        
                        tickers.Add(new StockTicker
                        {
                            Symbol = symbol,
                            Name = name,
                            Exchange = "nasdaq",
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                }
                
                _logger.LogInformation("Successfully parsed {Count} NASDAQ tickers (fallback)", tickers.Count);
                return tickers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing NASDAQ tickers (fallback)");
                return new List<StockTicker>();
            }
        }

        /// <summary>
        /// Parses tickers from SEC JSON format
        /// </summary>
        /// <param name="content">The SEC JSON content</param>
        /// <param name="exchange">The exchange filter (nasdaq, nyse, amex, all)</param>
        /// <returns>A list of stock tickers</returns>
        private List<StockTicker> ParseSecTickers(string content, string exchange)
        {
            try
            {
                var tickers = new List<StockTicker>();
                
                // Parse JSON content
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                
                if (jsonData == null)
                {
                    _logger.LogWarning("Failed to parse SEC JSON data");
                    return tickers;
                }
                
                // SEC JSON format: {"0": {"cik_str": 320193, "ticker": "AAPL", "title": "Apple Inc."}, ...}
                foreach (var kvp in jsonData)
                {
                    if (kvp.Value is JsonElement element)
                    {
                        try
                        {
                            var tickerData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
                            
                            if (tickerData != null && 
                                tickerData.ContainsKey("ticker") && 
                                tickerData.ContainsKey("title"))
                            {
                                string symbol = tickerData["ticker"]?.ToString()?.Trim();
                                string name = tickerData["title"]?.ToString()?.Trim();
                                
                                if (!string.IsNullOrEmpty(symbol) && !string.IsNullOrEmpty(name))
                                {
                                    // Determine exchange based on symbol patterns or use "all" for comprehensive list
                                    string tickerExchange = exchange == "all" ? "all" : exchange;
                                    
                                    tickers.Add(new StockTicker
                                    {
                                        Symbol = symbol,
                                        Name = name,
                                        Exchange = tickerExchange,
                                        LastUpdated = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error parsing individual ticker entry: {Key}", kvp.Key);
                        }
                    }
                }
                
                _logger.LogInformation("Successfully parsed {Count} SEC tickers for {Exchange}", tickers.Count, exchange);
                return tickers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SEC tickers");
                return new List<StockTicker>();
            }
        }

        /// <summary>
        /// Refreshes data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="exchange">The exchange (e.g., nasdaq)</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RefreshTickerDataAsync(string symbol, string exchange, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Normalize inputs
                symbol = symbol.ToUpper().Trim();
                exchange = exchange.ToLower().Trim();
                
                _logger.LogInformation("Refreshing data for {Symbol} on {Exchange}", symbol, exchange);
                
                // Log date range if provided
                if (startDate.HasValue && endDate.HasValue)
                {
                    _logger.LogInformation("Using date range: {StartDate} to {EndDate}",
                        startDate.Value.ToString("yyyy-MM-dd"),
                        endDate.Value.ToString("yyyy-MM-dd"));
                }
                
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
                var shortVolumeData = await _chartExchangeService.GetShortVolumeDataAsync(symbol, exchange, startDate, endDate);
                if (shortVolumeData.Any())
                {
                    // Get existing data dates to avoid duplicates
                    var existingDates = await _context.ShortVolumeData
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync();
                    
                    int addedCount = 0;
                    
                    // Save new data to database
                    foreach (var item in shortVolumeData)
                    {
                        if (!existingDates.Contains(item.Date.Date))
                        {
                            item.StockTickerSymbol = symbol;
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
                var shortInterestData = await _chartExchangeService.GetShortInterestDataAsync(symbol, startDate, endDate);
                if (shortInterestData.Any())
                {
                    // Get existing data dates to avoid duplicates
                    var existingDates = await _context.ShortInterestData
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync();
                    
                    int addedCount = 0;
                    
                    // Save new data to database
                    foreach (var item in shortInterestData)
                    {
                        if (!existingDates.Contains(item.Date.Date))
                        {
                            item.StockTickerSymbol = symbol;
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
                var borrowFeeData = await _chartExchangeService.GetBorrowFeeDataAsync(symbol, exchange, startDate, endDate);
                if (borrowFeeData.Any())
                {
                    // Get existing data dates to avoid duplicates
                    var existingDates = await _context.BorrowFeeData
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync();
                    
                    int addedCount = 0;
                    
                    // Save new data to database
                    foreach (var item in borrowFeeData)
                    {
                        if (!existingDates.Contains(item.Date.Date))
                        {
                            item.StockTickerSymbol = symbol;
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
                
                // Get tickers from SEC (covers all exchanges)
                var allTickers = await GetTickersFromExchangeAsync("all");
                
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
