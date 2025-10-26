using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockDataLib.Models;

namespace StockDataLib.Services
{
    public interface IAlphaVantageService
    {
        Task<List<PriceData>> GetDailyPriceDataAsync(string symbol);
    }

    public class AlphaVantageService : IAlphaVantageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AlphaVantageService> _logger;
        private readonly AlphaVantageOptions _options;

        public AlphaVantageService(
            IHttpClientFactory httpClientFactory,
            ILogger<AlphaVantageService> logger,
            IOptions<AlphaVantageOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Gets daily price data for a specific stock from Alpha Vantage
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of price data points</returns>
        public async Task<List<PriceData>> GetDailyPriceDataAsync(string symbol)
        {
            try
            {
                string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=compact&apikey={_options.ApiKey}";
                _logger.LogInformation("Fetching daily price data for {Symbol} from Alpha Vantage", symbol);

                using var httpClient = _httpClientFactory.CreateClient("AlphaVantage");
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch data: {StatusCode} {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return new List<PriceData>();
                }

                var content = await response.Content.ReadAsStringAsync();
                return ParseAlphaVantageResponse(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price data for {Symbol}", symbol);
                return new List<PriceData>();
            }
        }

        private List<PriceData> ParseAlphaVantageResponse(string jsonResponse)
        {
            try
            {
                var result = new List<PriceData>();
                
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                // Check if the response contains an error message
                if (root.TryGetProperty("Error Message", out _))
                {
                    _logger.LogWarning("Alpha Vantage returned an error: {Response}", jsonResponse);
                    return result;
                }

                // Check if the response contains the time series data
                if (!root.TryGetProperty("Time Series (Daily)", out JsonElement timeSeriesElement))
                {
                    _logger.LogWarning("Alpha Vantage response does not contain time series data: {Response}", jsonResponse);
                    return result;
                }

                // Process each date in the time series
                foreach (JsonProperty dateProperty in timeSeriesElement.EnumerateObject())
                {
                    if (!DateTime.TryParse(dateProperty.Name, out DateTime date))
                    {
                        continue;
                    }

                    JsonElement dataPoint = dateProperty.Value;
                    
                    // Extract OHLC values
                    if (!dataPoint.TryGetProperty("1. open", out JsonElement openElement) ||
                        !dataPoint.TryGetProperty("2. high", out JsonElement highElement) ||
                        !dataPoint.TryGetProperty("3. low", out JsonElement lowElement) ||
                        !dataPoint.TryGetProperty("4. close", out JsonElement closeElement))
                    {
                        continue;
                    }

                    // Parse the values
                    if (!decimal.TryParse(openElement.GetString(), out decimal open) ||
                        !decimal.TryParse(highElement.GetString(), out decimal high) ||
                        !decimal.TryParse(lowElement.GetString(), out decimal low) ||
                        !decimal.TryParse(closeElement.GetString(), out decimal close))
                    {
                        continue;
                    }

                    // Create price data point
                    result.Add(new PriceData
                    {
                        Date = date,
                        Open = open,
                        High = high,
                        Low = low,
                        Close = close
                    });
                }

                _logger.LogInformation("Successfully extracted {Count} price data points", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Alpha Vantage response");
                return new List<PriceData>();
            }
        }
    }

    public class AlphaVantageOptions
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}

// Made with Bob
