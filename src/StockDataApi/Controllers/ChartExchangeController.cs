using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StockDataLib.Data;
using StockDataLib.Models;
using StockDataLib.Services;

namespace StockDataApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartExchangeController : ControllerBase
    {
        private readonly StockDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ChartExchangeController> _logger;
        private readonly IChartExchangeService _chartExchangeService;

        public ChartExchangeController(
            StockDataContext context,
            IMemoryCache cache,
            ILogger<ChartExchangeController> logger,
            IChartExchangeService chartExchangeService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _chartExchangeService = chartExchangeService;
        }

        /// <summary>
        /// Gets all ChartExchange price data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all ChartExchange price data points</returns>
        [HttpGet("price/{symbol}")]
        public async Task<ActionResult<IEnumerable<ChartExchangePriceDto>>> GetPriceData(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                string cacheKey = $"ChartExchangePrice_{symbol}";

                if (_cache.TryGetValue(cacheKey, out List<ChartExchangePriceDto> cachedData))
                {
                    _logger.LogInformation("Returning cached ChartExchange price data for {Symbol}", symbol);
                    return cachedData;
                }

                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                var data = await _context.ChartExchangePrice
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new ChartExchangePriceDto
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
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} ChartExchange price data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ChartExchange price data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets all ChartExchange failure to deliver data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all ChartExchange failure to deliver data points</returns>
        [HttpGet("failure-to-deliver/{symbol}")]
        public async Task<ActionResult<IEnumerable<ChartExchangeFailureToDeliverDto>>> GetFailureToDeliverData(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                string cacheKey = $"ChartExchangeFailureToDeliver_{symbol}";

                if (_cache.TryGetValue(cacheKey, out List<ChartExchangeFailureToDeliverDto> cachedData))
                {
                    _logger.LogInformation("Returning cached ChartExchange failure to deliver data for {Symbol}", symbol);
                    return cachedData;
                }

                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                var data = await _context.ChartExchangeFailureToDeliver
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new ChartExchangeFailureToDeliverDto
                    {
                        Date = d.Date,
                        FailureToDeliver = d.FailureToDeliver,
                        Price = d.Price,
                        Volume = d.Volume,
                        SettlementDate = d.SettlementDate,
                        Cusip = d.Cusip,
                        CompanyName = d.CompanyName
                    })
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} ChartExchange failure to deliver data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ChartExchange failure to deliver data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets all ChartExchange Reddit mentions data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all ChartExchange Reddit mentions data points</returns>
        [HttpGet("reddit-mentions/{symbol}")]
        public async Task<ActionResult<IEnumerable<ChartExchangeRedditMentionsDto>>> GetRedditMentionsData(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                string cacheKey = $"ChartExchangeRedditMentions_{symbol}";

                if (_cache.TryGetValue(cacheKey, out List<ChartExchangeRedditMentionsDto> cachedData))
                {
                    _logger.LogInformation("Returning cached ChartExchange Reddit mentions data for {Symbol}", symbol);
                    return cachedData;
                }

                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                var data = await _context.ChartExchangeRedditMentions
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new ChartExchangeRedditMentionsDto
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
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} ChartExchange Reddit mentions data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ChartExchange Reddit mentions data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets all ChartExchange option chain data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all ChartExchange option chain data points</returns>
        [HttpGet("option-chain/{symbol}")]
        public async Task<ActionResult<IEnumerable<ChartExchangeOptionChainDto>>> GetOptionChainData(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                string cacheKey = $"ChartExchangeOptionChain_{symbol}";

                if (_cache.TryGetValue(cacheKey, out List<ChartExchangeOptionChainDto> cachedData))
                {
                    _logger.LogInformation("Returning cached ChartExchange option chain data for {Symbol}", symbol);
                    return cachedData;
                }

                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                var data = await _context.ChartExchangeOptionChain
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new ChartExchangeOptionChainDto
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
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} ChartExchange option chain data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ChartExchange option chain data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets all ChartExchange stock split data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all ChartExchange stock split data points</returns>
        [HttpGet("stock-splits/{symbol}")]
        public async Task<ActionResult<IEnumerable<ChartExchangeStockSplitDto>>> GetStockSplitData(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                string cacheKey = $"ChartExchangeStockSplit_{symbol}";

                if (_cache.TryGetValue(cacheKey, out List<ChartExchangeStockSplitDto> cachedData))
                {
                    _logger.LogInformation("Returning cached ChartExchange stock split data for {Symbol}", symbol);
                    return cachedData;
                }

                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                var data = await _context.ChartExchangeStockSplit
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new ChartExchangeStockSplitDto
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
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} ChartExchange stock split data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ChartExchange stock split data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets all short interest data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all short interest data points</returns>
        [HttpGet("short-interest/{symbol}")]
        public async Task<ActionResult<IEnumerable<ShortInterestDataDto>>> GetShortInterestData(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                string cacheKey = $"ShortInterest_{symbol}";

                if (_cache.TryGetValue(cacheKey, out List<ShortInterestDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached short interest data for {Symbol}", symbol);
                    return cachedData;
                }

                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                var data = await _context.ShortInterestData
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new ShortInterestDataDto
                    {
                        Date = d.Date,
                        ShortInterest = d.ShortInterest,
                        SharesShort = d.SharesShort
                    })
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} short interest data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving short interest data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets all short volume data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all short volume data points</returns>
        [HttpGet("short-volume/{symbol}")]
        public async Task<ActionResult<IEnumerable<ShortVolumeDataDto>>> GetShortVolumeData(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                string cacheKey = $"ShortVolume_{symbol}";

                if (_cache.TryGetValue(cacheKey, out List<ShortVolumeDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached short volume data for {Symbol}", symbol);
                    return cachedData;
                }

                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                var data = await _context.ShortVolumeData
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new ShortVolumeDataDto
                    {
                        Date = d.Date,
                        ShortVolume = d.ShortVolume,
                        ShortVolumePercent = d.ShortVolumePercent
                    })
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} short volume data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving short volume data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Gets all borrow fee data for a specific ticker
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <returns>A list of all borrow fee data points</returns>
        [HttpGet("borrow-fee/{symbol}")]
        public async Task<ActionResult<IEnumerable<BorrowFeeDataDto>>> GetBorrowFeeData(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                string cacheKey = $"BorrowFee_{symbol}";

                if (_cache.TryGetValue(cacheKey, out List<BorrowFeeDataDto> cachedData))
                {
                    _logger.LogInformation("Returning cached borrow fee data for {Symbol}", symbol);
                    return cachedData;
                }

                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                var data = await _context.BorrowFeeData
                    .Where(d => d.StockTickerSymbol == symbol)
                    .OrderBy(d => d.Date)
                    .Select(d => new BorrowFeeDataDto
                    {
                        Date = d.Date,
                        Fee = d.Fee,
                        AvailableShares = d.AvailableShares
                    })
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(cacheKey, data, cacheOptions);

                _logger.LogInformation("Retrieved {Count} borrow fee data points for {Symbol}", data.Count, symbol);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrow fee data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Fetches ChartExchange data for a symbol and stores it in the database
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g., AAPL)</param>
        /// <param name="years">Number of years of data to fetch (default: 2)</param>
        /// <returns>Result of the fetch operation</returns>
        [HttpPost("{symbol}/fetch")]
        public async Task<ActionResult> FetchChartExchangeData(string symbol, [FromQuery] int years = 2)
        {
            try
            {
                symbol = symbol.ToUpper().Trim();
                _logger.LogInformation("Fetching ChartExchange data for {Symbol} ({Years} years)", symbol, years);

                var startDate = DateTime.Now.AddYears(-years);
                var endDate = DateTime.Now;

                // Check if ticker exists
                var tickerExists = await _context.StockTickers.AnyAsync(t => t.Symbol == symbol);
                if (!tickerExists)
                {
                    _logger.LogWarning("Ticker {Symbol} not found", symbol);
                    return NotFound($"Ticker {symbol} not found");
                }

                int totalRecords = 0;

                // Fetch and store price data
                var priceData = await _chartExchangeService.GetPriceDataAsync(symbol, startDate, endDate);
                if (priceData.Any())
                {
                    var existingDates = await _context.ChartExchangePrice
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync();

                    var newPriceData = priceData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newPriceData.Any())
                    {
                        _context.ChartExchangePrice.AddRange(newPriceData);
                        totalRecords += newPriceData.Count;
                    }
                }

                // Fetch and store failure to deliver data
                var failureToDeliverData = await _chartExchangeService.GetFailureToDeliverDataAsync(symbol, startDate, endDate);
                if (failureToDeliverData.Any())
                {
                    var existingDates = await _context.ChartExchangeFailureToDeliver
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync();

                    var newFailureToDeliverData = failureToDeliverData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newFailureToDeliverData.Any())
                    {
                        _context.ChartExchangeFailureToDeliver.AddRange(newFailureToDeliverData);
                        totalRecords += newFailureToDeliverData.Count;
                    }
                }

                // Fetch and store Reddit mentions data
                var redditMentionsData = await _chartExchangeService.GetRedditMentionsDataAsync(symbol, startDate, endDate);
                if (redditMentionsData.Any())
                {
                    var existingDates = await _context.ChartExchangeRedditMentions
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync();

                    var newRedditMentionsData = redditMentionsData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newRedditMentionsData.Any())
                    {
                        _context.ChartExchangeRedditMentions.AddRange(newRedditMentionsData);
                        totalRecords += newRedditMentionsData.Count;
                    }
                }

                // Fetch and store option chain data
                var optionChainData = await _chartExchangeService.GetOptionChainDataAsync(symbol, startDate, endDate);
                if (optionChainData.Any())
                {
                    var existingDates = await _context.ChartExchangeOptionChain
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync();

                    var newOptionChainData = optionChainData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newOptionChainData.Any())
                    {
                        _context.ChartExchangeOptionChain.AddRange(newOptionChainData);
                        totalRecords += newOptionChainData.Count;
                    }
                }

                // Fetch and store stock split data
                var stockSplitData = await _chartExchangeService.GetStockSplitDataAsync(symbol, startDate, endDate);
                if (stockSplitData.Any())
                {
                    var existingDates = await _context.ChartExchangeStockSplit
                        .Where(d => d.StockTickerSymbol == symbol)
                        .Select(d => d.Date.Date)
                        .ToListAsync();

                    var newStockSplitData = stockSplitData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
                    if (newStockSplitData.Any())
                    {
                        _context.ChartExchangeStockSplit.AddRange(newStockSplitData);
                        totalRecords += newStockSplitData.Count;
                    }
                }

                await _context.SaveChangesAsync();

                // Clear cache for this symbol
                _cache.Remove($"ChartExchangePrice_{symbol}");
                _cache.Remove($"ChartExchangeFailureToDeliver_{symbol}");
                _cache.Remove($"ChartExchangeRedditMentions_{symbol}");
                _cache.Remove($"ChartExchangeOptionChain_{symbol}");
                _cache.Remove($"ChartExchangeStockSplit_{symbol}");

                _logger.LogInformation("Successfully fetched and stored {Count} ChartExchange data points for {Symbol}", totalRecords, symbol);

                return Ok(new
                {
                    success = true,
                    symbol = symbol,
                    count = totalRecords,
                    message = $"Successfully fetched {totalRecords} ChartExchange data points for {symbol}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange data for {Symbol}", symbol);
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }

    // DTOs for ChartExchange data
    public class ChartExchangePriceDto
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal? AdjustedClose { get; set; }
        public decimal? DividendAmount { get; set; }
        public decimal? SplitCoefficient { get; set; }
    }

    public class ChartExchangeFailureToDeliverDto
    {
        public DateTime Date { get; set; }
        public long FailureToDeliver { get; set; }
        public decimal Price { get; set; }
        public long Volume { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string? Cusip { get; set; }
        public string? CompanyName { get; set; }
    }

    public class ChartExchangeRedditMentionsDto
    {
        public DateTime Date { get; set; }
        public int Mentions { get; set; }
        public decimal? SentimentScore { get; set; }
        public string? SentimentLabel { get; set; }
        public string? Subreddit { get; set; }
        public int? Upvotes { get; set; }
        public int? Comments { get; set; }
        public decimal? EngagementScore { get; set; }
    }

    public class ChartExchangeOptionChainDto
    {
        public DateTime Date { get; set; }
        public string ExpirationDate { get; set; } = string.Empty;
        public decimal StrikePrice { get; set; }
        public string OptionType { get; set; } = string.Empty;
        public long Volume { get; set; }
        public long OpenInterest { get; set; }
        public decimal? Bid { get; set; }
        public decimal? Ask { get; set; }
        public decimal? LastPrice { get; set; }
        public decimal? ImpliedVolatility { get; set; }
        public decimal? Delta { get; set; }
        public decimal? Gamma { get; set; }
        public decimal? Theta { get; set; }
        public decimal? Vega { get; set; }
    }

    public class ChartExchangeStockSplitDto
    {
        public DateTime Date { get; set; }
        public string SplitRatio { get; set; } = string.Empty;
        public decimal SplitFactor { get; set; }
        public decimal FromFactor { get; set; }
        public decimal ToFactor { get; set; }
        public DateTime? ExDate { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? PayableDate { get; set; }
        public DateTime? AnnouncementDate { get; set; }
        public string? CompanyName { get; set; }
    }

    // Additional DTOs for short interest, short volume, and borrow fee data
    public class ShortInterestDataDto
    {
        public DateTime Date { get; set; }
        public decimal ShortInterest { get; set; }
        public long SharesShort { get; set; }
    }

    public class ShortVolumeDataDto
    {
        public DateTime Date { get; set; }
        public long ShortVolume { get; set; }
        public decimal ShortVolumePercent { get; set; }
    }

    public class BorrowFeeDataDto
    {
        public DateTime Date { get; set; }
        public decimal Fee { get; set; }
        public decimal? AvailableShares { get; set; }
    }
}
