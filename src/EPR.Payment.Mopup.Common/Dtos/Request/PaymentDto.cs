﻿namespace EPR.Payment.Mopup.Common.Dtos.Request
{
    public class PaymentDto
    {
        public Guid ExternalPaymentId { get; set; }
        public string? GovPayPaymentId { get; set; }
        public Enums.Status Status { get; set; }
    }
}
