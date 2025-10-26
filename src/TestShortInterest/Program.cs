using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockDataLib.Services;

namespace TestShortInterest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .AddHttpClient()
                .AddSingleton<IChartExchangeService, ChartExchangeService>()
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();
            var chartExchangeService = serviceProvider.GetService<IChartExchangeService>();

            logger.LogInformation("Starting short interest data test...");

            try
            {
                // Test fetching short interest data for SPY
                var shortInterestData = await chartExchangeService.GetShortInterestDataAsync("SPY");

                if (shortInterestData.Count > 0)
                {
                    logger.LogInformation($"Successfully fetched {shortInterestData.Count} short interest data points for SPY");
                    
                    // Display the first 5 data points
                    int count = 0;
                    foreach (var data in shortInterestData)
                    {
                        logger.LogInformation($"Date: {data.Date:yyyy-MM-dd}, Short Interest: {data.ShortInterest}%, Shares Short: {data.SharesShort:N0}");
                        
                        count++;
                        if (count >= 5) break;
                    }
                }
                else
                {
                    logger.LogWarning("No short interest data found for SPY");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching short interest data");
            }

            logger.LogInformation("Test completed");
        }
    }
}

// Made with Bob
