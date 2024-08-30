using EPR.Payment.Mopup.Function;
using EPR.Payment.Mopup.Extension;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.DependencyInjection;


[assembly: FunctionsStartup(typeof(Startup))]
namespace EPR.Payment.Mopup.Function
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Add Application Insights telemetry
            builder.Services.AddApplicationInsightsTelemetry();

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
