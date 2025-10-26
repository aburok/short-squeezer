using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockDataLib.Data;
using StockDataLib.Services;
using StockDataWorker.Services;
using System;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Add database context
        services.AddDbContext<StockDataContext>(options =>
            options.UseSqlServer(
                hostContext.Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        // Add HTTP client factory
        services.AddHttpClient();

        // Register services
        services.AddScoped<IChartExchangeService, ChartExchangeService>();
        services.AddScoped<ITickerService, TickerService>();

        // Configure background service options
        services.Configure<TickerBackgroundServiceOptions>(hostContext.Configuration.GetSection("TickerBackgroundService"));

        // Add background service
        services.AddHostedService<TickerBackgroundService>();

        // Log startup message
        Console.WriteLine("StockDataWorker is starting...");
        Console.WriteLine($"Environment: {hostContext.HostingEnvironment.EnvironmentName}");
    })
    .Build();

await host.RunAsync();

// Made with Bob
