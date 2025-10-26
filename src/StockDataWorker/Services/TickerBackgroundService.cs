using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockDataLib.Services;

namespace StockDataWorker.Services
{
    public class TickerBackgroundService : BackgroundService
    {
        private readonly ILogger<TickerBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _refreshInterval;
        private readonly TimeSpan _initialDelay;

        public TickerBackgroundService(
            ILogger<TickerBackgroundService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            
            // Get settings from configuration
            var intervalHours = configuration.GetValue<int>("TickerRefreshSettings:IntervalHours", 24);
            var initialDelayMinutes = configuration.GetValue<int>("TickerRefreshSettings:InitialDelayMinutes", 1);
            
            _refreshInterval = TimeSpan.FromHours(intervalHours);
            _initialDelay = TimeSpan.FromMinutes(initialDelayMinutes);
            
            _logger.LogInformation("Ticker Background Service configured with refresh interval of {Hours} hours",
                _refreshInterval.TotalHours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ticker Background Service is starting");

            // Initial delay before first execution
            _logger.LogInformation("Waiting for initial delay of {Minutes} minutes before first execution",
                _initialDelay.TotalMinutes);
            await Task.Delay(_initialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Fetching latest ticker lists from exchanges");
                    
                    // Create a scope to resolve scoped services
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tickerService = scope.ServiceProvider.GetRequiredService<ITickerService>();
                        
                        // Fetch all tickers from exchanges
                        bool success = await tickerService.RefreshAllTickersAsync();
                        
                        if (success)
                        {
                            _logger.LogInformation("Successfully refreshed ticker lists from exchanges");
                        }
                        else
                        {
                            _logger.LogWarning("Failed to refresh ticker lists from exchanges");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching ticker lists");
                }

                // Wait for the next scheduled execution
                _logger.LogInformation("Ticker Background Service is waiting for next scheduled execution in {Hours} hours", 
                    _refreshInterval.TotalHours);
                    
                await Task.Delay(_refreshInterval, stoppingToken);
            }

            _logger.LogInformation("Ticker Background Service is stopping");
        }
    }
}

// Made with Bob
