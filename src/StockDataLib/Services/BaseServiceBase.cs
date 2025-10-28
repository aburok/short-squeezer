using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockDataLib.Models;

namespace StockDataLib.Services;

public abstract class BaseServiceBase(ILogger logger, string _serviceName,
        IHttpClientFactory httpClientFactory,
    IServiceOptions options)
{
    public ILogger Logger { get; } = logger;
    public IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;
    public IServiceOptions Option { get; } = options;

    protected async Task<TModel[]> GetRawData<TResponse, TModel>(string symbol,
        string relativeUrl,
        Func<TResponse, TModel[]> mapResponse
    )
        where TModel : StockDataPoint
    {
        var timestamp = DateTimeOffset.UtcNow;
        try
        {
            if (string.IsNullOrEmpty(options.ApiKey))
            {
                logger.LogError("ChartExchange API key not configured");
                return [];
            }

            var client = httpClientFactory.CreateClient("ChartExchange");
            var url =
                $"{options.BaseUrl}/api/v1{relativeUrl}?symbol={symbol}&api_key={options.ApiKey}";

            logger.LogInformation("Fetching ChartExchange short volume data for {Symbol}", symbol);
            logger.LogInformation("Request URL: {Url}", url);

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<TResponse>(jsonContent);

                var responseMapped = mapResponse(apiResponse);
                foreach (var stockDataPoint in responseMapped)
                {
                    stockDataPoint.Date = timestamp;
                }

                return responseMapped;
            }
            else
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Error while executing request: {url} " + jsonContent, url);
            }

            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching ChartExchange short volume data for {Symbol}", symbol);
            return [];
        }
    }

    protected async Task<TModel[]> GetPagedData<TResponse, TItem, TModel>(string symbol,
        string relativeUrl,
        Func<TItem, TModel> map
    )
        where TResponse : ChartExchangePagedResponse<TItem>
        where TModel : StockDataPoint
    {
        var result = await GetRawData<TResponse, TModel>(symbol, relativeUrl, (response) =>
        {
            if (response?.Data == null)
            {
                logger.LogWarning("No short volume data returned from ChartExchange for {Symbol}", symbol);
                return [];
            }

            var shortVolumeData = response.Data.Select<TItem, TModel>(d => map(d)).ToArray();
            return shortVolumeData;
        });
        return result;
    }

    protected async Task<TModel[]> GetArrayData<TItem, TModel>(string symbol,
        string relativeUrl,
        Func<TItem, TModel> map
    )
        where TModel : StockDataPoint
    {
        var result = await GetRawData<TItem[], TModel>(symbol, relativeUrl, (response) =>
        {
            if (response.Length == 0)
            {
                logger.LogWarning("No short volume data returned from ChartExchange for {Symbol}", symbol);
                return [];
            }

            var shortVolumeData = response.Select<TItem, TModel>(d => map(d)).ToArray();
            return shortVolumeData;
        });
        return result;
    }
}