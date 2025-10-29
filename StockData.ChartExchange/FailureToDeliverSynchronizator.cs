using System.Globalization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockData.ChartExchange.DataModels;
using StockData.ChartExchange.Models;
using StockDataLib;
using StockDataLib.Data;
using StockDataLib.Models;

namespace StockData.ChartExchange;

[UsedImplicitly]
public class FailureToDeliverSynchronizator(
    StockDataContext context,
    ILogger<FailureToDeliverSynchronizator> logger,
    ChartExchangeDataClient chartExchangeDataClient) :
    ITickerDataSynchronizationService
{
    public async Task Synchronize(string symbol)
    {
        var latestMention = context.ChartExchangeFailureToDeliver.OrderByDescending(m => m.Date)
                                .FirstOrDefault(m => m.StockTickerSymbol == symbol)?.Date
                            ?? DateTimeOffset.UtcNow.AddYears(-1);

        var redditMentionsData = await Fetch(symbol, latestMention.Date);
        if (redditMentionsData.Any())
        {
            var existingDates = await context.ChartExchangeFailureToDeliver
                .Where(d => d.StockTickerSymbol == symbol)
                .Select(d => d.Date.Date)
                .ToListAsync();

            var newRedditMentionsData = redditMentionsData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
            if (newRedditMentionsData.Any())
            {
                context.ChartExchangeFailureToDeliver.AddRange(newRedditMentionsData);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task<ChartExchangeFailureToDeliver[]> Fetch(string symbol, DateTimeOffset until)
    {
        var url = "/data/stocks/failure-to-deliver/";
        var data =
            await chartExchangeDataClient.GetArrayData<FailureToDeliverData, ChartExchangeFailureToDeliver>(
                symbol, url, (d) => new ChartExchangeFailureToDeliver()
                {
                    Date = DateTime.ParseExact(d.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    FailureToDeliver = d.FailureToDeliver,
                    Price = d.Price,
                    Volume = d.Volume,
                    SettlementDate = !string.IsNullOrEmpty(d.SettlementDate)
                        ? DateTime.Parse(d.SettlementDate)
                        : null,
                    Cusip = d.Cusip,
                    CompanyName = d.CompanyName,
                });
        return data;
    }
}