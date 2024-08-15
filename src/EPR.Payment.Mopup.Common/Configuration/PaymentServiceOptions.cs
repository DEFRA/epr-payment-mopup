﻿using EPR.Payment.Mopup.Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Mopup.Common.Configuration
{
    public class PaymentServiceOptions
    {
        [Required(ErrorMessage = "Return URL is required")]
        [ValidUrl]
        public string? ReturnUrl { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Error URL is required")]
        [ValidUrl]
        public string? ErrorUrl { get; set; }
    }
}
