using EPR.Payment.Mopup.Extension;
using EPR.Payment.Mopup.Function;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;


[assembly: FunctionsStartup(typeof(Startup))]
namespace EPR.Payment.Mopup.Function
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
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

        /// <summary>
        /// Configures the application's configuration sources for an Azure Functions app.
        /// This method overrides the default configuration setup to include:
        /// - The current directory as the base path
        /// - User secrets (useful for local development and secure storage of sensitive data)
        /// - Environment variables (for runtime configuration across environments)
        /// 
        /// This setup allows the function app to access configuration values from multiple sources,
        /// supporting both local development and cloud deployment scenarios.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IFunctionsConfigurationBuilder"/> used to customize the configuration pipeline.
        /// </param>

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var builtConfig = builder.ConfigurationBuilder.Build();

            builder.ConfigurationBuilder
               .SetBasePath(Environment.CurrentDirectory)
               .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
               .AddEnvironmentVariables()
               .Build();
        }
    }
}