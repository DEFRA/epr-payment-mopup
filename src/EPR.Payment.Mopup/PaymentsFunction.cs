using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using EPR.Payment.Mopup.Common.Dtos.Request;
using System.Threading;
using System.Collections.Generic;
using System.Data;
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
        public async Task Run([Sql("Select * from Payment where InternalStatusId = 1", commandType: CommandType.Text, connectionStringSetting: "SqlConnectionString")] IEnumerable<Common.Dtos.Request.Payment> payments,
                        [Sql("Payment", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Common.Dtos.Request.Payment> paymentToUpdate,
                        [TimerTrigger("%FUNCTIONS_TIME_TRIGGER%")] TimerInfo myTimer, CancellationToken cancellationToken)
        {
            try
            {
                await _paymentsService.UpdatePaymentsAsync(payments, paymentToUpdate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ErrorOccured);
            }
        }
    }
}
