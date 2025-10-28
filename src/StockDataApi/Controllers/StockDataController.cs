using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockDataApi.Handlers.Commands;
using StockDataApi.Handlers.Queries;
using StockDataApi.Models.Commands;
using StockDataApi.Models.Queries;

namespace StockDataApi.Controllers
{
    /// <summary>
    /// Unified controller for all stock data operations
    /// Uses CQRS pattern with query and command handlers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StockDataController : ControllerBase
    {
        private readonly GetAllStockDataQueryHandler _getAllStockDataQueryHandler;
        private readonly FetchChartExchangeDataCommandHandler _fetchChartExchangeDataCommandHandler;
        private readonly ILogger<StockDataController> _logger;

        public StockDataController(
            GetAllStockDataQueryHandler getAllStockDataQueryHandler,
            FetchChartExchangeDataCommandHandler fetchChartExchangeDataCommandHandler,
            ILogger<StockDataController> logger)
        {
            _getAllStockDataQueryHandler = getAllStockDataQueryHandler;
            _fetchChartExchangeDataCommandHandler = fetchChartExchangeDataCommandHandler;
            _logger = logger;
        }

        /// <summary>
        /// Get all stock data for a ticker (unified endpoint)
        /// Uses CQRS query handler - returns ALL data without date filtering
        /// </summary>
        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetAllStockData(
            string symbol,
            [FromQuery] bool includeBorrowFee = true,
            [FromQuery] bool includeChartExchange = true,
            [FromQuery] bool includeFinra = false)
        {
            try
            {
                var query = new GetAllStockDataQuery
                {
                    Symbol = symbol,
                    StartDate = null, // No date filtering - return all data
                    EndDate = null,   // No date filtering - return all data
                    IncludeBorrowFee = includeBorrowFee,
                    IncludeChartExchange = includeChartExchange,
                    IncludeFinra = includeFinra
                };

                var result = await _getAllStockDataQueryHandler.Handle(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock data for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while retrieving the data");
            }
        }

        /// <summary>
        /// Fetch all ChartExchange data for a symbol (price, failure to deliver, Reddit mentions, option chain, stock splits)
        /// Uses CQRS command handler
        /// </summary>
        [HttpPost("{symbol}/fetch-chartexchange")]
        public async Task<IActionResult> FetchChartExchangeData([FromRoute] string symbol)
        {
            try
            {
                var command = new FetchChartExchangeDataCommand { Symbol = symbol };
                var result = await _fetchChartExchangeDataCommandHandler.Handle(command);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        symbol = result.Symbol,
                        prices = result.Prices,
                        failureToDeliver = result.FailureToDeliver,
                        redditMentions = result.RedditMentions,
                        optionChain = result.OptionChain,
                        stockSplits = result.StockSplits
                    });
                }
                else
                {
                    return StatusCode(500, new { success = false, error = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ChartExchange data for {Symbol}", symbol);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
