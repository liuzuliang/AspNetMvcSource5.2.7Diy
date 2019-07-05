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
        public double Age { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string TestRequired { get; set; }

        public string ReadonlyProp { get; }

        [Required]
        public MQuoSubAns Status { get; set; }
    }
}