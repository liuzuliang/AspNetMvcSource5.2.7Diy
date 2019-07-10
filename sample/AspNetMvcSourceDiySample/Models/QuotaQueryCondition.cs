using AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetMvcSourceDiySample.Models
{
    [ModelBinder(typeof(DynamicGridModelBinder))]
    public class QuotaQueryCondition
    {
        [DynamicGrid(FormatSeparator = SystemConst.DgdFormatSeparator)]
        public ICollection<int> Age { get; set; }

        [DynamicGrid(
            FormatSeparator = SystemConst.DgdFormatSeparator, 
            IfContainsEnumTypeThenEnableCaseSensitive = false
        )]
        public ICollection<MQuoSubAns> Status { get; set; }
    }
}