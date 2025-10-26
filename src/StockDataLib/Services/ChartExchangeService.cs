using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using StockDataLib.Models;

namespace StockDataLib.Services
{
    public interface IChartExchangeService
    {
        Task<List<BorrowFeeData>> GetBorrowFeeDataAsync(string symbol, string exchange);
        Task<string> GetHtmlContentAsync(string url);
        Task<List<ShortInterestData>> GetShortInterestDataAsync(string symbol);
        Task<List<ShortVolumeData>> GetShortVolumeDataAsync(string symbol, string exchange);
    }

    public class ChartExchangeService : IChartExchangeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChartExchangeService> _logger;

        public ChartExchangeService(IHttpClientFactory httpClientFactory, ILogger<ChartExchangeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets the borrow fee data for a specific stock from Chart Exchange
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="exchange">The exchange (e.g., nasdaq)</param>
        /// <returns>A list of borrow fee data points</returns>
        public async Task<List<BorrowFeeData>> GetBorrowFeeDataAsync(string symbol, string exchange)
        {
            try
            {
                string url = $"https://chartexchange.com/symbol/{exchange}-{symbol}/borrow-fee/";
                _logger.LogInformation("Fetching borrow fee data from: {Url}", url);

                string html = await GetHtmlContentAsync(url);
                
                if (string.IsNullOrEmpty(html))
                {
                    _logger.LogWarning("Failed to retrieve HTML content from {Url}", url);
                    return new List<BorrowFeeData>();
                }

                return ExtractBorrowFeeDataFromHtml(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching borrow fee data for {Symbol} on {Exchange}", symbol, exchange);
                return new List<BorrowFeeData>();
            }
        }

        /// <summary>
        /// Gets the HTML content from a URL
        /// </summary>
        /// <param name="url">The URL to fetch</param>
        /// <returns>The HTML content as a string</returns>
        public async Task<string> GetHtmlContentAsync(string url)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient("ChartExchange");
                
                // Add headers to mimic a browser request
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://chartexchange.com/");
                httpClient.DefaultRequestHeaders.Add("DNT", "1");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");

                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch data: {StatusCode} {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return string.Empty;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching HTML content from {Url}", url);
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts borrow fee data from HTML content
        /// </summary>
        /// <param name="html">The HTML content</param>
        /// <returns>A list of borrow fee data points</returns>
        private List<BorrowFeeData> ExtractBorrowFeeDataFromHtml(string html)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Find script elements that might contain the chart data
                var scriptNodes = doc.DocumentNode.SelectNodes("//script");
                if (scriptNodes == null)
                {
                    _logger.LogWarning("No script elements found in HTML");
                    return new List<BorrowFeeData>();
                }

                // Look for the script that contains the datasets variable
                string scriptContent = null;
                foreach (var scriptNode in scriptNodes)
                {
                    var content = scriptNode.InnerText;
                    if (content.Contains("datasets") && content.Contains("dat") && content.Contains("bars"))
                    {
                        scriptContent = content;
                        break;
                    }
                }

                if (scriptContent == null)
                {
                    _logger.LogWarning("Could not find script element with chart data");
                    return new List<BorrowFeeData>();
                }

                // Extract the datasets JSON using regex
                var regex = new Regex(@"(?:var\s+)?datasets\s*=\s*(\{[\s\S]*?\}\s*);");
                var match = regex.Match(scriptContent);

                if (!match.Success || match.Groups.Count < 2)
                {
                    _logger.LogWarning("Could not find datasets JSON in script");
                    return new List<BorrowFeeData>();
                }

                // Parse the JSON data
                string jsonStr = match.Groups[1].Value;
                
                // Use a dynamic approach to parse the JSON
                using JsonDocument doc2 = JsonDocument.Parse(jsonStr);
                JsonElement root = doc2.RootElement;

                // Check if the expected structure exists
                if (!root.TryGetProperty("dat", out JsonElement datElement) || 
                    !datElement.TryGetProperty("bars", out JsonElement barsElement) ||
                    barsElement.ValueKind != JsonValueKind.Array)
                {
                    _logger.LogWarning("Invalid data structure in datasets JSON");
                    return new List<BorrowFeeData>();
                }

                // Extract the borrow fee data
                var borrowFeeData = new List<BorrowFeeData>();
                foreach (JsonElement item in barsElement.EnumerateArray())
                {
                    // Extract time/date
                    DateTime date;
                    if (item.TryGetProperty("time", out JsonElement timeElement))
                    {
                        if (timeElement.ValueKind == JsonValueKind.Number)
                        {
                            // Handle Unix timestamp in milliseconds
                            long timestamp = timeElement.GetInt64();
                            date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                        }
                        else if (timeElement.ValueKind == JsonValueKind.String)
                        {
                            // Handle date string
                            if (!DateTime.TryParse(timeElement.GetString(), out date))
                            {
                                continue; // Skip if date parsing fails
                            }
                        }
                        else
                        {
                            continue; // Skip if time is not in expected format
                        }
                    }
                    else if (item.TryGetProperty("date", out JsonElement dateElement) && 
                             dateElement.ValueKind == JsonValueKind.String)
                    {
                        // Fallback to date property
                        if (!DateTime.TryParse(dateElement.GetString(), out date))
                        {
                            continue; // Skip if date parsing fails
                        }
                    }
                    else
                    {
                        continue; // Skip if no valid date/time property
                    }

                    // Extract fee value
                    decimal fee = 0;
                    if (item.TryGetProperty("close", out JsonElement closeElement) && 
                        closeElement.ValueKind == JsonValueKind.Number)
                    {
                        fee = closeElement.GetDecimal();
                    }
                    else if (item.TryGetProperty("value", out JsonElement valueElement) && 
                             valueElement.ValueKind == JsonValueKind.Number)
                    {
                        fee = valueElement.GetDecimal();
                    }
                    else if (item.TryGetProperty("fee", out JsonElement feeElement) && 
                             feeElement.ValueKind == JsonValueKind.Number)
                    {
                        fee = feeElement.GetDecimal();
                    }
                    else if (item.TryGetProperty("y", out JsonElement yElement) && 
                             yElement.ValueKind == JsonValueKind.Number)
                    {
                        fee = yElement.GetDecimal();
                    }

                    // Create data point
                    borrowFeeData.Add(new BorrowFeeData
                    {
                        Date = date,
                        Fee = fee
                    });
                }

                _logger.LogInformation("Successfully extracted {Count} borrow fee data points", borrowFeeData.Count);
                
                // Sort by date ascending
                return borrowFeeData.OrderBy(d => d.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting borrow fee data from HTML");
                return new List<BorrowFeeData>();
            }
        }

        /// <summary>
        /// Gets the short interest data for a specific stock from Chart Exchange
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., SPY)</param>
        /// <returns>A list of short interest data points</returns>
        public async Task<List<ShortInterestData>> GetShortInterestDataAsync(string symbol)
        {
            try
            {
                string url = $"https://chartexchange.com/download/?r=nyse-{symbol.ToLower()}.short_interest&k=symboldata";
                _logger.LogInformation("Fetching short interest data from: {Url}", url);

                using var httpClient = _httpClientFactory.CreateClient("ChartExchange");
                
                // Add headers to mimic a browser request
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/csv,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://chartexchange.com/");
                
                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch short interest data: {StatusCode} {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return new List<ShortInterestData>();
                }

                string csvContent = await response.Content.ReadAsStringAsync();
                return ParseShortInterestCsv(csvContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching short interest data for {Symbol}", symbol);
                return new List<ShortInterestData>();
            }
        }

        /// <summary>
        /// Parses CSV content into ShortInterestData objects
        /// </summary>
        /// <param name="csvContent">The CSV content as a string</param>
        /// <returns>A list of ShortInterestData objects</returns>
        private List<ShortInterestData> ParseShortInterestCsv(string csvContent)
        {
            try
            {
                var shortInterestData = new List<ShortInterestData>();
                
                // Split the CSV content into lines
                string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Skip the header line
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string[] fields = line.Split(',');
                    
                    // Ensure we have enough fields
                    if (fields.Length < 3)
                    {
                        continue;
                    }
                    
                    // Parse date
                    if (!DateTime.TryParse(fields[0], out DateTime date))
                    {
                        continue;
                    }
                    
                    // Parse shares short
                    if (!long.TryParse(fields[1].Replace(",", ""), out long sharesShort))
                    {
                        continue;
                    }
                    
                    // Parse short interest percentage
                    if (!decimal.TryParse(fields[2].Replace("%", ""), out decimal shortInterest))
                    {
                        continue;
                    }
                    
                    shortInterestData.Add(new ShortInterestData
                    {
                        Date = date,
                        SharesShort = sharesShort,
                        ShortInterest = shortInterest
                    });
                }
                
                _logger.LogInformation("Successfully parsed {Count} short interest data points", shortInterestData.Count);
                
                // Sort by date ascending
                return shortInterestData.OrderBy(d => d.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing short interest CSV data");
                return new List<ShortInterestData>();
            }
        }

        /// <summary>
        /// Gets the short volume data for a specific stock from Chart Exchange
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., BYND)</param>
        /// <param name="exchange">The exchange (e.g., nasdaq)</param>
        /// <returns>A list of short volume data points</returns>
        public async Task<List<ShortVolumeData>> GetShortVolumeDataAsync(string symbol, string exchange)
        {
            try
            {
                string url = $"https://chartexchange.com/download/?r={exchange.ToLower()}-{symbol.ToLower()}.short_volume&k=symboldata";
                _logger.LogInformation("Fetching short volume data from: {Url}", url);

                using var httpClient = _httpClientFactory.CreateClient("ChartExchange");
                
                // Add headers to mimic a browser request
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/csv,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://chartexchange.com/");
                
                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch short volume data: {StatusCode} {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return new List<ShortVolumeData>();
                }

                string csvContent = await response.Content.ReadAsStringAsync();
                return ParseShortVolumeCsv(csvContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching short volume data for {Symbol} on {Exchange}", symbol, exchange);
                return new List<ShortVolumeData>();
            }
        }

        /// <summary>
        /// Parses CSV content into ShortVolumeData objects
        /// </summary>
        /// <param name="csvContent">The CSV content as a string</param>
        /// <returns>A list of ShortVolumeData objects</returns>
        private List<ShortVolumeData> ParseShortVolumeCsv(string csvContent)
        {
            try
            {
                var shortVolumeData = new List<ShortVolumeData>();
                
                // Split the CSV content into lines
                string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Skip the header line
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string[] fields = line.Split(',');
                    
                    // Ensure we have enough fields
                    if (fields.Length < 3)
                    {
                        continue;
                    }
                    
                    // Parse date
                    if (!DateTime.TryParse(fields[0], out DateTime date))
                    {
                        continue;
                    }
                    
                    // Parse short volume
                    if (!long.TryParse(fields[1].Replace(",", ""), out long shortVolume))
                    {
                        continue;
                    }
                    
                    // Parse short volume percentage
                    if (!decimal.TryParse(fields[2].Replace("%", ""), out decimal shortVolumePercent))
                    {
                        continue;
                    }
                    
                    shortVolumeData.Add(new ShortVolumeData
                    {
                        Date = date,
                        ShortVolume = shortVolume,
                        ShortVolumePercent = shortVolumePercent
                    });
                }
                
                _logger.LogInformation("Successfully parsed {Count} short volume data points", shortVolumeData.Count);
                
                // Sort by date ascending
                return shortVolumeData.OrderBy(d => d.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing short volume CSV data");
                return new List<ShortVolumeData>();
            }
        }
    }
}

// Made with Bob
