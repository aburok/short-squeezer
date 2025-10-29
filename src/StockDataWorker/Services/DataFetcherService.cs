using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockData.Contracts;
using StockDataLib.Data;
using StockDataLib.Models;
using StockDataLib.Services;

namespace StockDataWorker.Services
{
    public class DataFetcherService
    {
        private readonly ILogger<DataFetcherService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DataFetcherOptions _options;
        private readonly IFinraService _finraService;

        public DataFetcherService(
            ILogger<DataFetcherService> logger,
            IServiceProvider serviceProvider,
            IOptions<DataFetcherOptions> options,
            IFinraService finraService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _finraService = finraService;
        }

        /// <summary>
        /// Fetches all data for all tickers
        /// </summary>
        public async Task FetchAllDataAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting data fetch process at: {time}", DateTimeOffset.Now);

            try
            {
                await FetchDataForAllTickersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data");
            }

            _logger.LogInformation("Data fetch process completed at: {time}", DateTimeOffset.Now);
        }

        private async Task FetchDataForAllTickersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StockDataContext>();

            // Get all tickers from the database
            var tickers = await dbContext.StockTickers.ToListAsync(stoppingToken);

            // If no tickers exist, add the default ones
            if (!tickers.Any())
            {
                _logger.LogInformation("No tickers found in database. Adding default tickers.");
                tickers = await AddDefaultTickersAsync(dbContext, stoppingToken);
            }

            // Process each ticker
            foreach (var ticker in tickers)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                _logger.LogInformation("Fetching data for {Symbol} on {Exchange}", ticker.Symbol, ticker.Exchange);

                try
                {
                    // Fetch FINRA short interest data
                    if (_options.EnableFinraDataFetching)
                    {
                        await FetchFinraShortInterestDataAsync(dbContext, ticker, stoppingToken);
                    }

                    // Update the last updated timestamp
                    ticker.LastUpdated = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching data for {Symbol}", ticker.Symbol);
                }

                // Add a delay between requests to avoid overwhelming the server
                await Task.Delay(TimeSpan.FromSeconds(_options.DelayBetweenRequestsSeconds), stoppingToken);
            }
        }

        private async Task FetchFinraShortInterestDataAsync(StockDataContext dbContext, StockTicker ticker, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Fetching FINRA short interest data for {Symbol}", ticker.Symbol);
            
            try
            {
                // Get data from the last 6 months to ensure we have recent data
                var startDate = DateTime.UtcNow.AddMonths(-6);
                var finraData = await _finraService.GetShortInterestDataAsync(ticker.Symbol, startDate, null);
                
                if (finraData.Any())
                {
                    await UpdateFinraShortInterestDataAsync(dbContext, ticker.Symbol, finraData, stoppingToken);
                    _logger.LogInformation("Successfully updated FINRA short interest data for {Symbol}", ticker.Symbol);
                }
                else
                {
                    _logger.LogWarning("No FINRA short interest data found for {Symbol}", ticker.Symbol);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FINRA short interest data for {Symbol}", ticker.Symbol);
            }
        }

        private async Task<List<StockTicker>> AddDefaultTickersAsync(StockDataContext dbContext, CancellationToken stoppingToken)
        {
            var defaultTickers = new List<StockTicker>
            {
                new StockTicker { Symbol = "GME", Exchange = "nyse", Name = "GameStop Corp.", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "AAPL", Exchange = "nasdaq", Name = "Apple Inc.", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "TSLA", Exchange = "nasdaq", Name = "Tesla, Inc.", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "AMC", Exchange = "nyse", Name = "AMC Entertainment Holdings, Inc.", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "MSFT", Exchange = "nasdaq", Name = "Microsoft Corporation", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "AMZN", Exchange = "nasdaq", Name = "Amazon.com, Inc.", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "GOOGL", Exchange = "nasdaq", Name = "Alphabet Inc.", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "META", Exchange = "nasdaq", Name = "Meta Platforms, Inc.", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "NFLX", Exchange = "nasdaq", Name = "Netflix, Inc.", LastUpdated = DateTime.UtcNow },
                new StockTicker { Symbol = "BYND", Exchange = "nasdaq", Name = "Beyond Meat, Inc.", LastUpdated = DateTime.UtcNow }
            };

            dbContext.StockTickers.AddRange(defaultTickers);
            await dbContext.SaveChangesAsync(stoppingToken);
            
            _logger.LogInformation("Added {Count} default tickers to the database", defaultTickers.Count);
            
            return defaultTickers;
        }

        private async Task UpdateFinraShortInterestDataAsync(
            StockDataContext dbContext,
            string tickerSymbol,
            List<StockDataLib.Services.FinraApiResponseData> newData,
            CancellationToken stoppingToken)
        {
            // Get existing data for this ticker
            var existingData = await dbContext.FinraShortInterestData
                .Where(d => d.StockTickerSymbol == tickerSymbol)
                .ToDictionaryAsync(d => d.SettlementDate.Date, d => d, stoppingToken);

            // Process new data
            foreach (var dataPoint in newData)
            {
                // Normalize the date to remove time component
                var dateKey = dataPoint.SettlementDate.Date;

                // Check if we already have data for this date
                if (existingData.TryGetValue(dateKey, out var existingPoint))
                {
                    // Update existing data if any value is different
                    if (existingPoint.ShortInterest != dataPoint.ShortInterest ||
                        existingPoint.ShortInterestPercent != dataPoint.ShortInterestPercent ||
                        existingPoint.MarketValue != dataPoint.MarketValue ||
                        existingPoint.SharesOutstanding != dataPoint.SharesOutstanding ||
                        existingPoint.AvgDailyVolume != dataPoint.AvgDailyVolume ||
                        existingPoint.Days2Cover != dataPoint.Days2Cover)
                    {
                        existingPoint.ShortInterest = dataPoint.ShortInterest;
                        existingPoint.ShortInterestPercent = dataPoint.ShortInterestPercent;
                        existingPoint.MarketValue = dataPoint.MarketValue;
                        existingPoint.SharesOutstanding = dataPoint.SharesOutstanding;
                        existingPoint.AvgDailyVolume = dataPoint.AvgDailyVolume;
                        existingPoint.Days2Cover = dataPoint.Days2Cover;
                        dbContext.FinraShortInterestData.Update(existingPoint);
                    }
                }
                else
                {
                    // Add new data point
                    var dbItem = new FinraShortInterestData
                    {
                        StockTickerSymbol = tickerSymbol,
                        Date = dataPoint.SettlementDate,
                        SettlementDate = dataPoint.SettlementDate,
                        ShortInterest = dataPoint.ShortInterest,
                        ShortInterestPercent = dataPoint.ShortInterestPercent,
                        MarketValue = dataPoint.MarketValue,
                        SharesOutstanding = dataPoint.SharesOutstanding,
                        AvgDailyVolume = dataPoint.AvgDailyVolume,
                        Days2Cover = dataPoint.Days2Cover
                    };
                    dbContext.FinraShortInterestData.Add(dbItem);
                }
            }

            // Save changes
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }

    public class DataFetcherOptions
    {
        public int FetchIntervalMinutes { get; set; } = 60; // Default to 1 hour
        public int DelayBetweenRequestsSeconds { get; set; } = 5; // Default to 5 seconds
        public bool EnableBorrowFeeDataFetching { get; set; } = true;
        public bool EnablePriceDataFetching { get; set; } = true;
        public bool EnableFinraDataFetching { get; set; } = true;
    }
}

// Made with Bob
