﻿using Newtonsoft.Json;

namespace EPR.Payment.Mopup.Common.Dtos.Response.Common
{
    public class Refunds
    {
        [JsonProperty("href")]
        public string? Href { get; set; }

        [JsonProperty("method")]
        public string? Method { get; set; }
    }
}