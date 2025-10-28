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
        private readonly FetchPolygonDataCommandHandler _fetchPolygonDataCommandHandler;
        private readonly ILogger<StockDataController> _logger;

        public StockDataController(
            GetAllStockDataQueryHandler getAllStockDataQueryHandler,
            FetchPolygonDataCommandHandler fetchPolygonDataCommandHandler,
            ILogger<StockDataController> logger)
        {
            _getAllStockDataQueryHandler = getAllStockDataQueryHandler;
            _fetchPolygonDataCommandHandler = fetchPolygonDataCommandHandler;
            _logger = logger;
        }

        /// <summary>
        /// Get all stock data for a ticker (unified endpoint)
        /// Uses CQRS query handler
        /// </summary>
        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetAllStockData(
            string symbol,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeBorrowFee = true,
            [FromQuery] bool includePolygon = true,
            [FromQuery] bool includeFinra = false)
        {
            try
            {
                var query = new GetAllStockDataQuery
                {
                    Symbol = symbol,
                    StartDate = startDate,
                    EndDate = endDate,
                    IncludeBorrowFee = includeBorrowFee,
                    IncludePolygon = includePolygon,
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
        /// Fetch all Polygon data for a symbol (price, short interest, short volume)
        /// Uses CQRS command handler
        /// </summary>
        [HttpPost("{symbol}/fetch-polygon")]
        public async Task<IActionResult> FetchPolygonData([FromRoute] string symbol)
        {
            try
            {
                var command = new FetchPolygonDataCommand { Symbol = symbol };
                var result = await _fetchPolygonDataCommandHandler.Handle(command);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        symbol = result.Symbol,
                        prices = result.Prices,
                        shortInterest = result.ShortInterest,
                        shortVolume = result.ShortVolume
                    });
                }
                else
                {
                    return StatusCode(500, new { success = false, error = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Polygon data for {Symbol}", symbol);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
