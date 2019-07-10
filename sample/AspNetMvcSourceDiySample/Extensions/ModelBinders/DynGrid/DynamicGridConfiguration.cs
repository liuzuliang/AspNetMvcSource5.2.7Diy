using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    /// <summary>
    /// 消息常量
    /// </summary>
    public static class DynamicGridConfiguration
    {
        /// <summary>
        /// 默认表格中的表单元素的名称格式化字符串中的分隔符。
        /// </summary>
        public static string DefaultFormatSeparator { get; } = "_";

        /// <summary>
        /// 默认表格中的表单元素的名称格式化字符串中的分隔符。
        /// </summary>
        public static int DefaultRowIndexMin { get; } = 0;

        /// <summary>
        /// 默认表格中的表单元素的名称格式化字符串中的分隔符。
        /// </summary>
        public static int DefaultRowIndexMax { get; } = 1000;

        /// <summary>
        /// 格式化枚举没有定义的错误消息
        /// </summary>
        /// <param name="formName"></param>
        /// <param name="formValue"></param>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string FormatEnumNoDefinedErrorMessage(string formName, string formValue, Type enumType)
        {
            // Chinese
            //return string.Format("表单文本框 \"{0}\" （值  \"{1}\" ）在枚举 \"{2}\" 中没有定义。",
            //    formName,
            //    formValue,
            //    enumType.FullName
            //);

            // English
            return string.Format("The text box \"{0}\" in the form (value \"{1}\" ) is not defined in the enumeration \"{2}\".",
                formName,
                formValue,
                enumType.FullName
            );
        }
    }
}