using Newtonsoft.Json;

namespace EPR.Payment.Mopup.Common.Dtos.Response
{
    public class PaymentStatusResponseDto
    {
        [JsonProperty("state")]
        public State? State { get; set; }

        [JsonProperty("paymentId")]
        public string? PaymentId { get; set; }
    }
}
