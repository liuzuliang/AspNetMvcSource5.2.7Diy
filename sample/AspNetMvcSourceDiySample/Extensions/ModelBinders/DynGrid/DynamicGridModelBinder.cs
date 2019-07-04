using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    /// <summary>
    /// 自定义的动态表格模型绑定。
    /// </summary>
    public class DynamicGridModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            DynamicGridAttribute propertyBinderAttribute = propertyDescriptor
                .Attributes
                .OfType<DynamicGridAttribute>()
                .FirstOrDefault();
            if (propertyBinderAttribute == null
                || propertyDescriptor.PropertyType == typeof(string)
                || propertyDescriptor.PropertyType.GetInterface(typeof(IEnumerable).Name) == null)
            {
                //按道理来说，是非法的值，被 DynamicGridAttribute 标记的属性必须是 IEnumerable 的实现类
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                return;
            }
            BindDynamicGridProperty(controllerContext, bindingContext, propertyDescriptor, propertyBinderAttribute);
        }

        protected virtual void BindDynamicGridProperty(ControllerContext controllerContext, 
            ModelBindingContext bindingContext, 
            PropertyDescriptor propertyDescriptor,
            DynamicGridAttribute propertyBinderAttribute)
        {
            Type propType = propertyDescriptor.PropertyType;//比如：ICollection<GridViewModel>
            Type genericArgType = propType.GetElementType() ?? propType.GetGenericArguments().FirstOrDefault();
            //比如：{Name = "GridViewModel" FullName = "XXX.Models.GridViewModel"}
            if (genericArgType == null)
            {
                return;
            }
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericArgType));
            bool isComplexType = !TypeDescriptor.GetConverter(genericArgType).CanConvertFrom(typeof(string));
            //object value = null;
            //propertyDescriptor.SetValue(bindingContext.Model, value);
        }
    }
}