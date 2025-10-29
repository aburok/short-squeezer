using System.Globalization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockData.ChartExchange.Models;
using StockData.Contracts.ChartExchange;
using StockDataLib;
using StockDataLib.Data;
using StockDataLib.Models;

namespace StockData.ChartExchange;

[UsedImplicitly]
public class BorrowFeeSynchronizator(
    StockDataContext context,
    ILogger<BorrowFeeSynchronizator> logger,
    ChartExchangeDataClient dataClient) :
    ITickerDataSynchronizationService
{
    public async Task Synchronize(string symbol)
    {
        var latest = context.ChartExchangeBorrowFee.OrderByDescending(m => m.Date)
                         .FirstOrDefault(m => m.StockTickerSymbol == symbol)?.Date
                     ?? DateTimeOffset.UtcNow.AddMonths(-1);

        var data = await Fetch(symbol, latest.Date);
        if (data.Any())
        {
            var existingDates = await context.ChartExchangeBorrowFee
                .Where(d => d.StockTickerSymbol == symbol)
                .Select(d => d.Date.Date)
                .ToListAsync();

            var newData = data.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
            if (newData.Any())
            {
                context.ChartExchangeBorrowFee.AddRange(newData);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets borrow fee data for a symbol within a date range
    /// </summary>
    public async Task<BorrowFeeEntity[]> Fetch(string symbol, DateTime endDate)
    {
        var url = "/data/stocks/borrow-fee/ib/";
        var data =
            await dataClient
                .GetPagedDataUntil<BorrowFeeResponse, BorrowFeeData, BorrowFeeEntity>(
                    symbol, url, (d) => new BorrowFeeEntity()
                    {
                        Date = DateTime.ParseExact(d.Timestamp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        Available = d.Available,
                        Fee = decimal.TryParse(d.Fee, out var fee) ? fee : 0,
                        Rebate = decimal.TryParse(d.Rebate, out var rebate) ? rebate : 0,
                    },
                    (borrowFees) => borrowFees.Last().Date < endDate);
        return data;
    }
}