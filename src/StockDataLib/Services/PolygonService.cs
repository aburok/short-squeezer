using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockDataLib.Models;

namespace StockDataLib.Services
{
    public interface IPolygonService
    {
        Task<List<PolygonPriceData>> GetHistoricalDataAsync(string symbol, int years = 2);
        Task<PolygonDailyBarResponse> GetDailyBarsAsync(string symbol, DateTime startDate, DateTime endDate);
        Task<List<PolygonShortInterestData>> GetShortInterestDataAsync(string symbol);
        Task<List<PolygonShortVolumeData>> GetShortVolumeDataAsync(string symbol, DateTime startDate, DateTime endDate);
    }

    public class PolygonService : IPolygonService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PolygonService> _logger;
        private readonly PolygonOptions _options;

        public PolygonService(
            IHttpClientFactory httpClientFactory,
            ILogger<PolygonService> logger,
            IOptions<PolygonOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Gets 2 years of historical data for a symbol
        /// </summary>
        public async Task<List<PolygonPriceData>> GetHistoricalDataAsync(string symbol, int years = 2)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("Polygon API key not configured");
                    return new List<PolygonPriceData>();
                }

                DateTime endDate = DateTime.Now;
                DateTime startDate = endDate.AddYears(-years);

                _logger.LogInformation("Fetching {Years} years of data for {Symbol} from Polygon.io", years, symbol);

                var response = await GetDailyBarsAsync(symbol, startDate, endDate);

                if (response?.Results == null || response.Results.Count == 0)
                {
                    _logger.LogWarning("No data received from Polygon for {Symbol}", symbol);
                    return new List<PolygonPriceData>();
                }

                var priceDataList = response.Results.Select(bar => new PolygonPriceData
                {
                    StockTickerSymbol = symbol.ToUpper(),
                    Date = bar.GetDateTime(),
                    Open = bar.Open,
                    High = bar.High,
                    Low = bar.Low,
                    Close = bar.Close,
                    Volume = bar.Volume,
                    VolumeWeightedPrice = bar.VolumeWeightedPrice,
                    NumberOfTransactions = bar.NumberOfTransactions,
                    PolygonRequestId = response.RequestId
                }).ToList();

                _logger.LogInformation("Successfully fetched {Count} data points for {Symbol} from Polygon", 
                    priceDataList.Count, symbol);

                return priceDataList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching historical data for {Symbol} from Polygon", symbol);
                return new List<PolygonPriceData>();
            }
        }

        /// <summary>
        /// Gets daily bars for a symbol within a date range from Polygon.io API
        /// </summary>
        public async Task<PolygonDailyBarResponse> GetDailyBarsAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("Polygon API key not configured");
                    return new PolygonDailyBarResponse();
                }

                // Format dates for Polygon API (YYYY-MM-DD)
                string startDateStr = startDate.ToString("yyyy-MM-dd");
                string endDateStr = endDate.ToString("yyyy-MM-dd");

                // Build the API URL
                string url = $"{_options.BaseUrl}/v2/aggs/ticker/{symbol.ToUpper()}/range/1/day/{startDateStr}/{endDateStr}?apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching Polygon data for {Symbol} from {Url}", symbol, url.Replace(_options.ApiKey, "***"));

                using var httpClient = _httpClientFactory.CreateClient("Polygon");
                
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch Polygon data: {StatusCode} {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return new PolygonDailyBarResponse();
                }

                var content = await response.Content.ReadAsStringAsync();
                var polygonResponse = JsonConvert.DeserializeObject<PolygonDailyBarResponse>(content);

                if (polygonResponse != null && polygonResponse.Status == "OK")
                {
                    _logger.LogInformation("Successfully fetched {Count} data points from Polygon", 
                        polygonResponse.Results?.Count ?? 0);
                }
                else
                {
                    _logger.LogWarning("Polygon API returned non-OK status: {Status}", 
                        polygonResponse?.Status ?? "null");
                }

                return polygonResponse ?? new PolygonDailyBarResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching daily bars for {Symbol} from Polygon", symbol);
                return new PolygonDailyBarResponse();
            }
        }

        /// <summary>
        /// Gets short interest data for a symbol from Polygon.io API
        /// </summary>
        public async Task<List<PolygonShortInterestData>> GetShortInterestDataAsync(string symbol)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("Polygon API key not configured");
                    return new List<PolygonShortInterestData>();
                }

                // Build the API URL for short interest endpoint
                string url = $"{_options.BaseUrl}/v1/indicator/si?ticker={symbol.ToUpper()}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching short interest data for {Symbol} from Polygon.io", symbol);

                using var httpClient = _httpClientFactory.CreateClient("Polygon");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch Polygon short interest: {StatusCode} {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return new List<PolygonShortInterestData>();
                }

                var content = await response.Content.ReadAsStringAsync();
                
                // Parse the response
                var shortInterestData = new List<PolygonShortInterestData>();
                
                try
                {
                    // Deserialize the response with the correct structure
                    var apiResponse = JsonConvert.DeserializeObject<PolygonShortInterestResponse>(content);
                    
                    if (apiResponse != null && apiResponse.Status == "OK" && apiResponse.Results != null)
                    {
                        foreach (var apiData in apiResponse.Results)
                        {
                            // Parse settlement_date (YYYY-MM-DD format)
                            if (DateTime.TryParse(apiData.SettlementDate, out DateTime parsedDate))
                            {
                                shortInterestData.Add(new PolygonShortInterestData
                                {
                                    StockTickerSymbol = symbol.ToUpper(),
                                    Date = parsedDate.Date,
                                    ShortInterest = apiData.ShortInterest,
                                    AvgDailyVolume = apiData.AvgDailyVolume,
                                    DaysToCover = apiData.DaysToCover,
                                    SettlementDate = parsedDate.Date,
                                    PolygonRequestId = apiResponse.RequestId
                                });
                            }
                            else
                            {
                                _logger.LogWarning("Failed to parse settlement date: {Date}", apiData.SettlementDate);
                            }
                        }
                        
                        _logger.LogInformation("Successfully fetched {Count} short interest records from Polygon", 
                            shortInterestData.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Polygon API returned status: {Status}", apiResponse?.Status ?? "null");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse Polygon short interest response");
                }

                return shortInterestData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Polygon short interest data for {Symbol}", symbol);
                return new List<PolygonShortInterestData>();
            }
        }

        /// <summary>
        /// Gets short volume data for a symbol from Polygon.io API
        /// </summary>
        public async Task<List<PolygonShortVolumeData>> GetShortVolumeDataAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("Polygon API key not configured");
                    return new List<PolygonShortVolumeData>();
                }

                // Format dates for Polygon API
                string startDateStr = startDate.ToString("yyyy-MM-dd");
                string endDateStr = endDate.ToString("yyyy-MM-dd");

                // Build the API URL for short volume endpoint
                string url = $"{_options.BaseUrl}/v2/aggs/ticker/{symbol.ToUpper()}/range/1/day/{startDateStr}/{endDateStr}?si=true&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching short volume data for {Symbol} from {StartDate} to {EndDate} from Polygon.io", 
                    symbol, startDateStr, endDateStr);

                using var httpClient = _httpClientFactory.CreateClient("Polygon");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch Polygon short volume: {StatusCode} {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return new List<PolygonShortVolumeData>();
                }

                var content = await response.Content.ReadAsStringAsync();
                
                // Parse the response
                var shortVolumeData = new List<PolygonShortVolumeData>();
                
                try
                {
                    // Deserialize the response with the correct structure
                    var apiResponse = JsonConvert.DeserializeObject<PolygonShortVolumeResponse>(content);
                    
                    if (apiResponse != null && apiResponse.Status == "OK" && apiResponse.Results != null)
                    {
                        foreach (var apiData in apiResponse.Results)
                        {
                            // Parse date (YYYY-MM-DD format)
                            if (DateTime.TryParse(apiData.Date, out DateTime parsedDate))
                            {
                                shortVolumeData.Add(new PolygonShortVolumeData
                                {
                                    StockTickerSymbol = symbol.ToUpper(),
                                    Date = parsedDate.Date,
                                    ShortVolume = apiData.ShortVolume ?? 0,
                                    TotalVolume = apiData.TotalVolume ?? 0,
                                    ShortVolumeRatio = apiData.ShortVolumeRatio ?? 0,
                                    AdfShortVolume = apiData.AdfShortVolume,
                                    AdfShortVolumeExempt = apiData.AdfShortVolumeExempt,
                                    ExemptVolume = apiData.ExemptVolume,
                                    NasdaqCarteretShortVolume = apiData.NasdaqCarteretShortVolume,
                                    NasdaqCarteretShortVolumeExempt = apiData.NasdaqCarteretShortVolumeExempt,
                                    NasdaqChicagoShortVolume = apiData.NasdaqChicagoShortVolume,
                                    NasdaqChicagoShortVolumeExempt = apiData.NasdaqChicagoShortVolumeExempt,
                                    NonExemptVolume = apiData.NonExemptVolume,
                                    NyseShortVolume = apiData.NyseShortVolume,
                                    NyseShortVolumeExempt = apiData.NyseShortVolumeExempt,
                                    PolygonRequestId = apiResponse.RequestId
                                });
                            }
                            else
                            {
                                _logger.LogWarning("Failed to parse date: {Date}", apiData.Date);
                            }
                        }
                        
                        _logger.LogInformation("Successfully fetched {Count} short volume records from Polygon", 
                            shortVolumeData.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Polygon API returned status: {Status}", apiResponse?.Status ?? "null");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse Polygon short volume response");
                }

                return shortVolumeData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Polygon short volume data for {Symbol}", symbol);
                return new List<PolygonShortVolumeData>();
            }
        }
    }

    public class PolygonOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.polygon.io";
        public int RateLimitCallsPerMinute { get; set; } = 5;
        public int DelayBetweenCallsSeconds { get; set; } = 12;
    }
}
