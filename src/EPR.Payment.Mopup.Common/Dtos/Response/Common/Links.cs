using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Mopup.Common.Dtos.Response.Common
{
    [ExcludeFromCodeCoverage]
    public class Links
    {
        [JsonProperty("self")]
        public Self? Self { get; set; }

        [JsonProperty("next_url")]
        public NextUrl? NextUrl { get; set; }

        [JsonProperty("next_url_post")]
        public NextUrlPost? NextUrlPost { get; set; }

        [JsonProperty("events")]
        public Events? Events { get; set; }

        [JsonProperty("refunds")]
        public Refunds? Refunds { get; set; }

        [JsonProperty("cancel")]
        public Cancel? Cancel { get; set; }
    }
}
