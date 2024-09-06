using EPR.Payment.Mopup.Extension;
using EPR.Payment.Mopup.Function;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;


[assembly: FunctionsStartup(typeof(Startup))]
namespace EPR.Payment.Mopup.Function
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                options.EnableAdaptiveSampling = false; // Disable adaptive sampling
                options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
            });

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddApplicationInsights();
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);
            });

            var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"local.settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"local.settings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            builder.Services.AddDataContext(config, config["SqlConnectionString"]);
        }
    }
}
