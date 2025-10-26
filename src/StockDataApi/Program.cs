using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StockDataLib.Data;
using StockDataLib.Services;
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all IP addresses
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80);  // Listen for HTTP traffic on port 80
    serverOptions.ListenAnyIP(443); // Listen for HTTPS traffic on port 443
});

// Add services to the container.
builder.Services.AddControllers();

// Add database context
builder.Services.AddDbContext<StockDataContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure().MigrationsAssembly("StockDataLib")));

// Add HTTP client factory
builder.Services.AddHttpClient();

// Add memory cache
builder.Services.AddMemoryCache();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register services
builder.Services.AddScoped<IChartExchangeService, ChartExchangeService>();
builder.Services.AddScoped<IAlphaVantageService, AlphaVantageService>();
builder.Services.AddScoped<ITickerService, TickerService>();

// Configure Alpha Vantage options
builder.Services.Configure<AlphaVantageOptions>(
    builder.Configuration.GetSection("AlphaVantage"));

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Stock Data API",
        Version = "v1",
        Description = "API for accessing stock data including borrow fees from Chart Exchange",
        Contact = new OpenApiContact
        {
            Name = "Stock Data Team"
        }
    });

    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Ensure database migrations are applied
await EnsureDatabaseMigratedAsync(app);

// Display comprehensive configuration information for debugging
Console.WriteLine("=== STOCK DATA API CONFIGURATION DEBUG ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Application Name: {app.Environment.ApplicationName}");
Console.WriteLine($"Content Root: {app.Environment.ContentRootPath}");
Console.WriteLine($"Web Root: {app.Environment.WebRootPath}");
Console.WriteLine();

// Display appsettings.json configuration
Console.WriteLine("📄 APPSETTINGS.JSON CONFIGURATION:");
DisplayConfigurationSection(builder.Configuration, "Logging");
DisplayConfigurationSection(builder.Configuration, "AllowedHosts");
DisplayConfigurationSection(builder.Configuration, "AlphaVantage");
Console.WriteLine();

// Display connection strings
Console.WriteLine("🗄️ DATABASE CONFIGURATION:");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    var maskedConnectionString = MaskConnectionString(connectionString);
    Console.WriteLine($"  Default Connection: {maskedConnectionString}");
}
else
{
    Console.WriteLine("  ❌ Default Connection: NOT CONFIGURED");
}
Console.WriteLine();

// Display environment variables
Console.WriteLine("🌍 ENVIRONMENT VARIABLES:");
DisplayEnvironmentVariables();
Console.WriteLine();

// Display configuration sources
Console.WriteLine("📋 CONFIGURATION SOURCES:");
DisplayConfigurationSources(builder.Configuration);
Console.WriteLine();

// Display URLs
Console.WriteLine("🌐 SERVER URLS:");
Console.WriteLine($"  HTTP: http://localhost:80");
Console.WriteLine($"  HTTPS: https://localhost:443");
Console.WriteLine($"  Swagger: http://localhost:80/swagger");
Console.WriteLine();

// Display registered services
Console.WriteLine("⚙️ REGISTERED SERVICES:");
Console.WriteLine("  ✅ Controllers with Views");
Console.WriteLine("  ✅ Entity Framework (SQL Server)");
Console.WriteLine("  ✅ HTTP Client Factory");
Console.WriteLine("  ✅ Memory Cache");
Console.WriteLine("  ✅ CORS (permissive)");
Console.WriteLine("  ✅ Chart Exchange Service");
Console.WriteLine("  ✅ Alpha Vantage Service");
Console.WriteLine("  ✅ Ticker Service");
Console.WriteLine("  ✅ Swagger/OpenAPI");
Console.WriteLine();

Console.WriteLine("=== CONFIGURATION DEBUG COMPLETE ===");
Console.WriteLine();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock Data API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Add startup logging
Console.WriteLine("🚀 Starting Stock Data API...");
Console.WriteLine($"⏰ Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine("🌐 Application is ready to accept requests!");
Console.WriteLine();

// Add shutdown handler
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine();
    Console.WriteLine("🛑 Stock Data API is shutting down...");
    Console.WriteLine($"⏰ Stopped at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
});

app.Run();

// Helper method to ensure database migrations are applied
static async Task EnsureDatabaseMigratedAsync(WebApplication app)
{
    Console.WriteLine("=== Checking database migrations ===");
    try
    {
        Console.WriteLine("🔄 Checking database migrations...");
        
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StockDataContext>();
        
        // Check if database exists
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            Console.WriteLine("📦 Database does not exist. Creating database...");
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Database created successfully");
        }
        else
        {
            Console.WriteLine("📊 Database exists. Checking for pending migrations...");
            
            // Get pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                Console.WriteLine($"🔄 Found {pendingMigrations.Count()} pending migrations:");
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"  - {migration}");
                }
                
                Console.WriteLine("📦 Applying migrations...");
                await context.Database.MigrateAsync();
                Console.WriteLine("✅ Migrations applied successfully");
            }
            else
            {
                Console.WriteLine("✅ Database is up to date - no pending migrations");
            }
        }
        
        // Verify database connection
        var isHealthy = await context.Database.CanConnectAsync();
        if (isHealthy)
        {
            Console.WriteLine("✅ Database connection verified");
        }
        else
        {
            Console.WriteLine("❌ Database connection failed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error ensuring database migrations: {ex.Message}");
        Console.WriteLine($"   Details: {ex.InnerException?.Message ?? ex.Message}");
        
        // In development, we might want to continue even if migrations fail
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("⚠️  Continuing in development mode despite migration error");
        }
        else
        {
            // In production, we should fail fast
            throw;
        }
    }
}

// Helper methods for configuration debugging
static void DisplayConfigurationSection(IConfiguration configuration, string sectionName)
{
    var section = configuration.GetSection(sectionName);
    if (section.Exists())
    {
        Console.WriteLine($"  {sectionName}:");
        foreach (var item in section.GetChildren())
        {
            if (item.Value != null)
            {
                var maskedValue = MaskSensitiveValue(sectionName, item.Key, item.Value);
                Console.WriteLine($"    {item.Key}: {maskedValue}");
            }
            else
            {
                Console.WriteLine($"    {item.Key}:");
                foreach (var subItem in item.GetChildren())
                {
                    var maskedValue = MaskSensitiveValue(sectionName, subItem.Key, subItem.Value);
                    Console.WriteLine($"      {subItem.Key}: {maskedValue}");
                }
            }
        }
    }
    else
    {
        Console.WriteLine($"  {sectionName}: NOT CONFIGURED");
    }
}

static void DisplayEnvironmentVariables()
{
    var relevantEnvVars = new[]
    {
        "ASPNETCORE_ENVIRONMENT",
        "ASPNETCORE_URLS",
        "DOTNET_ENVIRONMENT",
        "ConnectionStrings__DefaultConnection",
        "AlphaVantage__ApiKey",
        "ASPNETCORE_ENVIRONMENT",
        "ASPNETCORE_CONTENTROOT",
        "ASPNETCORE_WEBROOT"
    };

    foreach (var envVar in relevantEnvVars)
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrEmpty(value))
        {
            var maskedValue = MaskSensitiveValue("Environment", envVar, value);
            Console.WriteLine($"  {envVar}: {maskedValue}");
        }
    }

    // Show any custom environment variables that start with our app prefix
    var customEnvVars = Environment.GetEnvironmentVariables()
        .Cast<System.Collections.DictionaryEntry>()
        .Where(e => e.Key.ToString().StartsWith("STOCKDATA_", StringComparison.OrdinalIgnoreCase))
        .Take(5); // Limit to first 5 custom vars

    foreach (var envVar in customEnvVars)
    {
        var maskedValue = MaskSensitiveValue("Environment", envVar.Key.ToString(), envVar.Value.ToString());
        Console.WriteLine($"  {envVar.Key}: {maskedValue}");
    }
}

static void DisplayConfigurationSources(IConfiguration configuration)
{
    // Get configuration sources through reflection (since they're not directly exposed)
    try
    {
        var configRoot = configuration as IConfigurationRoot;
        if (configRoot != null)
        {
            var sourcesField = typeof(ConfigurationRoot).GetField("_sources", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (sourcesField != null)
            {
                var sources = sourcesField.GetValue(configRoot) as System.Collections.Generic.IList<IConfigurationSource>;
                if (sources != null)
                {
                    foreach (var source in sources)
                    {
                        Console.WriteLine($"  {source.GetType().Name}");
                    }
                }
            }
        }
    }
    catch
    {
        Console.WriteLine("  Configuration sources: Unable to determine");
    }
}

static string MaskSensitiveValue(string section, string key, string value)
{
    if (string.IsNullOrEmpty(value))
        return "NOT SET";

    // Mask sensitive values
    var sensitiveKeys = new[] { "password", "pwd", "secret", "key", "token", "connectionstring" };
    var isSensitive = sensitiveKeys.Any(sk => 
        key.Contains(sk, StringComparison.OrdinalIgnoreCase) || 
        section.Contains(sk, StringComparison.OrdinalIgnoreCase));

    if (isSensitive)
    {
        if (value.Length <= 4)
            return "***";
        return $"***{value.Substring(value.Length - 4)}";
    }

    return value;
}

// Helper method to mask sensitive connection string information
static string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString))
        return "NOT CONFIGURED";
    
    // Mask password and other sensitive information
    var masked = connectionString;
    
    // Mask password
    if (masked.Contains("Password="))
    {
        var passwordIndex = masked.IndexOf("Password=");
        var endIndex = masked.IndexOf(";", passwordIndex);
        if (endIndex == -1) endIndex = masked.Length;
        
        var passwordValue = masked.Substring(passwordIndex + 9, endIndex - passwordIndex - 9);
        masked = masked.Replace($"Password={passwordValue}", "Password=***");
    }
    
    // Mask user ID
    if (masked.Contains("User Id="))
    {
        var userIndex = masked.IndexOf("User Id=");
        var endIndex = masked.IndexOf(";", userIndex);
        if (endIndex == -1) endIndex = masked.Length;
        
        var userValue = masked.Substring(userIndex + 8, endIndex - userIndex - 8);
        masked = masked.Replace($"User Id={userValue}", "User Id=***");
    }
    
    return masked;
}

// Made with Bob
