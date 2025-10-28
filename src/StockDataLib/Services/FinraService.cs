using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockDataLib.Models;

namespace StockDataLib.Services
{
    public interface IFinraService
    {
        Task<List<FinraApiResponseData>> GetShortInterestDataAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<FinraApiResponseData>> GetAllShortInterestDataAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<FinraBlocksSummaryData>> GetBlocksSummaryDataAsync(DateTime? startDate = null, DateTime? endDate = null, string symbol = null);
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
                string url = $"{_options.ApiUrl}/data/group/OTCMarket/name/blocksSummaryMock{queryString}";

                _logger.LogInformation("Fetching FINRA short interest data for {Symbol} from {Url}", symbol, url);

                using var httpClient = _httpClientFactory.CreateClient("Finra");
                
                // Get OAuth token
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to obtain OAuth token for FINRA API");
                    return new List<FinraApiResponseData>();
                }
                _logger.LogInformation("Token obtained successfully: {token}", token);
                
                // Add authentication headers
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
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
                
                // if (startDate.HasValue)
                // {
                //     queryParams.Add($"settlementDate.gte={startDate.Value:yyyy-MM-dd}");
                // }
                //
                // if (endDate.HasValue)
                // {
                //     queryParams.Add($"settlementDate.lte={endDate.Value:yyyy-MM-dd}");
                // }

                // Add limit to prevent overwhelming the API
                queryParams.Add("limit=10");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"{_options.ApiUrl}data/group/OTCMarket/name/equityShortInterest{queryString}";

                _logger.LogInformation("Fetching all FINRA short interest data from {Url}", url);

                using var httpClient = _httpClientFactory.CreateClient("Finra");
                
                // Get OAuth token
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to obtain OAuth token for FINRA API");
                    return new List<FinraApiResponseData>();
                }
                
                // Add authentication headers
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
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

        /// <summary>
        /// Gets blocks summary data from FINRA API
        /// </summary>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="symbol">Optional symbol filter</param>
        /// <returns>A list of FINRA blocks summary data points</returns>
        public async Task<List<FinraBlocksSummaryData>> GetBlocksSummaryDataAsync(DateTime? startDate = null, DateTime? endDate = null, string symbol = null)
        {
            try
            {
                // Build the query parameters
                var queryParams = new List<string>();
                
                if (startDate.HasValue)
                {
                    queryParams.Add($"lastUpdateDate.gte={startDate.Value:yyyy-MM-dd}");
                }
                
                if (endDate.HasValue)
                {
                    queryParams.Add($"lastUpdateDate.lte={endDate.Value:yyyy-MM-dd}");
                }
                
                if (!string.IsNullOrEmpty(symbol))
                {
                    queryParams.Add($"MPID={symbol.ToUpper()}");
                }

                // Add limit to prevent overwhelming the API
                queryParams.Add("limit=1000");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"{_options.ApiUrl}data/group/OTCMarket/name/blocksSummary{queryString}";

                _logger.LogInformation("Fetching FINRA blocks summary data from {Url}", url);

                using var httpClient = _httpClientFactory.CreateClient("Finra");
                
                // Get OAuth token
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to obtain OAuth token for FINRA API");
                    return new List<FinraBlocksSummaryData>();
                }
                _logger.LogInformation("Token obtained successfully: {token}", token);
                
                // Add authentication headers
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var failContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to fetch FINRA blocks summary data: {StatusCode} {ReasonPhrase} {content}", 
                        response.StatusCode, response.ReasonPhrase, failContent);
                    return new List<FinraBlocksSummaryData>();
                }

                var content = await response.Content.ReadAsStringAsync();
                return ParseBlocksSummaryResponse(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FINRA blocks summary data");
                return new List<FinraBlocksSummaryData>();
            }
        }

        /// <summary>
        /// Gets OAuth access token using client credentials with Basic Authentication
        /// </summary>
        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.ClientSecret))
                {
                    _logger.LogError("FINRA Client ID or Client Secret not configured");
                    return string.Empty;
                }

                using var httpClient = _httpClientFactory.CreateClient("Finra");
                
                // Create Basic Authentication header
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                
                // Prepare the token request body
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var tokenUrl = _options.TokenUrl;
                _logger.LogInformation("Requesting OAuth token from {Url}", tokenUrl);

                var response = await httpClient.PostAsync(tokenUrl, requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get OAuth token: {StatusCode} {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error response: {ErrorContent}", errorContent);
                    return string.Empty;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseContent);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("access_token", out JsonElement tokenElement))
                {
                    var token = tokenElement.GetString();
                    _logger.LogInformation("Successfully obtained OAuth token");
                    return token ?? string.Empty;
                }

                _logger.LogError("OAuth token response does not contain access_token. Response: {Response}", responseContent);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining OAuth token");
                return string.Empty;
            }
        }

        private List<FinraApiResponseData> ParseFinraResponse(string jsonResponse)
        {
            try
            {
                // Log first part of response for debugging
                var previewLength = Math.Min(500, jsonResponse.Length);
                _logger.LogInformation("Response preview from FINRA: {Preview}", jsonResponse.Substring(0, previewLength));
                
                var result = new List<FinraApiResponseData>();
                
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                JsonElement dataElement;
                
                // Check if response is a direct array or has a "data" wrapper
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    dataElement = root;
                }
                else if (root.TryGetProperty("data", out dataElement))
                {
                    // Has data wrapper
                }
                else
                {
                    _logger.LogWarning("FINRA response format not recognized. Root type: {ValueKind}", root.ValueKind);
                    return result;
                }

                // Process each data point
                foreach (JsonElement dataPoint in dataElement.EnumerateArray())
                {
                    try
                    {
                        var finraData = ParseBlocksSummaryItem(dataPoint);
                        
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

                _logger.LogInformation("Successfully extracted {Count} FINRA data points", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing FINRA response");
                return new List<FinraApiResponseData>();
            }
        }

        /// <summary>
        /// Parses a single data point from FINRA blocks summary or short interest data
        /// </summary>
        private FinraApiResponseData ParseBlocksSummaryItem(JsonElement dataPoint)
        {
            var finraData = new FinraApiResponseData();

            foreach (JsonProperty property in dataPoint.EnumerateObject())
            {
                var propertyName = property.Name.ToLower();
                var propertyValue = property.Value;

                // Map blocks summary fields to short interest data structure
                switch (propertyName)
                {
                    // For blocks summary, we'll use available date fields
                    case "lastupdatedate":
                    case "initialpublisheddate":
                    case "lastreporteddate":
                        if (DateTime.TryParse(propertyValue.GetString(), out DateTime dateValue))
                        {
                            finraData.SettlementDate = dateValue;
                        }
                        break;

                    case "atssharepercent":
                        if (decimal.TryParse(propertyValue.GetRawText(), out decimal sharePercent))
                        {
                            finraData.ShortInterestPercent = sharePercent;
                        }
                        break;

                    case "atsblockquantity":
                    case "totalsharequantity":
                        if (long.TryParse(propertyValue.GetRawText(), out long quantity))
                        {
                            finraData.ShortInterest = quantity;
                        }
                        break;

                    case "averagetradesize":
                        if (long.TryParse(propertyValue.GetRawText(), out long avgTradeSize))
                        {
                            finraData.AvgDailyVolume = avgTradeSize;
                        }
                        break;

                    case "marketparticipantname":
                        finraData.Symbol = propertyValue.GetString() ?? "";
                        break;

                    case "mpid":
                        if (string.IsNullOrEmpty(finraData.Symbol))
                        {
                            finraData.Symbol = propertyValue.GetString() ?? "";
                        }
                        break;
                }
            }

            return finraData;
        }

        /// <summary>
        /// Parses blocks summary response from FINRA API using Newtonsoft.Json
        /// </summary>
        private List<FinraBlocksSummaryData> ParseBlocksSummaryResponse(string jsonResponse)
        {
            try
            {
                // Try to deserialize directly
                var result = JsonConvert.DeserializeObject<List<FinraBlocksSummaryData>>(jsonResponse);
                
                if (result != null)
                {
                    _logger.LogInformation("Successfully parsed {Count} blocks summary data points", result.Count);
                    return result;
                }

                // If direct deserialization fails, try to parse as wrapped response
                var token = JToken.Parse(jsonResponse);
                if (token.Type == JTokenType.Array)
                {
                    result = JsonConvert.DeserializeObject<List<FinraBlocksSummaryData>>(jsonResponse);
                }
                else if (token is JObject obj && obj["data"] != null)
                {
                    result = obj["data"].ToObject<List<FinraBlocksSummaryData>>();
                }

                if (result == null)
                {
                    _logger.LogWarning("Could not parse FINRA blocks summary response");
                    return new List<FinraBlocksSummaryData>();
                }

                _logger.LogInformation("Successfully parsed {Count} blocks summary data points", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing FINRA blocks summary response");
                return new List<FinraBlocksSummaryData>();
            }
        }
    }

    public class FinraOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TokenUrl { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
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

    /// <summary>
    /// Represents FINRA Blocks Summary data from their API
    /// </summary>
    [JsonObject]
    public class FinraBlocksSummaryData
    {
        [JsonProperty("atsOtc")]
        public string AtsOtc { get; set; } = string.Empty;
        
        [JsonProperty("ATSBlockSharePercent")]
        public decimal ATSBlockSharePercent { get; set; }
        
        [JsonProperty("lastUpdateDate")]
        public DateTime LastUpdateDate { get; set; }
        
        [JsonProperty("ATSBlockBusinessSharePercent")]
        public decimal ATSBlockBusinessSharePercent { get; set; }
        
        [JsonProperty("averageTradeSize")]
        public long AverageTradeSize { get; set; }
        
        [JsonProperty("averageBlockSizeRank")]
        public int AverageBlockSizeRank { get; set; }
        
        [JsonProperty("ATSBlockCount")]
        public long ATSBlockCount { get; set; }
        
        [JsonProperty("initialPublishedDate")]
        public DateTime InitialPublishedDate { get; set; }
        
        [JsonProperty("summaryStartDate")]
        public DateTime SummaryStartDate { get; set; }
        
        [JsonProperty("ATSBlockQuantity")]
        public long ATSBlockQuantity { get; set; }
        
        [JsonProperty("ATSShareRank")]
        public int ATSShareRank { get; set; }
        
        [JsonProperty("averageBlockSize")]
        public long AverageBlockSize { get; set; }
        
        [JsonProperty("ATSBlockTradeRank")]
        public int ATSBlockTradeRank { get; set; }
        
        [JsonProperty("averageTradeSizeRank")]
        public int AverageTradeSizeRank { get; set; }
        
        [JsonProperty("ATSTradeRank")]
        public int ATSTradeRank { get; set; }
        
        [JsonProperty("ATSBlockBusinessTradePercent")]
        public decimal ATSBlockBusinessTradePercent { get; set; }
        
        [JsonProperty("MPID")]
        public string MPID { get; set; } = string.Empty;
        
        [JsonProperty("monthStartDate")]
        public DateTime MonthStartDate { get; set; }
        
        [JsonProperty("ATSSharePercent")]
        public decimal ATSSharePercent { get; set; }
        
        [JsonProperty("ATSBlockShareRank")]
        public int ATSBlockShareRank { get; set; }
        
        [JsonProperty("ATSBlockTradePercent")]
        public decimal ATSBlockTradePercent { get; set; }
        
        [JsonProperty("marketParticipantName")]
        public string MarketParticipantName { get; set; } = string.Empty;
        
        [JsonProperty("summaryTypeCode")]
        public string SummaryTypeCode { get; set; } = string.Empty;
        
        [JsonProperty("ATSTradePercent")]
        public decimal ATSTradePercent { get; set; }
        
        [JsonProperty("lastReportedDate")]
        public DateTime LastReportedDate { get; set; }
        
        [JsonProperty("ATSBlockBusinessTradeRank")]
        public int ATSBlockBusinessTradeRank { get; set; }
        
        [JsonProperty("totalShareQuantity")]
        public long TotalShareQuantity { get; set; }
        
        [JsonProperty("ATSBlockBusinessShareRank")]
        public int ATSBlockBusinessShareRank { get; set; }
        
        [JsonProperty("summaryTypeDescription")]
        public string SummaryTypeDescription { get; set; } = string.Empty;
        
        [JsonProperty("totalTradeCount")]
        public long TotalTradeCount { get; set; }
    }
}

// Made with Bob
