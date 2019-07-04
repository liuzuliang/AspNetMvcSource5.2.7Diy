using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Models
{
    public class AddQuotaVendorStatusViewModel
    {
        [Required]
        public string Vendor { get; set; }

        [Required]
        public string Status { get; set; }
    }
}