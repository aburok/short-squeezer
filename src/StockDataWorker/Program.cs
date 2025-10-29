using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockData.ChartExchange;
using StockDataLib.Data;
using StockDataLib.Services;
using StockDataWorker.Services;

namespace StockDataWorker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            // Ensure database migrations are applied before starting the worker
            await EnsureDatabaseMigratedAsync(host);
            
            await host.RunAsync();
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
                    
                    // Add HTTP client for ChartExchange
                    services.AddHttpClient("ChartExchange");
                    
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
                    services.AddScoped<ITickerService, TickerService>();
                    services.AddScoped<IFinraService, FinraService>();
                    
                    // Configure options
                    services.Configure<FinraOptions>(configuration.GetSection("Finra"));
                    services.Configure<ChartExchangeOptions>(configuration.GetSection("ChartExchange"));
                    services.Configure<DataFetcherOptions>(configuration.GetSection("DataFetcher"));
                    
                    // Add background service
                    services.AddHostedService<TickerBackgroundService>();
                });

        // Helper method to ensure database migrations are applied
        private static async Task EnsureDatabaseMigratedAsync(IHost host)
        {
            try
            {
                Console.WriteLine("🔄 [Worker] Checking database migrations...");
                
                using var scope = host.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StockDataContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                // Check if database exists
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    logger.LogInformation("📦 [Worker] Database does not exist. Creating database...");
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("✅ [Worker] Database created successfully");
                }
                else
                {
                    logger.LogInformation("📊 [Worker] Database exists. Checking for pending migrations...");
                    
                    // Get pending migrations
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("🔄 [Worker] Found {Count} pending migrations:", pendingMigrations.Count());
                        foreach (var migration in pendingMigrations)
                        {
                            logger.LogInformation("  - {Migration}", migration);
                        }
                        
                        logger.LogInformation("📦 [Worker] Applying migrations...");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("✅ [Worker] Migrations applied successfully");
                    }
                    else
                    {
                        logger.LogInformation("✅ [Worker] Database is up to date - no pending migrations");
                    }
                }
                
                // Verify database connection
                var isHealthy = await context.Database.CanConnectAsync();
                if (isHealthy)
                {
                    logger.LogInformation("✅ [Worker] Database connection verified");
                }
                else
                {
                    logger.LogError("❌ [Worker] Database connection failed");
                }
            }
            catch (Exception ex)
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "❌ [Worker] Error ensuring database migrations: {Message}", ex.Message);
                
                // In development, we might want to continue even if migrations fail
                var environment = host.Services.GetRequiredService<IHostEnvironment>();
                if (environment.IsDevelopment())
                {
                    logger.LogWarning("⚠️  [Worker] Continuing in development mode despite migration error");
                }
                else
                {
                    // In production, we should fail fast
                    throw;
                }
            }
        }
    }
}

// Made with Bob