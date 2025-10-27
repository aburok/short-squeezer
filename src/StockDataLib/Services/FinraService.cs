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
    public interface IFinraService
    {
        Task<List<FinraApiResponseData>> GetShortInterestDataAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<FinraApiResponseData>> GetAllShortInterestDataAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    public class FinraService : IFinraService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FinraService> _logger;
        private readonly FinraOptions _options;

        public FinraService(
            IHttpClientFactory httpClientFactory,
            ILogger<FinraService> logger,
            IOptions<FinraOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Gets short interest data for a specific symbol from FINRA API
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>A list of FINRA short interest data points</returns>
        public async Task<List<FinraApiResponseData>> GetShortInterestDataAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Build the query parameters
                var queryParams = new List<string>();
                
                if (startDate.HasValue)
                {
                    queryParams.Add($"settlementDate.gte={startDate.Value:yyyy-MM-dd}");
                }
                
                if (endDate.HasValue)
                {
                    queryParams.Add($"settlementDate.lte={endDate.Value:yyyy-MM-dd}");
                }

                // Add symbol filter
                queryParams.Add($"symbol={symbol.ToUpper()}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"https://api.finra.org/data/group/OTCMarket/name/equityShortInterest{queryString}";

                _logger.LogInformation("Fetching FINRA short interest data for {Symbol} from {Url}", symbol, url);

                using var httpClient = _httpClientFactory.CreateClient("Finra");
                
                // Add authentication headers
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch FINRA data: {StatusCode} {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return new List<FinraApiResponseData>();
                }

                var content = await response.Content.ReadAsStringAsync();
                return ParseFinraResponse(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FINRA short interest data for {Symbol}", symbol);
                return new List<FinraApiResponseData>();
            }
        }

        /// <summary>
        /// Gets all short interest data from FINRA API (for bulk data collection)
        /// </summary>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>A list of FINRA short interest data points</returns>
        public async Task<List<FinraApiResponseData>> GetAllShortInterestDataAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Build the query parameters
                var queryParams = new List<string>();
                
                if (startDate.HasValue)
                {
                    queryParams.Add($"settlementDate.gte={startDate.Value:yyyy-MM-dd}");
                }
                
                if (endDate.HasValue)
                {
                    queryParams.Add($"settlementDate.lte={endDate.Value:yyyy-MM-dd}");
                }

                // Add limit to prevent overwhelming the API
                queryParams.Add("limit=10000");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"https://api.finra.org/data/group/OTCMarket/name/equityShortInterest{queryString}";

                _logger.LogInformation("Fetching all FINRA short interest data from {Url}", url);

                using var httpClient = _httpClientFactory.CreateClient("Finra");
                
                // Add authentication headers
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch FINRA data: {StatusCode} {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return new List<FinraApiResponseData>();
                }

                var content = await response.Content.ReadAsStringAsync();
                return ParseFinraResponse(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all FINRA short interest data");
                return new List<FinraApiResponseData>();
            }
        }

        private List<FinraApiResponseData> ParseFinraResponse(string jsonResponse)
        {
            try
            {
                var result = new List<FinraApiResponseData>();
                
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                // Check if the response contains data array
                if (!root.TryGetProperty("data", out JsonElement dataElement))
                {
                    _logger.LogWarning("FINRA response does not contain data array: {Response}", jsonResponse);
                    return result;
                }

                // Process each data point
                foreach (JsonElement dataPoint in dataElement.EnumerateArray())
                {
                    try
                    {
                        var finraData = new FinraApiResponseData();

                        // Parse each field from the FINRA response
                        foreach (JsonProperty property in dataPoint.EnumerateObject())
                        {
                            switch (property.Name.ToLower())
                            {
                                case "symbol":
                                    finraData.Symbol = property.Value.GetString() ?? "";
                                    break;
                                case "settlementdate":
                                    if (DateTime.TryParse(property.Value.GetString(), out DateTime settlementDate))
                                    {
                                        finraData.SettlementDate = settlementDate;
                                    }
                                    break;
                                case "shortinterest":
                                    if (long.TryParse(property.Value.GetString(), out long shortInterest))
                                    {
                                        finraData.ShortInterest = shortInterest;
                                    }
                                    break;
                                case "shortinterestpercent":
                                    if (decimal.TryParse(property.Value.GetString(), out decimal shortInterestPercent))
                                    {
                                        finraData.ShortInterestPercent = shortInterestPercent;
                                    }
                                    break;
                                case "marketvalue":
                                    if (decimal.TryParse(property.Value.GetString(), out decimal marketValue))
                                    {
                                        finraData.MarketValue = marketValue;
                                    }
                                    break;
                                case "sharesoutstanding":
                                    if (long.TryParse(property.Value.GetString(), out long sharesOutstanding))
                                    {
                                        finraData.SharesOutstanding = sharesOutstanding;
                                    }
                                    break;
                                case "avgdailyvolume":
                                    if (long.TryParse(property.Value.GetString(), out long avgDailyVolume))
                                    {
                                        finraData.AvgDailyVolume = avgDailyVolume;
                                    }
                                    break;
                                case "days2cover":
                                    if (decimal.TryParse(property.Value.GetString(), out decimal days2Cover))
                                    {
                                        finraData.Days2Cover = days2Cover;
                                    }
                                    break;
                            }
                        }

                        // Only add if we have essential data
                        if (!string.IsNullOrEmpty(finraData.Symbol) && finraData.SettlementDate != default)
                        {
                            result.Add(finraData);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing individual FINRA data point");
                        continue;
                    }
                }

                _logger.LogInformation("Successfully extracted {Count} FINRA short interest data points", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing FINRA response");
                return new List<FinraApiResponseData>();
            }
        }
    }

    public class FinraOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.finra.org";
    }

    /// <summary>
    /// Represents FINRA short interest data from their API (DTO for API responses)
    /// </summary>
    public class FinraApiResponseData
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime SettlementDate { get; set; }
        public long ShortInterest { get; set; }
        public decimal ShortInterestPercent { get; set; }
        public decimal MarketValue { get; set; }
        public long SharesOutstanding { get; set; }
        public long AvgDailyVolume { get; set; }
        public decimal Days2Cover { get; set; }
    }
}

// Made with Bob
