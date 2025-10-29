/*********************************************************************
 * © Copyright IBM Corp. 2025
 *********************************************************************/


using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace StockDataApi;

public interface IHttpResiliencePolicyFactory
{
    IAsyncPolicy<HttpResponseMessage> Policy { get; }
}
public class HttpResiliencePolicyFactory : IHttpResiliencePolicyFactory
{
    private readonly HttpOptions _resilienceConfig;
    private readonly ILogger<HttpResiliencePolicyFactory> _logger;

    public HttpResiliencePolicyFactory(IOptions<HttpOptions> options, ILogger<HttpResiliencePolicyFactory> logger)
    {
        _resilienceConfig = options.Value;
        _logger = logger;
        Policy = Polly.Policy.WrapAsync(GetPolicies().ToArray());
    }

    public IAsyncPolicy<HttpResponseMessage> Policy { get; }

    private IEnumerable<IAsyncPolicy<HttpResponseMessage>> GetPolicies()
    {
        if (_resilienceConfig.Timeout?.Enabled ?? true)
        {
            yield return GetTimeoutPolicy(_resilienceConfig.Timeout, _logger);
        }

        if (_resilienceConfig.CircuitBreaker?.Enabled ?? false)
        {
            yield return GetCircuitBreakerPolicy(_resilienceConfig.CircuitBreaker, _logger);
        }

        if (_resilienceConfig.Retry?.Enabled ?? true)
        {
            yield return GetRetryPolicy(_resilienceConfig.Retry);
        }
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(RetryConfig? retryConfig = null)
    {
        var retries = retryConfig?.Retries ?? 5;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), retries));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeoutConfig? config, ILogger logger)
    {
        var timeout = config?.Timeout ?? TimeSpan.FromMinutes(2);

        return Polly.Policy.TimeoutAsync<HttpResponseMessage>(timeout, (_, _, _, exception) =>
        {
            logger.LogWarning(exception, "Request timed out");
            return Task.CompletedTask;
        });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(CircuitBreakerConfig? config,
        ILogger logger)
    {
        var exceptionsAllowedBeforeBreaking = config?.ExceptionsAllowedBeforeBreaking ?? 3;

        var durationOfBreak = config?.DurationOfBreak ?? TimeSpan.FromSeconds(10);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking, durationOfBreak,
                (result, duration) => logger.LogWarning(result.Exception, "Circuit breaker opened"),
                () => logger.LogDebug("Circuit breaker reset"));
    }
}

[UsedImplicitly]
public record TimeoutConfig
{
    public bool? Enabled { get; init; }
    public TimeSpan? Timeout { get; init; }
}

[UsedImplicitly]
public record CircuitBreakerConfig
{
    public bool? Enabled { get; init; }
    public int? ExceptionsAllowedBeforeBreaking { get; init; }
    public TimeSpan? DurationOfBreak { get; init; }
}

[UsedImplicitly]
public record RetryConfig
{
    public bool? Enabled { get; init; }
    public int? Retries { get; init; }
}

[UsedImplicitly]
public record HttpOptions
{
    public static string SectionName = nameof(HttpOptions);
    public TimeoutConfig? Timeout { get; init; }
    public CircuitBreakerConfig? CircuitBreaker { get; init; }
    public RetryConfig? Retry { get; init; }
    
    /// <summary>
    /// Allows to log requests and responses send by HTTP Client. 
    /// </summary>
    public bool LogRequestsAndResponses { get; init; }
}
