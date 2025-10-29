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
public class RedditMentionsSynchronizator(
    StockDataContext context,
    ILogger<RedditMentionsSynchronizator> logger,
    ChartExchangeDataClient chartExchangeDataClient) :
    ITickerDataSynchronizationService
{
    public async Task Synchronize(string symbol)
    {
        var latestMention = context.ChartExchangeRedditMentions.OrderByDescending(m => m.Date)
                                .FirstOrDefault(m => m.StockTickerSymbol == symbol)?.Date
                            ?? DateTimeOffset.UtcNow.AddMonths(-1);

        var redditMentionsData = await Fetch(symbol, latestMention.Date);
        if (redditMentionsData.Any())
        {
            var existingDates = await context.ChartExchangeRedditMentions
                .Where(d => d.StockTickerSymbol == symbol)
                .Select(d => d.Date.Date)
                .ToListAsync();

            var newRedditMentionsData = redditMentionsData.Where(d => !existingDates.Contains(d.Date.Date)).ToList();
            if (newRedditMentionsData.Any())
            {
                context.ChartExchangeRedditMentions.AddRange(newRedditMentionsData);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task<ChartExchangeRedditMentions[]> Fetch(string symbol, DateTimeOffset until)
    {
        var url = "/data/reddit/mentions/stock/";
        var data =
            await chartExchangeDataClient
                .GetPagedDataUntil<RedditMentionsResponse, RedditMentionsData,
                    ChartExchangeRedditMentions>(
                    symbol, url, (d) => new ChartExchangeRedditMentions()
                    {
                        Date = DateTimeOffset.UtcNow,
                        Subreddit = d.Subreddit,
                        Created = DateTime.ParseExact(d.Created, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        Sentiment = d.Sentiment,
                        Author = d.Author,
                        Text = d.Text,
                        Link = d.Link,
                        ThingId = d.ThingId,
                        ThingType = d.ThingType,
                    },
                    (mentions) => mentions.Last().Date < until.Date);
        return data;
    }
}
/// <summary>
/// Represents the Reddit mentions API response from ChartExchange
/// </summary>
public class RedditMentionsResponse : PagedResponse<RedditMentionsData>
{

}