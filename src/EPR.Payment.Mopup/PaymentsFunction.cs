using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Payment.Mopup
{
    public class PaymentsFunction
    {
        private readonly IPaymentsService _paymentsService;
        private readonly ILogger<PaymentsFunction> _logger;

        public PaymentsFunction(IPaymentsService paymentsService, ILogger<PaymentsFunction> logger)
        {
            _paymentsService = paymentsService;
            _logger = logger;
        }

        [FunctionName("MopUpPaymentsFunction")]
        public async Task Run([TimerTrigger("%FUNCTIONS_TIME_TRIGGER%")] TimerInfo myTimer, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Mop Up time trigger function executed");
                await _paymentsService.UpdatePaymentsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccured);
            }
        }
    }
}
