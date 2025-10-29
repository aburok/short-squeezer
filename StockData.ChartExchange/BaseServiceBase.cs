using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockData.ChartExchange.Models;
using StockDataLib.Models;
using StockDataLib.Services;

namespace StockData.ChartExchange;

public abstract class ChartExchangeDataClient
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceOptions _options;
    private readonly HttpClient _httpClient;

    protected ChartExchangeDataClient(ILogger logger,
        IHttpClientFactory httpClientFactory,
        IServiceOptions options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _options = options;

        _httpClient = _httpClientFactory.CreateClient("ChartExchange");
    }

    /// <summary>
    /// Fetches paged data until a predicate is satisfied
    /// </summary>
    public async Task<TModel[]> GetPagedDataUntil<TResponse, TItem, TModel>(
        string symbol,
        string relativeUrl,
        Func<TItem, TModel> mapItem,
        Func<TModel[], bool> shouldStop,
        int pageSize = 1000)
        where TResponse : PagedResponse<TItem>
        where TModel : StockDataPoint
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("s");
        var allResults = new List<TModel>();
        int currentPage = 1;

        try
        {
            while (true)
            {
                var pageResponse = await FetchPageAsync<TResponse>(symbol, relativeUrl, currentPage, pageSize);

                if (pageResponse == null)
                {
                    break;
                }

                var items = pageResponse.Results ?? pageResponse.Data ?? [];
                if (items.Count == 0)
                {
                    break;
                }

                var mappedItems = items.Select(mapItem).ToArray();

                // Apply common properties
                foreach (var item in mappedItems)
                {
                    item.ChartExchangeRequestId = timestamp;
                    item.StockTickerSymbol = symbol;
                }

                allResults.AddRange(mappedItems);

                _logger.LogInformation("Fetched page {Page} for {Symbol}: {Count} items (total: {Total})",
                    currentPage, symbol, mappedItems.Length, allResults.Count);

                // Check if we should stop
                if (shouldStop(mappedItems))
                {
                    _logger.LogInformation("Stopping pagination for {Symbol} at page {Page} due to predicate", symbol, currentPage);
                    break;
                }

                // Check if there are more pages
                if (currentPage >= pageResponse.TotalPages)
                {
                    _logger.LogInformation("Reached last page ({TotalPages}) for {Symbol}", pageResponse.TotalPages, symbol);
                    break;
                }

                currentPage++;
            }

            return allResults.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged ChartExchange data for {Symbol}", symbol);
            return allResults.ToArray();
        }
    }

    protected async Task<TModel[]> GetRawData<TResponse, TModel>(string symbol,
        string relativeUrl,
        Func<TResponse, TModel[]> mapResponse
    )
        where TResponse : class
        where TModel : StockDataPoint
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("s");
        var pageResponse = await FetchPageAsync<TResponse>(symbol, relativeUrl, 1);

        if (pageResponse == null)
        {
            return [];
        }

        var responseMapped = mapResponse(pageResponse);
        foreach (var stockDataPoint in responseMapped)
        {
            stockDataPoint.ChartExchangeRequestId = timestamp;
            stockDataPoint.StockTickerSymbol = symbol;
        }

        return responseMapped;
    }

    /// <summary>
    /// Fetches a single page of data from the API
    /// </summary>
    public async Task<TResponse?> FetchPageAsync<TResponse>(string symbol, string relativeUrl, int page, int pageSize = 1000)
        where TResponse : class
    {
        try
        {
            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                _logger.LogError("ChartExchange API key not configured");
                return null;
            }

            var url = $"{_options.BaseUrl}/api/v1{relativeUrl}?symbol={symbol}&api_key={_options.ApiKey}&page_size={pageSize}&page={page}";

            _logger.LogInformation("Fetching ChartExchange data for {Symbol}, page {Page}", symbol, page);
            _logger.LogDebug("Request URL: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<TResponse>(jsonContent);
                return apiResponse;
            }
            else
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error while executing request: {Url}. Response: {Content}", url, jsonContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ChartExchange data for {Symbol}, page {Page}", symbol, page);
            return null;
        }
    }

    protected async Task<TModel[]> GetPagedData<TResponse, TItem, TModel>(string symbol,
        string relativeUrl,
        Func<TItem, TModel> map
    )
        where TResponse : PagedResponse<TItem>
        where TModel : StockDataPoint
    {
        var result = await GetRawData<TResponse, TModel>(symbol, relativeUrl, (response) =>
        {
            if (response?.Data == null && response?.Results == null)
            {
                _logger.LogWarning("No data returned from ChartExchange for {Symbol}", symbol);
                return [];
            }

            var items = response.Results ?? response.Data ?? [];

            _logger.LogInformation("{Url} for symbol {Symbol} returned {Count}.", relativeUrl, symbol,
                items.Count);
            var mappedData = items.Select(map).ToArray();
            return mappedData;
        });
        return result;
    }

    public async Task<TModel[]> GetArrayData<TItem, TModel>(string symbol,
        string relativeUrl,
        Func<TItem, TModel> map
    )
        where TModel : StockDataPoint
    {
        var result = await GetRawData<TItem[], TModel>(symbol, relativeUrl, (response) =>
        {
            if (response.Length == 0)
            {
                _logger.LogWarning("No data returned from ChartExchange for {Symbol}", symbol);
                return [];
            }

            var mappedData = response.Select(map).ToArray();
            _logger.LogInformation("{Url} for symbol {Symbol} returned {Count}.", relativeUrl, symbol,
                response.Length);
            return mappedData;
        });
        return result;
    }
}