using AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Models
{
    public class AddQuotaViewModel
    {
        [DynamicGrid("VendorStatusGrid", "{0}_{1}_{2}")]
        public ICollection<AddQuotaVendorStatusViewModel> VendorList { get; set; }
    }
}