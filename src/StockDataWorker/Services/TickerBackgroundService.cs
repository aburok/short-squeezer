using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockDataLib.Services;

namespace StockDataWorker.Services
{
    public class TickerBackgroundService : BackgroundService
    {
        private readonly ILogger<TickerBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TickerBackgroundServiceOptions _options;

        public TickerBackgroundService(
            ILogger<TickerBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptions<TickerBackgroundServiceOptions> options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ticker Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Ticker Background Service is running at: {time}", DateTimeOffset.Now);

                try
                {
                    await RefreshTickersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while refreshing tickers");
                }

                _logger.LogInformation("Ticker Background Service is sleeping for {Interval} hours", _options.RefreshIntervalHours);
                await Task.Delay(TimeSpan.FromHours(_options.RefreshIntervalHours), stoppingToken);
            }

            _logger.LogInformation("Ticker Background Service is stopping.");
        }

        private async Task RefreshTickersAsync()
        {
            _logger.LogInformation("Starting ticker refresh task");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var tickerService = scope.ServiceProvider.GetRequiredService<ITickerService>();

            // Refresh all tickers
            bool success = await tickerService.RefreshAllTickersAsync();

            if (success)
            {
                _logger.LogInformation("Successfully refreshed all tickers");
            }
            else
            {
                _logger.LogWarning("Failed to refresh all tickers");
            }
        }
    }

    public class TickerBackgroundServiceOptions
    {
        public int RefreshIntervalHours { get; set; } = 24; // Default to 24 hours
    }
}

// Made with Bob
