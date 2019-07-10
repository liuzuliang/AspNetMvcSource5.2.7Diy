using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    /// <summary>
    /// 配置
    /// </summary>
    public class DynamicGridConfig : IDynamicGridSetting
    {
        /// <summary>
        /// 循环获取时，最小值的索引是？备注：必须大于或等于 0，如果未配置或配置错误，表示从 0 开始获取。
        /// </summary>
        public int RowIndexMin { get; set; }

        /// <summary>
        /// 循环获取时，最小值的索引是？备注：必须大于或等于 <see cref="RowIndexMin"/>，如果未配置或配置错误，表示截止到索引为 1000。
        /// </summary>
        public int RowIndexMax { get; set; }

        /// <summary>
        /// 获取或设置指定的表格名称。如果为 Empty 或 Null，则默认为它作用的 Property 的名称。
        /// </summary>
        public string GridName { get; set; }

        /// <summary>
        /// 获取或设置表格中的表单元素的名称格式化字符串（备注：必须包含至少 1 个分隔符，且最大为 2 个分隔符）。
        /// 如果为 Empty 或 Null，则看它作用的 Property 的类型。如下：
        /// 1. 如果为 IEnumerable&lt;string&gt; 、IEnumerable&lt;int&gt; 等简单类型，那么为 {GridName}${RowIndex}。中间的连接符可通过 <see cref="FormatSeparator"/> 配置。
        /// 2. 如果为 IEnumerable&lt;ClassX&gt; 、IEnumerable&lt;Struct&gt; 等复杂类型，那么为 {GridName}${PropName}${RowIndex}。中间的连接符可通过 <see cref="FormatSeparator"/> 配置。
        /// </summary>
        public string ElementNameFormat { get; set; }

        /// <summary>
        /// 获取或设置表格中的表单元素的名称格式化字符串中的分隔符（备注：强烈建议设置为 C# 变量名以外的字符，比如：下划线、美元符号、井号等等）。
        /// 如果为 Empty 或 Null，则默认为 <see cref="DynamicGridConfiguration.DefaultFormatSeparator"/>
        /// </summary>
        public string FormatSeparator { get; set; }

        /// <summary>
        /// 获取或设置如果它作用的 Property 的类型是枚举类型，那么是否启用大小写敏感来获取表单或 QueryString 的值。默认为 false，即不区分大小写。
        /// </summary>
        public bool IfContainsEnumTypeThenEnableCaseSensitive { get; set; }
    }
}