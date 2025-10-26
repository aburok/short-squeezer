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
builder.Services.AddControllersWithViews();

// Add database context
builder.Services.AddDbContext<StockDataContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Add HTTP client factory
builder.Services.AddHttpClient();

// Add memory cache
builder.Services.AddMemoryCache();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock Data API v1"));
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

// Map controllers and set default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Made with Bob
