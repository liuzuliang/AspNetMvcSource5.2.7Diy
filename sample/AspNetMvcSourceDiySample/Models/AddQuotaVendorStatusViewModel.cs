//using AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Models
{
    public struct AddQuotaVendorStatusViewModel
    {
        [Required]
        [StringLength(12)]
        public string Vendor { get; set; }

        [Required]
        //[PropertyMapping("TestAge")]
        public double Age { get; set; }

        [Required]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string TestRequired { get; set; }

        public string ReadonlyProp { get; }

        [Required]
        public MQuoSubAns Status { get; set; }


        public UserQueryCondition TestComplexProperty { get; set; }
    }
}