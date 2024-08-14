using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Mopup.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ServicesConfiguration
    {
        public static string SectionName => "Services";

        public Service GovPayService { get; set; } = new Service();
    }

    public class Service
    {
        public string? Url { get; set; }
        public string? EndPointName { get; set; }
        public string? BearerToken { get; set; }
        public string? HttpClientName { get; set; }
    }
}
