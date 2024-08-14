namespace EPR.Payment.Mopup.Common.Constants
{
    public static class ExceptionMessages
    {
        // HttpGovPayService exceptions
        public const string BearerTokenNull = "Bearer token is null. Unable to initiate payment.";
        public const string GovPayResponseInvalid = "GovPay response does not contain a valid PaymentId.";
        public const string PaymentStatusNotFound = "Payment status not found or status is not available.";
        public const string ErrorInitiatingPayment = "Error occurred while initiating payment.";
        public const string ErrorRetrievingPaymentStatus = "Error occurred while retrieving payment status.";

        // HttpPaymentsService exceptions
        public const string PaymentServiceBaseUrlMissing = "PaymentService BaseUrl configuration is missing";
        public const string PaymentServiceEndPointNameMissing = "PaymentService EndPointName configuration is missing";
        public const string SuccessStatusWithErrorCode = "Error code should be null or empty for a success status.";
        public const string FailedStatusWithoutErrorCode = "Error code cannot be null or empty for a failed status.";
        public const string ErrorStatusWithoutErrorCode = "Error code cannot be null or empty for an error status.";
    }
}
