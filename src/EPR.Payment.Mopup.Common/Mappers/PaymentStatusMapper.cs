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
            if (state != null && state.Status != null)
            {
                switch (state.Status.ToLower())
                {
                    case "success":
                        if (!string.IsNullOrEmpty(state.Code))
                        {
                            throw new ServiceException(ExceptionMessages.SuccessStatusWithErrorCode);
                        }
                        return Status.Success;
                    case "failed":
                        if (string.IsNullOrEmpty(state.Code))
                        {
                            throw new ServiceException(ExceptionMessages.FailedStatusWithoutErrorCode);
                        }
                        return state.Code switch
                        {
                            "P0030" => Status.UserCancelled,
                            _ => Status.Failed,
                        };
                    case "error":
                        if (string.IsNullOrEmpty(state.Code))
                        {
                            throw new ServiceException(ExceptionMessages.ErrorStatusWithoutErrorCode);
                        }
                        return Status.Error;
                    default:
                        throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
                }
            }
            throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
        }
    }
}
