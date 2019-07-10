using AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetMvcSourceDiySample.Models
{
    //[ModelBinder(typeof(DynamicGridModelBinder))]
    public class AddQuotaViewModel
    {
        [Required]
        public string Name { get; set; }

        //[DynamicGrid]//单元测试：全部默认
        //[DynamicGrid(FormatSeparator = "_", RowIndexMin = 1, RowIndexMax = 200)]//单元测试：全部默认
        [DynamicGrid(ElementNameFormat = "{GridName}_{PropName}_{RowIndex}")]
        //[DynamicGrid(ElementNameFormat = "Fix_{GridName}_{PropName}_{RowIndex}")]//单元测试：支持这种写法，但由于Form中没有这种格式的Name元素，将无法获取到任何值
        public ICollection<AddQuotaVendorStatusViewModel> VendorStatusGrid { get; set; }

        [DynamicGrid(FormatSeparator = SystemConst.DgdFormatSeparator)]
        public ICollection<int> YourQuestionIdList { get; set; }


        //单元测试1[测试通过]：把 VendorStatusGrid 的类型改为：ICollection<AddQuotaVendorStatusViewModel>
        //单元测试2[测试通过]：把 VendorStatusGrid 的类型改为：AddQuotaVendorStatusViewModel[]
        //单元测试4[测试通过]：把 VendorStatusGrid 的类型改为：ICollection<string>
        //单元测试5[测试通过]：把 VendorStatusGrid 的类型改为：string[]
        //单元测试6[测试通过]：把 VendorStatusGrid 的类型 AddQuotaVendorStatusViewModel 中的 Status 的类型改为 double 
        //单元测试7[测试通过]：类型 AddQuotaVendorStatusViewModel 中 Property 上面标记了 [Required] 等其它验证 Attribute，检查是否生效了？
        //单元测试8[测试通过]：更改 FormatSeparator 和 ElementNameFormat 的格式。
        //单元测试9[测试通过]：DynamicGridAttribute 中的 GridName 应该可选输入，默认为 属性名称。
        //单元测试10[测试通过]：DynamicGridAttribute 中的 GridName 和 ElementNameFormat 也应该可选输入.
        //单元测试11[测试通过]：测试类型为 struct
        //单元测试12[测试通过]：允许 AddQuotaVendorStatusViewModel 类中自定义属性名（增加一个 Attribute）
    }
}