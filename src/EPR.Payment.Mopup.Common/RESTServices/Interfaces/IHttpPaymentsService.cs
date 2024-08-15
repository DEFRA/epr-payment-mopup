using EPR.Payment.Mopup.Common.Dtos.Request;

namespace EPR.Payment.Mopup.Common.RESTServices.Interfaces
{
    public interface IHttpPaymentsService
    {
        Task UpdatePaymentAsync(Guid id, UpdatePaymentRequestDto paymentStatusUpdateRequest, CancellationToken cancellationToken = default);
    }
}
