namespace EPR.Payment.Mopup.Common.Dtos.Request
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public Guid OrganisationId { get; set; }

        public Guid ExternalPaymentId { get; set; }

        public string? GovpayPaymentId { get; set; }
        public Enums.Status InternalStatusId { get; set; }
        public string Regulator { get; set; } = null!;
        public string? GovPayStatus { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string Reference { get; set; } = null!;
        public decimal Amount { get; set; }
        public string ReasonForPayment { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public Guid UpdatedByOrganisationId { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
