using AutoMapper;
using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Data.Interfaces.Repositories;
using EPR.Payment.Mopup.Common.Dtos.Request;
using EPR.Payment.Mopup.Common.Dtos.Response;
using EPR.Payment.Mopup.Common.Enums;
using EPR.Payment.Mopup.Common.Exceptions;
using EPR.Payment.Mopup.Common.Mappers;
using EPR.Payment.Mopup.Common.RESTServices.Interfaces;
using EPR.Payment.Mopup.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Payment.Mopup.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IPaymentsRepository _paymentRepository;
        private readonly IHttpGovPayService _httpGovPayService;
        private readonly ILogger<PaymentsService> _logger;
        private readonly IMapper _mapper;
        public PaymentsService(
            IPaymentsRepository paymentRepository,
            IHttpGovPayService httpGovPayService,
            ILogger<PaymentsService> logger,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _httpGovPayService = httpGovPayService ?? throw new ArgumentNullException(nameof(httpGovPayService));
            _logger = logger;
            _mapper = mapper;
        }
        public async Task UpdatePaymentsAsync(CancellationToken cancellationToken = default)
        {
            var payments = await _paymentRepository.GetPaymentsByStatusAsync(Status.InProgress, cancellationToken);
            var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);

            foreach (var paymentDto in paymentDtos)
            {
                if (string.IsNullOrEmpty(paymentDto.GovpayPaymentId))
                {
                    throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
                }
                var paymentStatusResponse = await GetPaymentStatusResponseAsync(paymentDto.GovpayPaymentId, cancellationToken);
                var status = PaymentStatusMapper.GetPaymentStatus(
                            paymentStatusResponse.State?.Status ?? throw new ServiceException(ExceptionMessages.PaymentStatusNotFound),
                            paymentStatusResponse.State?.Code
                            );
                var updateRequest = CreateUpdatePaymentRequest(paymentDto, paymentStatusResponse, status);
                var entity = _mapper.Map<Common.Data.DataModels.Payment>(updateRequest);
                await _paymentRepository.UpdatePaymentStatusAsync(entity, cancellationToken);
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

        private PaymentDto CreateUpdatePaymentRequest(PaymentDto payment, PaymentStatusResponseDto paymentStatusResponse, Status status)
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
