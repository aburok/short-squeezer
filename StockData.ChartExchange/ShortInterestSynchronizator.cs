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
public class ShortInterestSynchronizator(
    StockDataContext context,
    ILogger<RedditMentionsSynchronizator> logger,
    ChartExchangeDataClient dataClient) :
    ITickerDataSynchronizationService
{
    public async Task Synchronize(string symbol)
    {
        var data = await Fetch(symbol);
        if (data.Any())
        {
            var existingDates = await context.ChartExchangeShortInterest
                .Where(d => d.StockTickerSymbol == symbol)
                .Select(d => d.Date.Date)
                .ToListAsync();

            var newData = data.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
            if (newData.Any())
            {
                context.ChartExchangeShortInterest.AddRange(newData);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets short interest data for a symbol within a date range
    /// </summary>
    public async Task<ChartExchangeShortInterest[]> Fetch(string symbol)
    {
        var url = "/data/stocks/short-interest/";
        var data =
            await dataClient.GetArrayData<ShortInterestData, ChartExchangeShortInterest>(
                symbol, url, (d) => new ChartExchangeShortInterest()
                {
                    Date = DateTime.ParseExact(d.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    ShortInterestPercent = decimal.TryParse(d.ShortInterest, out var shortInterestPercent)
                        ? shortInterestPercent
                        : 0,
                    ShortPosition = d.ShortPosition,
                    DaysToCover = decimal.TryParse(d.DaysToCover, out var daysToCover) ? daysToCover : 0,
                    ChangeNumber = d.ChangeNumber,
                    ChangePercent = decimal.TryParse(d.ChangePercent, out var changePercent) ? changePercent : 0,
                });
        return data;
    }
}