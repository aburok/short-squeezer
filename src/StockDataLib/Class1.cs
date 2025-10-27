using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StockDataLib;

public static class ConfigurationExtension
{
    public static void ShowConfiguration(this ConfigurationManager builder, ILogger logger)
    {
        if (Environment.GetEnvironmentVariable("MAIL_MONITOR_SHOW_CONFIG") != "true")
            return;

        var keyPatterns = Environment.GetEnvironmentVariable("MAIL_MONITOR_SHOW_CONFIG_KEY_PATTERNS")?
                              .Split("||", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                          ?? new[] {"Secret", "Password", "ApiKey"};

        var valuePatterns = Environment.GetEnvironmentVariable("MAIL_MONITOR_SHOW_CONFIG_VALUE_PATTERNS")?
                                .Split("||", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            ?? new[] {"(Password=)([^;]*)(;|$)"};

        var maskKeyPatterns = keyPatterns.Select(v => new Regex(v, RegexOptions.IgnoreCase));
        var maskValuePatterns = valuePatterns.Select(v => new Regex(v, RegexOptions.IgnoreCase));

        var configurationView = builder.GetDebugView((context) =>
        {
            if (maskKeyPatterns.Any(r => r.IsMatch(context.Key)))
            {
                return new string('*', context.Value?.Length ?? 0);
            }

            var value = context.Value ?? "";

            return maskValuePatterns.Aggregate(value,
                (current, valuePattern) => valuePattern.Replace(current, "$1***$3"));
        });
        
        logger.LogInformation(configurationView);
    }
}
