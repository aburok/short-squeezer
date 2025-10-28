using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StockDataApi.Models.Queries;
using StockDataApi.Models.Responses;
using StockDataLib.Data;

namespace StockDataApi.Handlers.Queries
{
    /// <summary>
    /// Query handler for retrieving all stock data for a ticker
    /// </summary>
    public class GetAllStockDataQueryHandler
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GetAllStockDataQueryHandler> _logger;

        public GetAllStockDataQueryHandler(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<GetAllStockDataQueryHandler> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<StockDataResponse> Handle(GetAllStockDataQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                var symbol = query.Symbol.ToUpper().Trim();

                var cacheKey = $"StockData_{symbol}_{query.StartDate}_{query.EndDate}_{query.IncludeBorrowFee}_{query.IncludeChartExchange}_{query.IncludeFinra}";

                if (_cache.TryGetValue(cacheKey, out StockDataResponse cachedData))
                {
                    _logger.LogInformation("Returning cached stock data for {Symbol}", symbol);
                    return cachedData;
                }

                var response = new StockDataResponse { Symbol = symbol };

                // Fetch borrow fee data
                if (query.IncludeBorrowFee)
                {
                    response.BorrowFeeData = await GetBorrowFeeDataAsync(symbol, query.StartDate, query.EndDate, cancellationToken);
                }

                // Fetch ChartExchange data
                if (query.IncludeChartExchange)
                {
                    response.ChartExchangeData = await GetChartExchangeDataAsync(symbol, query.StartDate, query.EndDate, cancellationToken);
                }

                // Fetch FINRA data
                if (query.IncludeFinra)
                {
                    response.FinraData = await GetFinraDataAsync(symbol, query.StartDate, query.EndDate, cancellationToken);
                }

                // Cache for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                _cache.Set(cacheKey, response, cacheOptions);

                _logger.LogInformation("Retrieved stock data for {Symbol}", symbol);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock data for {Symbol}", query.Symbol);
                throw;
            }
        }

        private async Task<BorrowFeeDataDto[]> GetBorrowFeeDataAsync(string symbol, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var query = _context.BorrowFeeData.Where(d => d.StockTickerSymbol == symbol);

            if (startDate.HasValue)
                query = query.Where(d => d.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(d => d.Date <= endDate.Value.Date);

            return await query
                .OrderBy(d => d.Date)
                .Select(d => new BorrowFeeDataDto
                {
                    Date = d.Date,
                    Fee = d.Fee,
                    AvailableShares = d.AvailableShares
                })
                .ToArrayAsync(cancellationToken);
        }

        private async Task<ChartExchangeDataDto> GetChartExchangeDataAsync(string symbol, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var chartExchangeData = new ChartExchangeDataDto();

            // Price data
            var priceQuery = _context.ChartExchangePrice.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) priceQuery = priceQuery.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) priceQuery = priceQuery.Where(d => d.Date <= endDate.Value.Date);

            chartExchangeData.PriceData = await priceQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangePriceDataDto
                {
                    Date = d.Date,
                    Open = d.Open,
                    High = d.High,
                    Low = d.Low,
                    Close = d.Close,
                    Volume = d.Volume,
                    AdjustedClose = d.AdjustedClose,
                    DividendAmount = d.DividendAmount,
                    SplitCoefficient = d.SplitCoefficient
                })
                .ToArrayAsync(cancellationToken);

            // Failure to deliver data
            var failureToDeliverQuery = _context.ChartExchangeFailureToDeliver.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) failureToDeliverQuery = failureToDeliverQuery.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) failureToDeliverQuery = failureToDeliverQuery.Where(d => d.Date <= endDate.Value.Date);

            chartExchangeData.FailureToDeliverData = await failureToDeliverQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeFailureToDeliverDataDto
                {
                    Date = d.Date,
                    FailureToDeliver = d.FailureToDeliver,
                    Price = d.Price,
                    Volume = d.Volume,
                    SettlementDate = d.SettlementDate,
                    Cusip = d.Cusip,
                    CompanyName = d.CompanyName
                })
                .ToArrayAsync(cancellationToken);

            // Reddit mentions data
            var redditMentionsQuery = _context.ChartExchangeRedditMentions.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) redditMentionsQuery = redditMentionsQuery.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) redditMentionsQuery = redditMentionsQuery.Where(d => d.Date <= endDate.Value.Date);

            chartExchangeData.RedditMentionsData = await redditMentionsQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeRedditMentionsDataDto
                {
                    Date = d.Date,
                    Mentions = d.Mentions,
                    SentimentScore = d.SentimentScore,
                    SentimentLabel = d.SentimentLabel,
                    Subreddit = d.Subreddit,
                    Upvotes = d.Upvotes,
                    Comments = d.Comments,
                    EngagementScore = d.EngagementScore
                })
                .ToArrayAsync(cancellationToken);

            // Option chain data
            var optionChainQuery = _context.ChartExchangeOptionChain.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) optionChainQuery = optionChainQuery.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) optionChainQuery = optionChainQuery.Where(d => d.Date <= endDate.Value.Date);

            chartExchangeData.OptionChainData = await optionChainQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeOptionChainDataDto
                {
                    Date = d.Date,
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
                    Vega = d.Vega
                })
                .ToArrayAsync(cancellationToken);

            // Stock split data
            var stockSplitQuery = _context.ChartExchangeStockSplit.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) stockSplitQuery = stockSplitQuery.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) stockSplitQuery = stockSplitQuery.Where(d => d.Date <= endDate.Value.Date);

            chartExchangeData.StockSplitData = await stockSplitQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeStockSplitDataDto
                {
                    Date = d.Date,
                    SplitRatio = d.SplitRatio,
                    SplitFactor = d.SplitFactor,
                    FromFactor = d.FromFactor,
                    ToFactor = d.ToFactor,
                    ExDate = d.ExDate,
                    RecordDate = d.RecordDate,
                    PayableDate = d.PayableDate,
                    AnnouncementDate = d.AnnouncementDate,
                    CompanyName = d.CompanyName
                })
                .ToArrayAsync(cancellationToken);

            return chartExchangeData;
        }

        private async Task<FinraDataDto> GetFinraDataAsync(string symbol, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var finraData = new FinraDataDto();

            var query = _context.FinraShortInterestData.Where(d => d.StockTickerSymbol == symbol);
            if (startDate.HasValue) query = query.Where(d => d.Date >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(d => d.Date <= endDate.Value.Date);

            finraData.ShortInterestData = await query
                .OrderBy(d => d.Date)
                .Select(d => new FinraShortInterestDto
                {
                    Date = d.Date,
                    ShortInterest = d.ShortInterest,
                    SharesOutstanding = d.SharesOutstanding,
                    ShortInterestPercent = d.ShortInterestPercent,
                    Days2Cover = d.Days2Cover
                })
                .ToArrayAsync(cancellationToken);

            return finraData;
        }
    }
}
