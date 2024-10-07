using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Dtos.Response;
using EPR.Payment.Mopup.Common.Enums;
using EPR.Payment.Mopup.Common.Exceptions;

namespace EPR.Payment.Mopup.Common.Mappers
{
    public static class PaymentStatusMapper
    {
        public static Status GetPaymentStatus(State? state)
        {
            if (state?.Status == null)
            {
                throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
            }

            return state.Status.ToLower() switch
            {
                "success" => HandleSuccessStatus(state.Code),
                "failed" => HandleFailedStatus(state.Code),
                "error" => HandleErrorStatus(state.Code),
                _ => throw new ServiceException(ExceptionMessages.PaymentStatusNotFound),
            };
        }

        private static Status HandleSuccessStatus(string? code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                throw new ServiceException(ExceptionMessages.SuccessStatusWithErrorCode);
            }
            return Status.Success;
        }

        private static Status HandleFailedStatus(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ServiceException(ExceptionMessages.FailedStatusWithoutErrorCode);
            }

            return code switch
            {
                "P0030" => Status.UserCancelled,
                _ => Status.Failed,
            };
        }

        private static Status HandleErrorStatus(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ServiceException(ExceptionMessages.ErrorStatusWithoutErrorCode);
            }
            return Status.Error;
        }
    }
}
