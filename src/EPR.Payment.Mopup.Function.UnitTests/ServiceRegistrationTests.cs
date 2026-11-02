using EPR.Payment.Mopup.Common.Data;
using EPR.Payment.Mopup.Common.Data.Interfaces.Repositories;
using EPR.Payment.Mopup.Common.RESTServices.Interfaces;
using EPR.Payment.Mopup.Extension;
using EPR.Payment.Mopup.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Payment.Mopup.Function.UnitTests;

[TestClass]
public class ServiceRegistrationTests
{
    [TestMethod]
    public void AddDataContext_ResolvesTimerDependencies_WithSqlServerConfiguration()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SqlConnectionString"] = "Server=localhost;Database=FeesPayment;Integrated Security=true;TrustServerCertificate=true",
            ["Services:GovPayService:Url"] = "https://publicapi.payments.service.gov.uk",
            ["Services:GovPayService:EndPointName"] = "v1",
            ["Services:GovPayService:BearerToken"] = "test-token"
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddDataContext(configuration, configuration["SqlConnectionString"]!);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        using var scope = provider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        Assert.IsNotNull(ActivatorUtilities.CreateInstance<PaymentsFunction>(scopedProvider));
        Assert.IsNotNull(scopedProvider.GetRequiredService<IPaymentsService>());
        Assert.IsNotNull(scopedProvider.GetRequiredService<IPaymentsRepository>());
        Assert.IsNotNull(scopedProvider.GetRequiredService<IHttpGovPayService>());
        var context = scopedProvider.GetRequiredService<AppDbContext>();
        Assert.AreEqual("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
        var connection = new SqlConnectionStringBuilder(context.Database.GetConnectionString());
        Assert.AreEqual("localhost", connection.DataSource);
        Assert.AreEqual("FeesPayment", connection.InitialCatalog);
        Assert.IsTrue(connection.IntegratedSecurity);
        Assert.IsTrue(connection.TrustServerCertificate);
        Assert.AreEqual(300, context.Database.GetCommandTimeout());
        Assert.AreEqual(QueryTrackingBehavior.NoTracking, context.ChangeTracker.QueryTrackingBehavior);
    }
}
