using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockDataLib.Services;

namespace TestShortVolume
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

            logger.LogInformation("Starting short volume data test...");

            try
            {
                // Test fetching short volume data for BYND
                var shortVolumeData = await chartExchangeService.GetShortVolumeDataAsync("BYND", "nasdaq");

                if (shortVolumeData.Count > 0)
                {
                    logger.LogInformation($"Successfully fetched {shortVolumeData.Count} short volume data points for BYND");
                    
                    // Display the first 5 data points
                    int count = 0;
                    foreach (var data in shortVolumeData)
                    {
                        logger.LogInformation($"Date: {data.Date:yyyy-MM-dd}, Short Volume: {data.ShortVolume:N0}, Short Volume %: {data.ShortVolumePercent}%");
                        
                        count++;
                        if (count >= 5) break;
                    }
                }
                else
                {
                    logger.LogWarning("No short volume data found for BYND");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching short volume data");
            }

            logger.LogInformation("Test completed");
        }
    }
}

// Made with Bob
