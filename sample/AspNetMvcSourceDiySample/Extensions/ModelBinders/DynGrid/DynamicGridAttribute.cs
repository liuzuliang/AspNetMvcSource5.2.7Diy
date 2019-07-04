using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    [AttributeUsageAttribute(AttributeTargets.Property)]
    public class DynamicGridAttribute : Attribute
    {
        public DynamicGridAttribute(string gridName, string innerElementNameFormatInGridForm)
        {
            this.GridName = gridName;
            this.InnerElementNameFormatInGridForm = innerElementNameFormatInGridForm;
        }

        /// <summary>
        /// 获取指定的表格名称
        /// </summary>
        public string GridName { get; private set; }

        /// <summary>
        /// 获取表格中的表单元素的名称格式化
        /// </summary>
        public string InnerElementNameFormatInGridForm { get; private set; }
    }
}