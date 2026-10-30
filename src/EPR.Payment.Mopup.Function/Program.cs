using EPR.Payment.Mopup.Extension;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Mopup.Function;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureAppConfiguration((context, configuration) =>
            {
                var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development";
                configuration
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"local.settings.{environment}.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.Configure<LoggerFilterOptions>(options =>
                {
                    // The Application Insights SDK defaults to Warning; retain application Information logs.
                    var defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName ==
                        "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                    if (defaultRule != null)
                    {
                        options.Rules.Remove(defaultRule);
                    }
                });
                services.AddDataContext(context.Configuration, context.Configuration["SqlConnectionString"]);
            })
            .Build();

        host.Run();
    }
}
