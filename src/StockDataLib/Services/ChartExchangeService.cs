using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockDataLib.Models;

namespace StockDataLib.Services
{
    public interface IChartExchangeService
    {
        Task<ChartExchangeFailureToDeliver[]> GetFailureToDeliverDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        Task<ChartExchangeRedditMentions[]> GetRedditMentionsDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        // Task<ChartExchangeOptionChain[]> GetOptionChainDataAsync(string symbol, DateTime startDate,
        //     DateTime endDate);

        Task<ChartExchangeStockSplit[]> GetStockSplitDataAsync(string symbol, DateTime startDate, DateTime endDate);

        Task<ChartExchangeShortInterest[]> GetShortInterestDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        Task<ChartExchangeShortVolume[]> GetShortVolumeDataAsync(string symbol, DateTime startDate,
            DateTime endDate);

        Task<ChartExchangeBorrowFee[]> GetBorrowFeeDataAsync(string symbol, DateTime startDate,
            DateTime endDate);
    }

    public class ChartExchangeService : BaseServiceBase, IChartExchangeService
    {
        public ChartExchangeService(
            IHttpClientFactory httpClientFactory,
            ILogger<ChartExchangeService> logger,
            IOptions<ChartExchangeOptions> options)
        :base(logger, "ChartExchange", httpClientFactory, options.Value)
        {
        }

        /// <summary>
        /// Gets failure to deliver data for a symbol within a date range
        /// </summary>
        public async Task<ChartExchangeFailureToDeliver[]> GetFailureToDeliverDataAsync(string symbol,
            DateTime startDate, DateTime endDate)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var url = "/data/stocks/failure-to-deliver/";
            var data =
                await GetArrayData<ChartExchangeFailureToDeliverData, ChartExchangeFailureToDeliver>(
                    symbol, url, (d) => new ChartExchangeFailureToDeliver()
                    {
                        StockTickerSymbol = symbol,
                        Date = DateTime.Parse(d.Date),
                        FailureToDeliver = d.FailureToDeliver,
                        Price = d.Price,
                        Volume = d.Volume,
                        SettlementDate = !string.IsNullOrEmpty(d.SettlementDate)
                            ? DateTime.Parse(d.SettlementDate)
                            : null,
                        Cusip = d.Cusip,
                        CompanyName = d.CompanyName,
                        ChartExchangeRequestId = timestamp,
                    });
            return data;
        }

        /// <summary>
        /// Gets Reddit mentions data for a symbol within a date range
        /// </summary>
        public async Task<ChartExchangeRedditMentions[]> GetRedditMentionsDataAsync(string symbol,
            DateTime startDate, DateTime endDate)
        {
            var url = "/data/reddit/mentions/stock/";
            var data =
                await GetPagedData<ChartExchangeRedditMentionsResponse, ChartExchangeRedditMentionsData,
                    ChartExchangeRedditMentions>(
                    symbol, url, (d) => new ChartExchangeRedditMentions()
                    {
                        StockTickerSymbol = symbol,
                        Date = DateTime.Parse(d.Date),
                        Mentions = d.Mentions,
                        SentimentScore = d.SentimentScore,
                        SentimentLabel = d.SentimentLabel,
                        Subreddit = d.Subreddit,
                        Upvotes = d.Upvotes,
                        Comments = d.Comments,
                        EngagementScore = d.EngagementScore,
                    });
            return data;
        }

        // /// <summary>
        // /// Gets option chain data for a symbol within a date range
        // /// </summary>
        // public async Task<ChartExchangeOptionChain[]> GetOptionChainDataAsync(string symbol, DateTime startDate,
        //     DateTime endDate)
        // {
        //     var url = "/data/options/chain-summary/";
        //     var data =
        //         await GetData<ChartExchangeOptionChainResponse, ChartExchangeOptionChainData, ChartExchangeOptionChain>(
        //             symbol, url, (d, response) => new ChartExchangeOptionChain()
        //             {
        //                 StockTickerSymbol = symbol,
        //                 Date = DateTime.Parse(d.Date),
        //                 ExpirationDate = d.ExpirationDate,
        //                 StrikePrice = d.StrikePrice,
        //                 OptionType = d.OptionType,
        //                 Volume = d.Volume,
        //                 OpenInterest = d.OpenInterest,
        //                 Bid = d.Bid,
        //                 Ask = d.Ask,
        //                 LastPrice = d.LastPrice,
        //                 ImpliedVolatility = d.ImpliedVolatility,
        //                 Delta = d.Delta,
        //                 Gamma = d.Gamma,
        //                 Theta = d.Theta,
        //                 Vega = d.Vega,
        //                 ChartExchangeRequestId = response.Timestamp?.ToString()
        //             });
        //     return data;
        // }

        /// <summary>
        /// Gets stock split data for a symbol within a date range
        /// </summary>
        public async Task<ChartExchangeStockSplit[]> GetStockSplitDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            var url = "/data/stocks/splits/";
            var data =
                await GetArrayData<ChartExchangeStockSplitData, ChartExchangeStockSplit>(
                    symbol, url, (d) => new ChartExchangeStockSplit()
                    {
                        StockTickerSymbol = symbol,
                        Date = DateTime.Parse(d.Date),
                        SplitRatio = d.SplitRatio,
                        SplitFactor = d.SplitFactor,
                        FromFactor = d.FromFactor,
                        ToFactor = d.ToFactor,
                        ExDate = !string.IsNullOrEmpty(d.ExDate) ? DateTime.Parse(d.ExDate) : null,
                        RecordDate = !string.IsNullOrEmpty(d.RecordDate) ? DateTime.Parse(d.RecordDate) : null,
                        PayableDate = !string.IsNullOrEmpty(d.PayableDate) ? DateTime.Parse(d.PayableDate) : null,
                        AnnouncementDate = !string.IsNullOrEmpty(d.AnnouncementDate)
                            ? DateTime.Parse(d.AnnouncementDate)
                            : null,
                        CompanyName = d.CompanyName,
                    });
            return data;
        }

        /// <summary>
        /// Gets short interest data for a symbol within a date range
        /// </summary>
        public async Task<ChartExchangeShortInterest[]> GetShortInterestDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            var url = "/data/stocks/short-interest/";
            var data =
                await GetArrayData<ChartExchangeShortInterestData, ChartExchangeShortInterest>(
                    symbol, url, (d) => new ChartExchangeShortInterest()
                    {
                        StockTickerSymbol = symbol,
                        Date = DateTime.Parse(d.Date),
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

        /// <summary>
        /// Gets short volume data for a symbol within a date range
        /// </summary>
        public async Task<ChartExchangeShortVolume[]> GetShortVolumeDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            var url = "/data/stocks/short-volume/";
            var data = await GetArrayData<ChartExchangeShortVolumeData, ChartExchangeShortVolume>(
                symbol, url, (d) => new ChartExchangeShortVolume()
                {
                    StockTickerSymbol = symbol,
                    Date = DateTime.Parse(d.Date),
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
                    ChartExchangeRequestId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                });
            return data;
        }

        /// <summary>
        /// Gets borrow fee data for a symbol within a date range
        /// </summary>
        public async Task<ChartExchangeBorrowFee[]> GetBorrowFeeDataAsync(string symbol, DateTime startDate,
            DateTime endDate)
        {
            var url = "/data/stock/borrow-fee/ib/";
            var data =
                await GetPagedData<ChartExchangeBorrowFeeResponse, ChartExchangeBorrowFeeData, ChartExchangeBorrowFee>(
                    symbol, url, (d) => new ChartExchangeBorrowFee()
                    {
                        StockTickerSymbol = symbol,
                        Date = DateTime.Parse(d.Timestamp),
                        Available = d.Available,
                        Fee = decimal.TryParse(d.Fee, out var fee) ? fee : 0,
                        Rebate = decimal.TryParse(d.Rebate, out var rebate) ? rebate : 0,
                    });
            return data;
        }
    }
}