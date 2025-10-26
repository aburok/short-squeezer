using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockDataLib.Data;
using StockDataLib.Services;
using StockDataWorker.Services;

namespace StockDataWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Add configuration
                    IConfiguration configuration = hostContext.Configuration;
                    
                    // Add logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddDebug();
                    });
                    
                    // Add HTTP client factory
                    services.AddHttpClient();
                    
                    // Add database context
                    services.AddDbContext<StockDataContext>(options =>
                        options.UseSqlServer(
                            configuration.GetConnectionString("DefaultConnection"),
                            sqlOptions => sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null)
                        ));
                    
                    // Add services
                    services.AddScoped<IChartExchangeService, ChartExchangeService>();
                    services.AddScoped<ITickerService, TickerService>();
                    
                    // Add background service
                    services.AddHostedService<TickerBackgroundService>();
                });
    }
}

// Made with Bob
