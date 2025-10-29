using System;
using System.Globalization;
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

                var cacheKey = $"StockData_{symbol}_{query.IncludeChartExchange}_{query.IncludeFinra}";

                if (_cache.TryGetValue(cacheKey, out StockDataResponse cachedData))
                {
                    _logger.LogInformation("Returning cached stock data for {Symbol}", symbol);
                    return cachedData;
                }

                var response = new StockDataResponse { Symbol = symbol };

                // Fetch ChartExchange data
                if (query.IncludeChartExchange)
                {
                    response.ChartExchangeData = await GetChartExchangeDataAsync(symbol, cancellationToken);
                }

                // Fetch FINRA data
                if (query.IncludeFinra)
                {
                    response.FinraData = await GetFinraDataAsync(symbol, cancellationToken);
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

        private async Task<ChartExchangeDataDto> GetChartExchangeDataAsync(string symbol, CancellationToken cancellationToken)
        {
            var chartExchangeData = new ChartExchangeDataDto();

            // Failure to deliver data
            var failureToDeliverQuery = _context.ChartExchangeFailureToDeliver.Where(d => d.StockTickerSymbol == symbol);

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

            chartExchangeData.RedditMentionsData = await redditMentionsQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeRedditMentionsDataDto
                {
                    Subreddit = d.Subreddit,
                    Created = d.Created,
                    Sentiment = d.Sentiment,
                    Author = d.Author,
                    Text = d.Text,
                    Link = d.Link,
                    ThingId = d.ThingId,
                    ThingType = d.ThingType,
                })
                .ToArrayAsync(cancellationToken);

            // Option chain data
            var optionChainQuery = _context.ChartExchangeOptionChain.Where(d => d.StockTickerSymbol == symbol);

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

            // Short interest data
            var shortInterestQuery = _context.ChartExchangeShortInterest.Where(d => d.StockTickerSymbol == symbol);

            chartExchangeData.ShortInterestData = await shortInterestQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeShortInterestDataDto
                {
                    Date = d.Date,
                    ShortInterestPercent = d.ShortInterestPercent,
                    ShortPosition = d.ShortPosition,
                    DaysToCover = d.DaysToCover,
                    ChangeNumber = d.ChangeNumber,
                    ChangePercent = d.ChangePercent
                })
                .ToArrayAsync(cancellationToken);

            // Short volume data
            var shortVolumeQuery = _context.ChartExchangeShortVolume.Where(d => d.StockTickerSymbol == symbol);

            chartExchangeData.ShortVolumeData = await shortVolumeQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeShortVolumeDataDto
                {
                    Date = d.Date,
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
                    ShortVolumePercent = d.ShortVolumePercent
                })
                .ToArrayAsync(cancellationToken);

            // Borrow fee data
            var borrowFeeQuery = _context.ChartExchangeBorrowFee.Where(d => d.StockTickerSymbol == symbol);

            chartExchangeData.BorrowFeeData = await borrowFeeQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeBorrowFeeDataDto
                {
                    Date = d.Date,
                    Available = d.Available,
                    Fee = d.Fee,
                    Rebate = d.Rebate
                })
                .ToArrayAsync(cancellationToken);

            // Daily aggregated borrow fee data (OHLC)
            var borrowFeeDailyQuery = _context.ChartExchangeBorrowFeeDaily.Where(d => d.StockTickerSymbol == symbol);

            chartExchangeData.BorrowFeeDailyData = await borrowFeeDailyQuery
                .OrderBy(d => d.Date)
                .Select(d => new ChartExchangeBorrowFeeDailyDataDto
                {
                    Date = d.Date,
                    Open = d.Open,
                    High = d.High,
                    Low = d.Low,
                    Close = d.Close,
                    Average = d.Average,
                    DataPointCount = d.DataPointCount,
                    MaxAvailable = d.MaxAvailable,
                    MinAvailable = d.MinAvailable,
                    AverageAvailable = d.AverageAvailable
                })
                .ToArrayAsync(cancellationToken);

            return chartExchangeData;
        }

        private async Task<FinraDataDto> GetFinraDataAsync(string symbol, CancellationToken cancellationToken)
        {
            var finraData = new FinraDataDto();

            var query = _context.FinraShortInterestData.Where(d => d.StockTickerSymbol == symbol);

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
