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
public class ShortVolumeSynchronizator(
    StockDataContext context,
    ILogger<ShortVolumeSynchronizator> logger,
    ChartExchangeDataClient dataClient) :
    ITickerDataSynchronizationService
{
    public async Task Synchronize(string symbol)
    {
        var until = context.ChartExchangeShortVolume.OrderByDescending(m => m.Date)
                        .FirstOrDefault(m => m.StockTickerSymbol == symbol)?.Date
                    ?? DateTimeOffset.UtcNow.AddYears(-1);
        
        var data = await Fetch(symbol, until);
        if (data.Any())
        {
            var existingDates = await context.ChartExchangeShortInterest
                .Where(d => d.StockTickerSymbol == symbol)
                .Select(d => d.Date.Date)
                .ToListAsync();

            var newData = data.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
            if (newData.Any())
            {
                context.ChartExchangeShortVolume.AddRange(newData);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets short volume data for a symbol within a date range
    /// </summary>
    public async Task<ChartExchangeShortVolume[]> Fetch(string symbol, DateTimeOffset until)
    {
        var url = "/data/stocks/short-volume/";
        var data = await dataClient.GetPagedDataUntil<ShortVolumeResponse,ShortVolumeData, ChartExchangeShortVolume>(
            symbol, url, (d) => new ChartExchangeShortVolume()
            {
                Date = DateTime.ParseExact(d.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Rt = d.Rt,
                St = d.St,
                Lt = d.Lt,
                Fs = d.Fs,
                Fse = d.Fse,
                Xnas = d.Xnas,
                Xphl = d.Xphl,
                Xnys = d.Xnys,
                Arcx = d.Arcx,
                Xcis = d.Xcis,
                Xase = d.Xase,
                Xchi = d.Xchi,
                Edgx = d.Edgx,
                Bats = d.Bats,
                Edga = d.Edga,
                Baty = d.Baty,
                ShortVolumePercent = d.Rt > 0 ? (decimal) d.St / d.Rt * 100 : 0,
            },
            (volumes) => volumes.Last().Date < until.Date);
        return data;
    }
}