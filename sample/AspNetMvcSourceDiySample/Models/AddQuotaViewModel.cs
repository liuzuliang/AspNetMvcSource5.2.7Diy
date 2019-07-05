using AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Models
{
    public class AddQuotaViewModel
    {
        public const string VendorListDynamicGridFormatSeparator = "_";

        public const string VendorListDynamicGridFormat =
            "{" + DynamicGridAttribute.FormNamePrefix + "}" + VendorListDynamicGridFormatSeparator +
            "{" + DynamicGridAttribute.PropName + "}" + VendorListDynamicGridFormatSeparator +
            "{" + DynamicGridAttribute.RowIndex + "}";

        [Required]
        public string Name { get; set; }

        [DynamicGrid("VendorStatusGrid", VendorListDynamicGridFormat, VendorListDynamicGridFormatSeparator)]
        public ICollection<AddQuotaVendorStatusViewModel> VendorList { get; set; }

        //单元测试1[测试通过]：把 VendorList 的类型改为：ICollection<AddQuotaVendorStatusViewModel>
        //单元测试2[测试通过]：把 VendorList 的类型改为：AddQuotaVendorStatusViewModel[]
        //单元测试4：把 VendorList 的类型改为：ICollection<string>
        //单元测试5：把 VendorList 的类型改为：string[]
        //单元测试6[测试通过]：把 VendorList 的类型 AddQuotaVendorStatusViewModel 中的 Status 的类型改为 double 
        //单元测试7：类型 AddQuotaVendorStatusViewModel 中 Property 上面标记了 [Required] 等其它验证 Attribute，检查是否生效了？
        //单元测试8：更改 VendorListDynamicGridFormatSeparator 和 VendorListDynamicGridFormat 的格式。
        //单元测试9：DynamicGridAttribute 中的 GridName 应该可选输入，默认为 属性名称。
        //单元测试9：DynamicGridAttribute 中的 VendorListDynamicGridFormat 和 VendorListDynamicGridFormatSeparator 也应该可选输入.

    }
}