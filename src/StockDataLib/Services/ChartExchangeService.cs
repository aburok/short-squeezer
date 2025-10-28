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
        Task<List<ChartExchangePrice>> GetPriceDataAsync(string symbol, DateTime startDate, DateTime endDate);
        Task<List<ChartExchangeFailureToDeliver>> GetFailureToDeliverDataAsync(string symbol, DateTime startDate, DateTime endDate);
        Task<List<ChartExchangeRedditMentions>> GetRedditMentionsDataAsync(string symbol, DateTime startDate, DateTime endDate);
        Task<List<ChartExchangeOptionChain>> GetOptionChainDataAsync(string symbol, DateTime startDate, DateTime endDate);
        Task<List<ChartExchangeStockSplit>> GetStockSplitDataAsync(string symbol, DateTime startDate, DateTime endDate);

        // Legacy method for borrow fee data (scraping from ChartExchange website)
        Task<List<BorrowFeeData>> GetBorrowFeeDataAsync(string symbol, string exchange, DateTime startDate, DateTime endDate);
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
        /// Gets price data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangePrice>> GetPriceDataAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<ChartExchangePrice>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url = $"{_options.BaseUrl}/api/v1/price/{symbol}?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange price data for {Symbol} from {StartDate} to {EndDate}", symbol, startDate, endDate);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ChartExchangePriceResponse>(jsonContent);

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("No price data returned from ChartExchange for {Symbol}", symbol);
                    return new List<ChartExchangePrice>();
                }

                var priceData = apiResponse.Data.Select(d => new ChartExchangePrice
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
                    Open = d.Open,
                    High = d.High,
                    Low = d.Low,
                    Close = d.Close,
                    Volume = d.Volume,
                    AdjustedClose = d.AdjustedClose,
                    DividendAmount = d.DividendAmount,
                    SplitCoefficient = d.SplitCoefficient,
                    ChartExchangeRequestId = apiResponse.Timestamp?.ToString()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} price data points for {Symbol}", priceData.Count, symbol);
                return priceData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange price data for {Symbol}", symbol);
                return new List<ChartExchangePrice>();
            }
        }

        /// <summary>
        /// Gets failure to deliver data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeFailureToDeliver>> GetFailureToDeliverDataAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<ChartExchangeFailureToDeliver>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url = $"{_options.BaseUrl}/api/v1/failure-to-deliver/{symbol}?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange failure to deliver data for {Symbol} from {StartDate} to {EndDate}", symbol, startDate, endDate);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ChartExchangeFailureToDeliverResponse>(jsonContent);

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("No failure to deliver data returned from ChartExchange for {Symbol}", symbol);
                    return new List<ChartExchangeFailureToDeliver>();
                }

                var failureToDeliverData = apiResponse.Data.Select(d => new ChartExchangeFailureToDeliver
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
                    FailureToDeliver = d.FailureToDeliver,
                    Price = d.Price,
                    Volume = d.Volume,
                    SettlementDate = !string.IsNullOrEmpty(d.SettlementDate) ? DateTime.Parse(d.SettlementDate) : null,
                    Cusip = d.Cusip,
                    CompanyName = d.CompanyName,
                    ChartExchangeRequestId = apiResponse.Timestamp?.ToString()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} failure to deliver data points for {Symbol}", failureToDeliverData.Count, symbol);
                return failureToDeliverData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange failure to deliver data for {Symbol}", symbol);
                return new List<ChartExchangeFailureToDeliver>();
            }
        }

        /// <summary>
        /// Gets Reddit mentions data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeRedditMentions>> GetRedditMentionsDataAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<ChartExchangeRedditMentions>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url = $"{_options.BaseUrl}/api/v1/reddit-mentions/{symbol}?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange Reddit mentions data for {Symbol} from {StartDate} to {EndDate}", symbol, startDate, endDate);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ChartExchangeRedditMentionsResponse>(jsonContent);

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("No Reddit mentions data returned from ChartExchange for {Symbol}", symbol);
                    return new List<ChartExchangeRedditMentions>();
                }

                var redditMentionsData = apiResponse.Data.Select(d => new ChartExchangeRedditMentions
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
                    ChartExchangeRequestId = apiResponse.Timestamp?.ToString()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} Reddit mentions data points for {Symbol}", redditMentionsData.Count, symbol);
                return redditMentionsData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange Reddit mentions data for {Symbol}", symbol);
                return new List<ChartExchangeRedditMentions>();
            }
        }

        /// <summary>
        /// Gets option chain data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeOptionChain>> GetOptionChainDataAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<ChartExchangeOptionChain>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url = $"{_options.BaseUrl}/api/v1/option-chain/{symbol}?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange option chain data for {Symbol} from {StartDate} to {EndDate}", symbol, startDate, endDate);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ChartExchangeOptionChainResponse>(jsonContent);

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("No option chain data returned from ChartExchange for {Symbol}", symbol);
                    return new List<ChartExchangeOptionChain>();
                }

                var optionChainData = apiResponse.Data.Select(d => new ChartExchangeOptionChain
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
                    ChartExchangeRequestId = apiResponse.Timestamp?.ToString()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} option chain data points for {Symbol}", optionChainData.Count, symbol);
                return optionChainData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange option chain data for {Symbol}", symbol);
                return new List<ChartExchangeOptionChain>();
            }
        }

        /// <summary>
        /// Gets stock split data for a symbol within a date range
        /// </summary>
        public async Task<List<ChartExchangeStockSplit>> GetStockSplitDataAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogError("ChartExchange API key not configured");
                    return new List<ChartExchangeStockSplit>();
                }

                var client = _httpClientFactory.CreateClient("ChartExchange");
                var url = $"{_options.BaseUrl}/api/v1/stock-splits/{symbol}?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&apikey={_options.ApiKey}";

                _logger.LogInformation("Fetching ChartExchange stock split data for {Symbol} from {StartDate} to {EndDate}", symbol, startDate, endDate);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ChartExchangeStockSplitResponse>(jsonContent);

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("No stock split data returned from ChartExchange for {Symbol}", symbol);
                    return new List<ChartExchangeStockSplit>();
                }

                var stockSplitData = apiResponse.Data.Select(d => new ChartExchangeStockSplit
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
                    AnnouncementDate = !string.IsNullOrEmpty(d.AnnouncementDate) ? DateTime.Parse(d.AnnouncementDate) : null,
                    CompanyName = d.CompanyName,
                    ChartExchangeRequestId = apiResponse.Timestamp?.ToString()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} stock split data points for {Symbol}", stockSplitData.Count, symbol);
                return stockSplitData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange stock split data for {Symbol}", symbol);
                return new List<ChartExchangeStockSplit>();
            }
        }

        /// <summary>
        /// Gets borrow fee data by scraping ChartExchange website (legacy method)
        /// </summary>
        public async Task<List<BorrowFeeData>> GetBorrowFeeDataAsync(string symbol, string exchange, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Fetching ChartExchange borrow fee data for {Symbol} from {StartDate} to {EndDate}", symbol, startDate, endDate);

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
    }
}