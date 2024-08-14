using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Payment.Mopup.Services.Interfaces
{
    public interface IPaymentsService
    {
        Task UpdatePaymentsAsync(IEnumerable<Common.Dtos.Request.Payment> payments, IAsyncCollector<Common.Dtos.Request.Payment> paymentToUpdate, CancellationToken cancellationToken = default);
    }
}
