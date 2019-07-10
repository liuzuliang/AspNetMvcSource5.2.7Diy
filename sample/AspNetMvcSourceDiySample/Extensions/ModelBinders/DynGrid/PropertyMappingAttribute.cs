using System;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    /// <summary>
    /// 标识映射 Html Form 中动态表格中每一行中的某个列。（备注：这个类需要移到单个的命名空间下，为了防止组件依赖 System.Web.Mvc）
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Property)]
    public class PropertyMappingAttribute : Attribute
    {
        /// <summary>
        /// 构造函数-标识需要绑定动态表格到目标属性
        /// </summary>
        public PropertyMappingAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}