using EPR.Payment.Mopup.Common.Enums;

namespace EPR.Payment.Mopup.Common.Data.Interfaces.Repositories
{
    public interface IPaymentsRepository
    {
        Task<List<DataModels.Payment>> GetPaymentsByStatusAsync(Status status, CancellationToken cancellationToken);
        Task UpdatePaymentStatusAsync(DataModels.Payment? entity, CancellationToken cancellationToken);
    }
}
