using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockDataLib.Services;
using StockDataWorker.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StockDataWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly WorkerOptions _options;

        public Worker(
            ILogger<Worker> logger,
            IServiceProvider serviceProvider,
            IOptions<WorkerOptions> options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker service is starting at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a scope to resolve scoped services
                    using var scope = _serviceProvider.CreateScope();
                    
                    // Get the data fetcher service
                    var dataFetcherService = scope.ServiceProvider.GetRequiredService<DataFetcherService>();
                    
                    // Run the data fetching process
                    await dataFetcherService.FetchAllDataAsync(stoppingToken);
                    
                    _logger.LogInformation("Data fetching completed successfully at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while fetching data");
                }

                // Wait for the next scheduled run
                _logger.LogInformation("Worker sleeping for {interval} minutes", _options.RunIntervalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(_options.RunIntervalMinutes), stoppingToken);
            }

            _logger.LogInformation("Worker service is stopping");
        }
    }

    public class WorkerOptions
    {
        public int RunIntervalMinutes { get; set; } = 60; // Default to 1 hour
    }
}
