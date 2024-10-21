using EPR.Payment.Mopup.Common.Data.DataModels.Lookups;
using Microsoft.EntityFrameworkCore;

namespace EPR.Payment.Mopup.Common.Data.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<PaymentStatus> PaymentStatus { get; }
        DbSet<DataModels.OnlinePayment> Payment { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
