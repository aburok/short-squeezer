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
    public interface IChartExchangeService
    {
        Task<List<ChartExchangeFailureToDeliver>> GetFailureToDeliverDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        Task<List<ChartExchangeRedditMentions>> GetRedditMentionsDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        Task<List<ChartExchangeOptionChain>> GetOptionChainDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        Task<List<ChartExchangeStockSplit>> GetStockSplitDataAsync(string symbol, DateTime startDate, DateTime endDate);

        Task<List<ChartExchangeShortInterest>> GetShortInterestDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        Task<List<ChartExchangeShortVolume>> GetShortVolumeDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        Task<List<ChartExchangeBorrowFee>> GetBorrowFeeDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        // Legacy method for borrow fee data (scraping from ChartExchange website)
        Task<List<BorrowFeeData>> GetBorrowFeeDataAsync(string symbol, string exchange, DateTime startDate,
            DateTime endDate);
    }

    public class ChartExchangeService : IChartExchangeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChartExchangeService> _logger;
        private readonly ChartExchangeOptions _options;

        public ChartExchangeService(
            IHttpClientFactory httpClientFactory,
            ILogger<ChartExchangeService> logger,
            IOptions<ChartExchangeOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Gets failure to deliver data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeFailureToDeliver>> GetFailureToDeliverDataAsync(string symbol,
            DateTime startDate, DateTime endDate)
        {
            var url = "/data/stocks/failure-to-deliver/";
            var data = await GetData<ChartExchangeFailureToDeliverResponse, ChartExchangeFailureToDeliverData, ChartExchangeFailureToDeliver>(
                symbol, url, (d, response) => new ChartExchangeFailureToDeliver()
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
                    FailureToDeliver = d.FailureToDeliver,
                    Price = d.Price,
                    Volume = d.Volume,
                    SettlementDate = !string.IsNullOrEmpty(d.SettlementDate) ? DateTime.Parse(d.SettlementDate) : null,
                    Cusip = d.Cusip,
                    CompanyName = d.CompanyName,
                    ChartExchangeRequestId = response.Timestamp?.ToString()
                });
            return data;
        }

        /// <summary>
        /// Gets Reddit mentions data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeRedditMentions>> GetRedditMentionsDataAsync(string symbol,
            DateTime startDate, DateTime endDate)
        {
            var url = "/data/stocks/short-volume/";
            var data = await GetData<ChartExchangeRedditMentionsResponse, ChartExchangeRedditMentionsData, ChartExchangeRedditMentions>(
                symbol, url, (d, response) => new ChartExchangeRedditMentions()
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
                    Mentions = d.Mentions,
                    SentimentScore = d.SentimentScore,
                    SentimentLabel = d.SentimentLabel,
                    Subreddit = d.Subreddit,
                    Upvotes = d.Upvotes,
                    Comments = d.Comments,
                    EngagementScore = d.EngagementScore,
                    ChartExchangeRequestId = response.Timestamp?.ToString()
                });
            return data;

        }

        /// <summary>
        /// Gets option chain data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeOptionChain>> GetOptionChainDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            var url = "/data/options/chain-summary/";
            var data = await GetData<ChartExchangeOptionChainResponse, ChartExchangeOptionChainData, ChartExchangeOptionChain>(
                symbol, url, (d, response) => new ChartExchangeOptionChain()
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
                    ExpirationDate = d.ExpirationDate,
                    StrikePrice = d.StrikePrice,
                    OptionType = d.OptionType,
                    Volume = d.Volume,
                    OpenInterest = d.OpenInterest,
                    Bid = d.Bid,
                    Ask = d.Ask,
                    LastPrice = d.LastPrice,
                    ImpliedVolatility = d.ImpliedVolatility,
                    Delta = d.Delta,
                    Gamma = d.Gamma,
                    Theta = d.Theta,
                    Vega = d.Vega,
                    ChartExchangeRequestId = response.Timestamp?.ToString()
                });
            return data;
        }

        /// <summary>
        /// Gets stock split data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeStockSplit>> GetStockSplitDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            var url = "/data/stocks/splits/";
            var data = await GetData<ChartExchangeStockSplitResponse, ChartExchangeStockSplitData, ChartExchangeStockSplit>(
                symbol, url, (d, response) => new ChartExchangeStockSplit()
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
                    SplitRatio = d.SplitRatio,
                    SplitFactor = d.SplitFactor,
                    FromFactor = d.FromFactor,
                    ToFactor = d.ToFactor,
                    ExDate = !string.IsNullOrEmpty(d.ExDate) ? DateTime.Parse(d.ExDate) : null,
                    RecordDate = !string.IsNullOrEmpty(d.RecordDate) ? DateTime.Parse(d.RecordDate) : null,
                    PayableDate = !string.IsNullOrEmpty(d.PayableDate) ? DateTime.Parse(d.PayableDate) : null,
                    AnnouncementDate = !string.IsNullOrEmpty(d.AnnouncementDate)
                        ? DateTime.Parse(d.AnnouncementDate)
                        : null,
                    CompanyName = d.CompanyName,
                    ChartExchangeRequestId = response.Timestamp?.ToString()
                });
            return data;
        }

        /// <summary>
        /// Gets short interest data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeShortInterest>> GetShortInterestDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<ChartExchangeShortInterest>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url = $"{_options.BaseUrl}/api/v1/data/stocks/short-interest/?symbol={symbol}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange short interest data for {Symbol}", symbol);
                _logger.LogInformation("Request URL: {Url}", url);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<List<ChartExchangeShortInterestData>>(jsonContent);

                if (apiResponse == null)
                {
                    _logger.LogWarning("No short interest data returned from ChartExchange for {Symbol}", symbol);
                    return new List<ChartExchangeShortInterest>();
                }

                var shortInterestData = apiResponse.Select(d => new ChartExchangeShortInterest
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
                    ShortInterestPercent = decimal.TryParse(d.ShortInterest, out var shortInterestPercent) ? shortInterestPercent : 0,
                    ShortPosition = d.ShortPosition,
                    DaysToCover = decimal.TryParse(d.DaysToCover, out var daysToCover) ? daysToCover : 0,
                    ChangeNumber = d.ChangeNumber,
                    ChangePercent = decimal.TryParse(d.ChangePercent, out var changePercent) ? changePercent : 0,
                    ChartExchangeRequestId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} short interest data points for {Symbol}", shortInterestData.Count, symbol);
                return shortInterestData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange short interest data for {Symbol}", symbol);
                return new List<ChartExchangeShortInterest>();
            }
        }

        /// <summary>
        /// Gets short volume data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeShortVolume>> GetShortVolumeDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<ChartExchangeShortVolume>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url = $"{_options.BaseUrl}/api/v1/data/stocks/short-volume/?symbol={symbol}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange short volume data for {Symbol}", symbol);
                _logger.LogInformation("Request URL: {Url}", url);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<List<ChartExchangeShortVolumeData>>(jsonContent);

                if (apiResponse == null)
                {
                    _logger.LogWarning("No short volume data returned from ChartExchange for {Symbol}", symbol);
                    return new List<ChartExchangeShortVolume>();
                }

                var shortVolumeData = apiResponse.Select(d => new ChartExchangeShortVolume
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
                    Rt = d.Rt,
                    St = d.St,
                    Lt = d.Lt,
                    Fs = d.Fs,
                    Fse = d.Fse,
                    Xnas = d.Xnas,
                    Xphl = d.Xphl,
                    Xnys = d.Xnys,
                    Arcx = d.Arcx,
                    Xcis = d.Xcis,
                    Xase = d.Xase,
                    Xchi = d.Xchi,
                    Edgx = d.Edgx,
                    Bats = d.Bats,
                    Edga = d.Edga,
                    Baty = d.Baty,
                    ShortVolumePercent = d.Rt > 0 ? (decimal)d.St / d.Rt * 100 : 0,
                    ChartExchangeRequestId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} short volume data points for {Symbol}", shortVolumeData.Count, symbol);
                return shortVolumeData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange short volume data for {Symbol}", symbol);
                return new List<ChartExchangeShortVolume>();
            }
        }

        /// <summary>
        /// Gets borrow fee data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeBorrowFee>> GetBorrowFeeDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<ChartExchangeBorrowFee>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url = $"{_options.BaseUrl}/api/v1/data/stocks/borrow-fee/?symbol={symbol}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange borrow fee data for {Symbol}", symbol);
                _logger.LogInformation("Request URL: {Url}", url);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ChartExchangeBorrowFeeResponse>(jsonContent);

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("No borrow fee data returned from ChartExchange for {Symbol}", symbol);
                    return new List<ChartExchangeBorrowFee>();
                }

                var borrowFeeData = apiResponse.Data.Select(d => new ChartExchangeBorrowFee
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Timestamp),
                    Available = d.Available,
                    Fee = decimal.TryParse(d.Fee, out var fee) ? fee : 0,
                    Rebate = decimal.TryParse(d.Rebate, out var rebate) ? rebate : 0,
                    ChartExchangeRequestId = apiResponse.Timestamp?.ToString()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} borrow fee data points for {Symbol}", borrowFeeData.Count, symbol);
                return borrowFeeData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange borrow fee data for {Symbol}", symbol);
                return new List<ChartExchangeBorrowFee>();
            }
        }

        private async Task<List<TModel>> GetData<TResponse, TItem, TModel>(string symbol,
            string relativeUrl,
            Func<TItem, TResponse, TModel> map
        )
            where TResponse : ChartExchangePagedResponse<TItem>
            where TModel : StockDataPoint
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<TModel>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url =
                    $"{_options.BaseUrl}/api/v1{relativeUrl}?symbol={symbol}&api_key={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange short volume data for {Symbol}", symbol);
                _logger.LogInformation("Request URL: {Url}", url);

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<TResponse>(jsonContent);

                    if (apiResponse?.Data == null)
                    {
                        _logger.LogWarning("No short volume data returned from ChartExchange for {Symbol}", symbol);
                        return new List<TModel>();
                    }

                    var shortVolumeData = apiResponse.Data.Select(d => map(d, apiResponse)).ToList();

                    _logger.LogInformation("Retrieved {Count} short volume data points for {Symbol}",
                        shortVolumeData.Count,
                        symbol);
                    return shortVolumeData;
                }
                else
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error while executing request: {url} " + jsonContent, url);
                }
                return new List<TModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange short volume data for {Symbol}", symbol);
                return new List<TModel>();
            }
        }

        /// <summary>
        /// Gets borrow fee data by scraping ChartExchange website (legacy method)
        /// </summary>
        public async Task<List<BorrowFeeData>> GetBorrowFeeDataAsync(string symbol, string exchange, DateTime startDate,
            DateTime endDate)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching ChartExchange borrow fee data for {Symbol} from {StartDate} to {EndDate}", symbol,
                    startDate, endDate);

                // This method would implement the original ChartExchange scraping logic
                // For now, return empty list as this functionality should be moved to a separate service
                _logger.LogWarning("Borrow fee scraping from ChartExchange website not implemented in new service");
                return new List<BorrowFeeData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange borrow fee data for {Symbol}", symbol);
                return new List<BorrowFeeData>();
            }
        }
    }

    /// <summary>
    /// Configuration options for ChartExchange API
    /// </summary>
    public class ChartExchangeOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.chartexchange.com";
        public int RateLimitPerMinute { get; set; } = 60;
        public bool IgnoreSslErrors { get; set; } = false;
    }
}