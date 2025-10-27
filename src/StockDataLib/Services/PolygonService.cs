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
                    // Polygon returns an array of short interest data
                    var apiDataList = JsonConvert.DeserializeObject<List<PolygonShortInterestBarData>>(content);
                    
                    if (apiDataList != null && apiDataList.Count > 0)
                    {
                        foreach (var apiData in apiDataList)
                        {
                            if (DateTime.TryParse(apiData.Date, out DateTime parsedDate))
                            {
                                shortInterestData.Add(new PolygonShortInterestData
                                {
                                    StockTickerSymbol = symbol.ToUpper(),
                                    Date = parsedDate.Date,
                                    ShortInterest = apiData.ShortInterest,
                                    AvgDailyVolume = apiData.AvgDailyVolume,
                                    DaysToCover = apiData.DaysToCover,
                                    SettlementDate = parsedDate.Date,
                                    PolygonRequestId = response.RequestMessage?.RequestUri?.ToString()
                                });
                            }
                        }
                        
                        _logger.LogInformation("Successfully fetched {Count} short interest records from Polygon", 
                            shortInterestData.Count);
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
                    // Try to parse as short volume response
                    var apiData = JsonConvert.DeserializeObject<dynamic>(content);
                    
                    if (apiData?.results != null)
                    {
                        foreach (var result in apiData.results)
                        {
                            try
                            {
                                long timestamp = result.t;
                                DateTime date = DateTimeOffset.FromUnixTimeMilliseconds((long)timestamp).UtcDateTime.Date;

                                // Check if short volume fields exist
                                if (result.short_volume != null || result.si != null)
                                {
                                    shortVolumeData.Add(new PolygonShortVolumeData
                                    {
                                        StockTickerSymbol = symbol.ToUpper(),
                                        Date = date,
                                        ShortVolume = (long)(result.short_volume ?? result.si?.short_volume ?? 0),
                                        TotalVolume = (long)(result.v ?? 0),
                                        ShortVolumeRatio = (decimal)(result.short_volume_ratio ?? 0),
                                        PolygonRequestId = response.RequestMessage?.RequestUri?.ToString()
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to parse individual short volume record");
                            }
                        }
                        
                        _logger.LogInformation("Successfully fetched {Count} short volume records from Polygon", 
                            shortVolumeData.Count);
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
