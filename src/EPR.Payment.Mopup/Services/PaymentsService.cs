using AutoMapper;
using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Dtos.Response;
using EPR.Payment.Mopup.Common.Enums;
using EPR.Payment.Mopup.Common.Exceptions;
using EPR.Payment.Mopup.Common.Mappers;
using EPR.Payment.Mopup.Common.RESTServices.Interfaces;
using EPR.Payment.Mopup.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Payment.Mopup.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IHttpGovPayService _httpGovPayService;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentsService> _logger;
        public PaymentsService(
            IHttpGovPayService httpGovPayService,
            IMapper mapper,
            ILogger<PaymentsService> logger)
        {
            _httpGovPayService = httpGovPayService ?? throw new ArgumentNullException(nameof(httpGovPayService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
        }
        public async Task UpdatePaymentsAsync(IEnumerable<Common.Dtos.Request.Payment> payments, IAsyncCollector<Common.Dtos.Request.Payment> paymentToUpdate, CancellationToken cancellationToken = default)
        {
            foreach (var item in payments)
            {
                var paymentStatusResponse = await GetPaymentStatusResponseAsync(item.GovpayPaymentId, cancellationToken);
                var status = PaymentStatusMapper.GetPaymentStatus(
                            paymentStatusResponse.State?.Status ?? throw new ServiceException(ExceptionMessages.PaymentStatusNotFound),
                            paymentStatusResponse.State?.Code
                            );
                var updateRequest = CreateUpdatePaymentRequest(item, paymentStatusResponse, status);
                await paymentToUpdate.AddAsync(updateRequest);
            }
        }

        private async Task<PaymentStatusResponseDto> GetPaymentStatusResponseAsync(string govPayPaymentId, CancellationToken cancellationToken)
        {
            try
            {
                var paymentStatusResponse = await _httpGovPayService.GetPaymentStatusAsync(govPayPaymentId, cancellationToken);
                if (paymentStatusResponse?.State == null || paymentStatusResponse.State.Status == null)
                {
                    throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
                }
                return paymentStatusResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessages.ErrorRetrievingPaymentStatus);
                throw new ServiceException(ExceptionMessages.ErrorRetrievingPaymentStatus, ex);
            }
        }

        private Common.Dtos.Request.Payment CreateUpdatePaymentRequest(Common.Dtos.Request.Payment payment, PaymentStatusResponseDto paymentStatusResponse, Status status)
        {
            payment.InternalStatusId = status;
            payment.ErrorCode = paymentStatusResponse!.State!.Code;
            payment.ErrorMessage = paymentStatusResponse!.State!.Message;
            payment.GovPayStatus = paymentStatusResponse!.State!.Status;
            payment.UpdatedDate = DateTime.Now;

            return payment;
        }
    }
}
