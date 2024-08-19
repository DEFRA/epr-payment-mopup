namespace EPR.Payment.Mopup.Common.RESTServices.Interfaces
{
    public interface IHttpGovPayClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
