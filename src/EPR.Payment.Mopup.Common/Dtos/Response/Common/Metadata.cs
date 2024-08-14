using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Mopup.Common.Dtos.Response.Common
{
    [ExcludeFromCodeCoverage]
    public class Metadata
    {
        [JsonProperty("ledger_code")]
        public string? LedgerCode { get; set; }

        [JsonProperty("internal_reference_number")]
        public int InternalReferenceNumber { get; set; }
    }
}
