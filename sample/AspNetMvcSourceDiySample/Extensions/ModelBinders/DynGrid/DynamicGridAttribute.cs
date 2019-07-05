using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    /// <summary>
    /// 标识需要绑定动态表格到目标属性
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Property)]
    public class DynamicGridAttribute : Attribute
    {
        public const string FormNamePrefix = nameof(FormNamePrefix);
        public const string PropName = nameof(PropName);
        public const string RowIndex = nameof(RowIndex);

        /// <summary>
        /// 构造函数-标识需要绑定动态表格到目标属性
        /// </summary>
        /// <param name="gridName">表格名称</param>
        /// <param name="elementNameFormat">表格中的表单元素的名称格式化字符串（备注：必须包含至少 1 个分隔符，且最大为 2 个分隔符）</param>
        /// <param name="formatSeparator">表格中的表单元素的名称格式化字符串中的分隔符。比如：下划线、美元符号、井号等等</param>
        public DynamicGridAttribute(string gridName, string elementNameFormat, string formatSeparator)
        {
            if (string.IsNullOrEmpty(gridName))
            {
                throw new ArgumentNullException(nameof(gridName));
            }
            if (string.IsNullOrEmpty(elementNameFormat))
            {
                throw new ArgumentNullException(nameof(elementNameFormat));
            }
            if (string.IsNullOrEmpty(formatSeparator))
            {
                throw new ArgumentNullException(nameof(formatSeparator));
            }
            this.GridName = gridName;
            this.ElementNameFormat = elementNameFormat;
            this.FormatSeparator = formatSeparator;
        }

        /// <summary>
        /// 获取指定的表格名称
        /// </summary>
        public string GridName { get; private set; }

        /// <summary>
        /// 获取表格中的表单元素的名称格式化字符串（备注：必须包含至少 1 个分隔符，且最大为 2 个分隔符）
        /// </summary>
        public string ElementNameFormat { get; private set; }

        /// <summary>
        /// 获取表格中的表单元素的名称格式化字符串中的分隔符
        /// </summary>
        public string FormatSeparator { get; private set; }
    }
}