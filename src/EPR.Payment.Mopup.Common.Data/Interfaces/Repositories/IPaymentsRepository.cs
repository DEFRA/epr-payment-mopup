using EPR.Payment.Mopup.Common.Enums;

namespace EPR.Payment.Mopup.Common.Data.Interfaces.Repositories
{
    public interface IPaymentsRepository
    {
        Task<List<DataModels.OnlinePayment>> GetPaymentsByStatusAsync(Status status, CancellationToken cancellationToken);
        Task UpdatePaymentStatusAsync(DataModels.OnlinePayment? entity, CancellationToken cancellationToken);
    }
}
