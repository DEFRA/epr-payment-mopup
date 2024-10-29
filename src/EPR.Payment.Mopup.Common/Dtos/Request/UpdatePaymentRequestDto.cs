using EPR.Payment.Mopup.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPR.Payment.Mopup.Common.Dtos.Request
{
    public class UpdatePaymentRequestDto
    {
        public string? GovPayPaymentId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
