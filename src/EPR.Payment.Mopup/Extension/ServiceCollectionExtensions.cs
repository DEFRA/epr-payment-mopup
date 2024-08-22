using EPR.Payment.Mopup.Common.Configuration;
using EPR.Payment.Mopup.Common.Data;
using EPR.Payment.Mopup.Common.Data.Interfaces;
using EPR.Payment.Mopup.Common.Data.Interfaces.Repositories;
using EPR.Payment.Mopup.Common.Data.Repositories;
using EPR.Payment.Mopup.Common.RESTServices;
using EPR.Payment.Mopup.Common.RESTServices.Interfaces;
using EPR.Payment.Mopup.Services;
using EPR.Payment.Mopup.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace EPR.Payment.Mopup.Extension
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IPaymentsRepository, PaymentsRepository>();
            services.Configure<ServicesConfiguration>(configuration.GetSection(ServicesConfiguration.SectionName));
            RegisterHttpService<IHttpGovPayService, HttpGovPayService>(
                services, nameof(ServicesConfiguration.GovPayService));
            services.AddScoped<IPaymentsService, PaymentsService>();
            return services;

        }

        private static void RegisterHttpService<TInterface, TImplementation>(
            IServiceCollection services, string configName, string endPointOverride = null)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            // Perform validation before adding to the service collection
            var serviceOptions = CreateServiceOptions(services, configName, endPointOverride);

            services.AddScoped<TInterface>(s =>
            {
                Trace.TraceInformation($"Registering service {typeof(TImplementation).Name} for {configName}");

                var instance = Activator.CreateInstance(typeof(TImplementation),
                    s.GetRequiredService<IHttpContextAccessor>(),
                    s.GetRequiredService<IHttpClientFactory>(),
                    serviceOptions);

                Trace.TraceError(instance == null ? $"Failed to create instance of {typeof(TImplementation).Name}" : $"Successfully created instance of {typeof(TImplementation).Name}");

                return instance == null
                    ? throw new InvalidOperationException($"Failed to create instance of {typeof(TImplementation).Name}")
                    : (TInterface)(TImplementation)instance;
            });
        }
        private static IOptions<Service> CreateServiceOptions(IServiceCollection services, string configName, string endPointOverride)
        {
            var serviceProvider = services.BuildServiceProvider();
            var servicesConfig = serviceProvider.GetRequiredService<IOptions<ServicesConfiguration>>().Value;

            var serviceConfig = (Service)servicesConfig.GetType().GetProperty(configName)?.GetValue(servicesConfig);

            ValidateServiceConfiguration(serviceConfig, configName);

            var endPointName = endPointOverride ?? serviceConfig?.EndPointName;

            return Options.Create(new Service
            {
                Url = serviceConfig?.Url,
                EndPointName = endPointName,
                BearerToken = serviceConfig?.BearerToken,
                HttpClientName = serviceConfig?.HttpClientName
            });
        }

        private static void ValidateServiceConfiguration(Service serviceConfig, string configName)
        {
            if (serviceConfig?.Url == null)
            {
                throw new InvalidOperationException($"{configName} Url configuration is missing.");
            }

            if (serviceConfig.EndPointName == null)
            {
                throw new InvalidOperationException($"{configName} EndPointName configuration is missing.");
            }
        }

        public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, o => o.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddScoped<IAppDbContext, AppDbContext>(provider => provider.GetService<AppDbContext>()!);
            AddDependencies(services, configuration);
            return services;
        }
    }
}
