using System.Threading;
using System.Threading.Tasks;

namespace EPR.Payment.Mopup.Services.Interfaces
{
    public interface IPaymentsService
    {
        Task UpdatePaymentsAsync(CancellationToken cancellationToken = default);
    }
}
