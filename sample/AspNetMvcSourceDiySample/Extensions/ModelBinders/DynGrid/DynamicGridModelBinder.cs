using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    /// <summary>
    /// 自定义的动态表格模型绑定。
    /// </summary>
    public class DynamicGridModelBinder : DefaultModelBinder
    {
        private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetMethod("ToArray");

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            DynamicGridAttribute dgAttribute = propertyDescriptor
                .Attributes
                .OfType<DynamicGridAttribute>()
                .FirstOrDefault();
            if (dgAttribute == null
                || propertyDescriptor.PropertyType == typeof(string)
                || propertyDescriptor.PropertyType.GetInterface(typeof(IEnumerable).Name) == null)
            {
                //按道理来说，是非法的值，被 DynamicGridAttribute 标记的属性必须是 IEnumerable 的实现类
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                return;
            }
            BindDynamicGridProperty(controllerContext, bindingContext, propertyDescriptor, dgAttribute);
        }

        protected virtual void BindDynamicGridProperty(ControllerContext controllerContext, 
            ModelBindingContext bindingContext, 
            PropertyDescriptor propertyDescriptor,
            DynamicGridAttribute dgAttribute)
        {
            Type propType = propertyDescriptor.PropertyType;
            //比如：ICollection<GridViewModel>、GridViewModel[]、List<string>、List<int?> 等等
            //允许数组类型
            if ( !propType.IsArray
                && (!propType.IsGenericType || propType.GetGenericArguments().Count() > 1)
            )
            {
                //排除非泛型或者泛型参数达到2个或2个以上
                throw new SystemException(
                    string.Format("属性 \"{0}\" 的类型是 {1}，这是一个非法的值，必须是 ClassX[] 类型或者 IEnumerable<ClassX> 类型，且泛型参数最大为1个。",
                        propertyDescriptor.Name,
                        propType.FullName
                    )
                );
            }
            Type genericArgType;
            if (propType.IsGenericType)
            {
                genericArgType = propType.GetGenericArguments().First();
            }
            else
            {
                //数组
                genericArgType = propType.GetElementType();
            }
            if (TypeHelpers.IsNullableValueType(genericArgType)
                || genericArgType.IsEnum
                || genericArgType == typeof(string))
            {
                //允许 Nullable 类型、String 类型、枚举类型
            }
            else if (genericArgType.IsGenericType || genericArgType.IsArray)
            {
                //抛出异常
                throw new SystemException(
                    string.Format("属性 \"{0}\" 的类型是 {1}，这是一个非法的值。内部的 {2} 类型不能是泛型，也不能是数组类型。",
                        propertyDescriptor.Name,
                        propType.FullName,
                        genericArgType.FullName
                    )
                );
            }
            else
            {
                //support struct type?
            }
            //比如：{Name = "GridViewModel" FullName = "XXX.Models.GridViewModel"}
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericArgType));
            bool isComplexType = !TypeDescriptor.GetConverter(genericArgType).CanConvertFrom(typeof(string));
            var propertyBindingContext = new DynamicGridPropertyBindingContext
            {
                ResultList = list,
                GenericArgType = genericArgType,
                DgAttribute = dgAttribute,
                PropDescriptor = propertyDescriptor
            };
            object model;
            if (isComplexType)
            {
                //复杂模型
                model = BindDynamicGridComplexProperty(controllerContext,
                    bindingContext,
                    propertyBindingContext
                );
            }
            else
            {
                //简单模型
                model = BindDynamicGridSimpleProperty(controllerContext,
                    bindingContext,
                    propertyBindingContext
                );
            }
            if (model == null)
            {
                return;
            }
            if (propType.IsArray)
            {
                model = ToArrayMethod.MakeGenericMethod(genericArgType).Invoke(this, new[] { model });
            }
            propertyBindingContext.PropDescriptor.SetValue(bindingContext.Model, model);
        }

        protected virtual object BindDynamicGridSimpleProperty(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext
            )
        {
            return UpdateSimplePropertyCollection(controllerContext,
                bindingContext,
                propertyBindingContext);
        }

        protected virtual object BindDynamicGridComplexProperty(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext
            )
        {
            return UpdateComplexPropertyCollection(controllerContext,
                bindingContext,
                propertyBindingContext);
        }

        protected virtual object UpdateSimplePropertyCollection(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext)
        {
            IEnumerable<int> indexes = GetZeroBasedIndexes();

            IModelBinder elementBinder = Binders.GetBinder(propertyBindingContext.GenericArgType);
            List<string> modelList = new List<string>();
            foreach (int currentIndex in indexes)
            {
                string formName = $"{propertyBindingContext.DgAttribute.GridName}_{ currentIndex }";
                ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(formName);
                if (valueProviderResult == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(valueProviderResult.AttemptedValue))
                {
                    //检查该属性上是否标记了 System.ComponentModel.DataAnnotations 中的一些 Attribute
                    //如果标记了，验证失败后，需要写入 ModelState.Error
                    //暂时未实现

                    if (bindingContext.ModelMetadata.ConvertEmptyStringToNull)
                    {
                        return null;
                    }

                    continue;
                }
                //这里需要类型转换
                object realValue = null;
                try
                {
                    realValue = valueProviderResult.ConvertTo(propertyBindingContext.GenericArgType);
                }
                catch (Exception)
                {
                    continue; // ignore error ?
                }
                try
                {
                    propertyBindingContext.ResultList.Add(realValue);
                }
                catch (Exception)
                {
                    continue; // ignore error ?
                }
            }
            // if there weren't any elements at all in the request, just return
            if (modelList.Count == 0)
            {
                return null; // ?
            }
            return modelList;
        }

        protected virtual object UpdateComplexPropertyCollection(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext)
        {
            IEnumerable<int> indexes = GetZeroBasedIndexes();

            IModelBinder elementBinder = Binders.GetBinder(propertyBindingContext.GenericArgType);

            #region 这中间的代码主要是 想验证模型。检查该属性上是否标记了 System.ComponentModel.DataAnnotations 中的一些 Attribute

            PropertyDescriptorCollection propCollections = GetPropDescriptorCollection(propertyBindingContext.GenericArgType);
            
            #endregion

            PropertyInfo[] propInfoList = propertyBindingContext.GenericArgType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            TransferSingle transferSingle = new TransferSingle();
            foreach (int currentIndex in indexes)
            {
                if (transferSingle.Single != null)
                {
                    //注意：这里的 single 对象应该延迟初始化
                    transferSingle.Single = null;
                }

                //下面是方案1
                //SetProperty(controllerContext,
                //    bindingContext,
                //    propertyBindingContext,
                //    propInfoList,
                //    transferSingle,
                //    currentIndex);

                //下面是方案2
                SetPropertyDescriptor(controllerContext,
                    bindingContext,
                    propertyBindingContext,
                    propCollections,
                    transferSingle,
                    currentIndex);

                if (transferSingle.Single == null)
                {
                    continue;
                }
                propertyBindingContext.ResultList.Add(transferSingle.Single);
            }
            if (propertyBindingContext.ResultList.Count == 0)
            {
                return null; // ???
            }
            return propertyBindingContext.ResultList;
        }

        protected virtual ValueProviderResultWrapper GetValueProviderResult(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext,
            string propName,
            int currentIndex
            )
        {
            string formName = $"{propertyBindingContext.DgAttribute.GridName}_{ propName }_{ currentIndex }";
            var result = bindingContext.ValueProvider.GetValue(formName);
            return new ValueProviderResultWrapper
            {
                ValueResult = result,
                FormName = formName
            };
        }

        protected virtual void SetProperty(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext,
            PropertyInfo[] propInfoList,
            TransferSingle transferSingle,
            int currentIndex)
        {
            foreach (PropertyInfo propItem in propInfoList)
            {
                if (!propItem.CanWrite)
                {
                    //跳过只读属性
                    continue;
                }
                ValueProviderResultWrapper valueProviderResultWrapper = GetValueProviderResult(controllerContext,
                    bindingContext,
                    propertyBindingContext,
                    propItem.Name,
                    currentIndex
                );
                if (valueProviderResultWrapper.ValueResult == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(valueProviderResultWrapper.ValueResult.AttemptedValue))
                {
                    //检查该属性上是否标记了 System.ComponentModel.DataAnnotations 中的一些 Attribute
                    //如果标记了，验证失败后，需要写入 ModelState.Error
                    //暂时未实现
                    continue;
                }
                //这里需要类型转换
                object realValue = null;
                try
                {
                    realValue = valueProviderResultWrapper.ValueResult.ConvertTo(propItem.PropertyType);
                }
                catch (Exception)
                {
                    continue; // ignore error ?
                }
                DelayNewObject(propertyBindingContext, transferSingle);
                try
                {
                    propItem.SetValue(transferSingle.Single, realValue);
                }
                catch (Exception)
                {
                    continue; // ignore error ?
                }
            }
        }

        protected virtual void DelayNewObject(DynamicGridPropertyBindingContext propertyBindingContext,
            TransferSingle transferSingle)
        {
            if (transferSingle.Single == null)
            {
                transferSingle.Single = CreateModelInstance(propertyBindingContext.GenericArgType, propertyBindingContext);
            }
        }

        protected virtual void SetPropertyDescriptor(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext,
            PropertyDescriptorCollection propCollections,
            TransferSingle transferSingle,
            int currentIndex)
        {
            Predicate<string> propertyFilter = bindingContext.PropertyFilter;
            for (int i = 0; i < propCollections.Count; i++)
            {
                PropertyDescriptor property = propCollections[i];
                //下面这个方法已经排除了只读属性
                if (!TypeHelpers.ShouldUpdateProperty(property, propertyFilter))
                {
                    continue;
                }
                ValueProviderResultWrapper valueProviderResultWrapper = GetValueProviderResult(controllerContext,
                    bindingContext,
                    propertyBindingContext,
                    property.Name,
                    currentIndex
                );
                if (valueProviderResultWrapper.ValueResult == null)
                {
                    continue;
                }
                //这里需要类型转换
                object realValue = null;
                try
                {
                    realValue = valueProviderResultWrapper.ValueResult.ConvertTo(property.PropertyType);
                }
                catch (Exception ex)
                {
                    bindingContext.ModelState.AddModelError(valueProviderResultWrapper.FormName, ex);
                    continue; // ignore error ?
                }
                DelayNewObject(propertyBindingContext, transferSingle);
                try
                {
                    property.SetValue(transferSingle.Single, realValue);
                }
                catch (Exception ex)
                {
                    bindingContext.ModelState.AddModelError(valueProviderResultWrapper.FormName, ex);
                }
            }
        }

        protected virtual object CreateModelInstance(Type modelType, DynamicGridPropertyBindingContext absBindingContext)
        {
            try
            {
                return Activator.CreateInstance(modelType);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("属性 \"{0}\" 的类型是 {1}。无法创建内部的 {2} 类型的实例，请确保它是公共的，且包含公共的无参数的构造函数。",
                    absBindingContext.PropDescriptor.Name,
                    absBindingContext.PropDescriptor.PropertyType.FullName,
                    modelType.FullName
                ), exception);
            }
        }

        protected virtual void ValidateModel(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            string key,
            string errorMessage)
        {
            //bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            bindingContext.ModelState.AddModelError(key, errorMessage);
        }

        protected virtual PropertyDescriptorCollection GetPropDescriptorCollection(Type type)
        {
            ICustomTypeDescriptor customTypeDescriptor = new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
            return customTypeDescriptor.GetProperties();
        }

        private static IEnumerable<int> GetZeroBasedIndexes()
        {
            int i = 0;
            while (i < 1000)
            {
                yield return i;
                i++;
            }
        }
    }
}