﻿using Newtonsoft.Json;

namespace EPR.Payment.Mopup.Common.Dtos.Response.Common
{
    public class CardDetails
    {
        [JsonProperty("last_digits_card_number")]
        public string? LastDigitsCardNumber { get; set; }

        [JsonProperty("first_digits_card_number")]
        public string? FirstDigitsCardNumber { get; set; }

        [JsonProperty("cardholder_name")]
        public string? CardholderName { get; set; }

        [JsonProperty("expiry_date")]
        public string? ExpiryDate { get; set; }

        [JsonProperty("billing_address")]
        public BillingAddress? BillingAddress { get; set; }

        [JsonProperty("card_brand")]
        public string? CardBrand { get; set; }

        [JsonProperty("card_type")]
        public string? CardType { get; set; }

        [JsonProperty("wallet_type")]
        public string? WalletType { get; set; }
    }
}
