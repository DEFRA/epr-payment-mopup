using Newtonsoft.Json;

namespace EPR.Payment.Mopup.Common.Dtos.Response.Common
{
    public class Params
    {
        [JsonProperty("chargeTokenId")]
        public string? ChargeTokenId { get; set; }
    }
}
