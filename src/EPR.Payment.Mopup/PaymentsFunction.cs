using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(EPR.Payment.Mopup.Startup))]
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

        [FunctionName("PaymentsFunction")]
        public async Task Run([TimerTrigger("%FUNCTIONS_TIME_TRIGGER%")] TimerInfo myTimer, CancellationToken cancellationToken)
        {
            try
            {
                await _paymentsService.UpdatePaymentsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccured);
            }
        }
    }
}
