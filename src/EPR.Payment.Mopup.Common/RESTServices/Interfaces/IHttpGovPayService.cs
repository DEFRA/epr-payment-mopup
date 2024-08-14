using EPR.Payment.Mopup.Common.Dtos.Response;

namespace EPR.Payment.Mopup.Common.RESTServices.Interfaces
{
    public interface IHttpGovPayService
    {
        Task<PaymentStatusResponseDto?> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken = default);
    }
}
