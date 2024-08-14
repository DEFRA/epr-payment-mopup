using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Enums;
using EPR.Payment.Mopup.Common.Exceptions;

namespace EPR.Payment.Mopup.Common.Mappers
{
    public static class PaymentStatusMapper
    {
        public static Status GetPaymentStatus(string status, string? errorCode)
        {
            switch (status.ToLower())
            {
                case "success":
                    if (!string.IsNullOrEmpty(errorCode))
                    {
                        throw new ServiceException(ExceptionMessages.SuccessStatusWithErrorCode);
                    }
                    return Status.Success;
                case "failed":
                    if (string.IsNullOrEmpty(errorCode))
                    {
                        throw new ServiceException(ExceptionMessages.FailedStatusWithoutErrorCode);
                    }
                    return errorCode switch
                    {
                        "P0030" => Status.UserCancelled,
                        _ => Status.Failed,
                    };
                case "error":
                    if (string.IsNullOrEmpty(errorCode))
                    {
                        throw new ServiceException(ExceptionMessages.ErrorStatusWithoutErrorCode);
                    }
                    return Status.Error;
                default:
                    throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
            }
        }
    }
}
