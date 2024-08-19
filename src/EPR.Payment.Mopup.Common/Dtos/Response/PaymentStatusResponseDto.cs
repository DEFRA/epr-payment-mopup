﻿using EPR.Payment.Mopup.Common.Dtos.Response.Common;
using Newtonsoft.Json;

namespace EPR.Payment.Mopup.Common.Dtos.Response
{
    public class PaymentStatusResponseDto
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("metadata")]
        public Metadata? Metadata { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("state")]
        public State? State { get; set; }

        [JsonProperty("paymentId")]
        public string? PaymentId { get; set; }

        [JsonProperty("payment_provider")]
        public string? PaymentProvider { get; set; }

        [JsonProperty("created_date")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("refund_summary")]
        public RefundSummary? RefundSummary { get; set; }

        [JsonProperty("settlement_summary")]
        public SettlementSummary? SettlementSummary { get; set; }

        [JsonProperty("card_details")]
        public CardDetails? CardDetails { get; set; }

        [JsonProperty("delayed_capture")]
        public bool DelayedCapture { get; set; }

        [JsonProperty("moto")]
        public bool Moto { get; set; }

        [JsonProperty("return_url")]
        public string? ReturnUrl { get; set; }

        [JsonProperty("authorisation_mode")]
        public string? AuthorisationMode { get; set; }

        [JsonProperty("_links")]
        public Links? Links { get; set; }
    }

    public class NextUrlPost
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("params")]
        public Params? Params { get; set; }

        [JsonProperty("href")]
        public string? Href { get; set; }

        [JsonProperty("method")]
        public string? Method { get; set; }
    }
}
