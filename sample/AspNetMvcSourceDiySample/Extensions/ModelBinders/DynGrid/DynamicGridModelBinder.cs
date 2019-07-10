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

        #region 公共逻辑

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            if (!DynamicGridTypeContainer.IsValidPropertyType(propertyDescriptor.PropertyType))
            {
                //被 DynamicGridAttribute 标记的属性必须是 IEnumerable 的实现类
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                return;
            }
            bool dynamicRegisterFirst = true; //是否动态注册的优先？默认为 true
            DynamicGridAttribute dgAttribute;
            if (dynamicRegisterFirst)
            {
                dgAttribute = DynamicGridTypeContainer.TryGetDgAttributeFromRegisterd(propertyDescriptor);
                dgAttribute = dgAttribute != null ? dgAttribute : propertyDescriptor
                    .Attributes
                    .OfType<DynamicGridAttribute>()
                    .FirstOrDefault();
            }
            else
            {
                dgAttribute = propertyDescriptor
                    .Attributes
                    .OfType<DynamicGridAttribute>()
                    .FirstOrDefault();
                dgAttribute = dgAttribute != null ? dgAttribute : DynamicGridTypeContainer.TryGetDgAttributeFromRegisterd(propertyDescriptor);
            }
            if (dgAttribute == null)
            {
                //没有标记或注册 DynamicGridAttribute
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
            if (!propType.IsArray
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
                //支持 Struct 类型
            }
            //比如：{Name = "GridViewModel" FullName = "XXX.Models.GridViewModel"}
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericArgType));
            bool isComplexType = GetIsComplexType(controllerContext, bindingContext, propertyDescriptor, dgAttribute, genericArgType);
            //判断配置的 dgAttribute 是否为空、是否有错误
            if (!ValidateDynamicGridAttribute(controllerContext, bindingContext, propertyDescriptor, dgAttribute, genericArgType))
            {
                return;
            }
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
                model = UpdateComplexPropertyCollection(controllerContext,
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
            if (!isComplexType)
            {
                //简单模型不需要验证模型
                return;
            }
            //下面开始验证模型
            OnComplexModelUpdated(controllerContext, bindingContext, propertyBindingContext);
        }

        /// <summary>
        /// 获取是否是复杂模型
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyDescriptor"></param>
        /// <param name="dgAttribute"></param>
        /// <param name="genericArgType"></param>
        /// <returns></returns>
        protected virtual bool GetIsComplexType(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            PropertyDescriptor propertyDescriptor,
            DynamicGridAttribute dgAttribute,
            Type genericArgType)
        {
            bool isComplexType = !TypeDescriptor.GetConverter(genericArgType).CanConvertFrom(typeof(string));
            return isComplexType;
        }

        /// <summary>
        /// 判断配置的 dgAttribute 是否为空、是否有错误。如果有错误，建议在方法内抛出异常，以提醒程序员。
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyDescriptor"></param>
        /// <param name="dgAttribute"></param>
        /// <returns></returns>
        protected virtual bool ValidateDynamicGridAttribute(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            PropertyDescriptor propertyDescriptor,
            DynamicGridAttribute dgAttribute,
            Type genericArgType)
        {
            bool isComplexType = GetIsComplexType(controllerContext, bindingContext, propertyDescriptor, dgAttribute, genericArgType);
            //判断配置的 dgAttribute 是否为空
            if (string.IsNullOrEmpty(dgAttribute.GridName))
            {
                dgAttribute.GridName = propertyDescriptor.Name;
            }
            if (string.IsNullOrEmpty(dgAttribute.FormatSeparator))
            {
                dgAttribute.FormatSeparator = DynamicGridConfiguration.DefaultFormatSeparator;
            }
            if (string.IsNullOrEmpty(dgAttribute.ElementNameFormat))
            {
                if (isComplexType)
                {
                    dgAttribute.ElementNameFormat = "{GridName}" + dgAttribute.FormatSeparator + "{PropName}" + dgAttribute.FormatSeparator + "{RowIndex}";
                }
                else
                {
                    dgAttribute.ElementNameFormat = "{GridName}" + dgAttribute.FormatSeparator + "{RowIndex}";
                }
            }
            //下面是验证配置的 dgAttribute 是否有错误？
            //步骤1：验证是否包含 {GridName}
            if (!dgAttribute.ElementNameFormat.Contains("{GridName}"))
            {
                throw new DynamicGridModelBinderConfigurationException(string.Format("属性 \"{0}\" 上标记的 {1} 配置的 {2}={3} 有错误：必须包含字符串 \"{{GridName}}\"，区分大小写。",
                        propertyDescriptor.Name,
                        nameof(DynamicGridAttribute),
                        nameof(DynamicGridAttribute.ElementNameFormat),
                        dgAttribute.ElementNameFormat
                    ));
            }
            //步骤2：验证是否包含 {RowIndex}
            if (!dgAttribute.ElementNameFormat.Contains("{RowIndex}"))
            {
                throw new DynamicGridModelBinderConfigurationException(string.Format("属性 \"{0}\" 上标记的 {1} 配置的 {2}={3} 有错误：必须包含字符串 \"{{RowIndex}}\"，区分大小写。",
                        propertyDescriptor.Name,
                        nameof(DynamicGridAttribute),
                        nameof(DynamicGridAttribute.ElementNameFormat),
                        dgAttribute.ElementNameFormat
                    ));
            }
            //步骤3：验证是否包含 {PropName}
            if (isComplexType)
            {
                //必须包含
                if (!dgAttribute.ElementNameFormat.Contains("{PropName}"))
                {
                    throw new DynamicGridModelBinderConfigurationException(string.Format("属性 \"{0}\" 上标记的 {1} 配置的 {2}={3} 有错误：由于它作用的 Property 是复杂类型，所以必须包含字符串 \"{{PropName}}\"，区分大小写。",
                            propertyDescriptor.Name,
                            nameof(DynamicGridAttribute),
                            nameof(DynamicGridAttribute.ElementNameFormat),
                            dgAttribute.ElementNameFormat
                        ));
                }
            }
            else
            {
                //禁止包含
                if (dgAttribute.ElementNameFormat.Contains("{PropName}"))
                {
                    throw new DynamicGridModelBinderConfigurationException(string.Format("属性 \"{0}\" 上标记的 {1} 配置的 {2}={3} 有错误：由于它作用的 Property 是简单类型，所以禁止包含字符串 \"{{PropName}}\"，区分大小写。",
                            propertyDescriptor.Name,
                            nameof(DynamicGridAttribute),
                            nameof(DynamicGridAttribute.ElementNameFormat),
                            dgAttribute.ElementNameFormat
                        ));
                }
            }
            return true;
        }

        protected virtual string GetValueProviderResultFormName(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext,
            string propName,
            int currentIndex
            )
        {
            string formName = propertyBindingContext.DgAttribute.ElementNameFormat
                    .Replace("{GridName}", propertyBindingContext.DgAttribute.GridName)
                    .Replace("{RowIndex}", currentIndex.ToString());
            if (!string.IsNullOrEmpty(propName))
            {
                //绑定的是复杂类型
                formName = formName.Replace("{PropName}", propName);
            }
            return formName;
        }

        /// <summary>
        /// 创建模型的实例
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="absBindingContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 生成要遍历的索引集合。从 min 到 max。默认 min = <see cref="DynamicGridAttribute.RowIndexMin"/> , max = <see cref="DynamicGridAttribute.RowIndexMax"/>
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        protected virtual IEnumerable<int> GetZeroBasedIndexes(int min, int max)
        {
            int i;
            if (min >= 0)
            {
                i = min;
            }
            else
            {
                i = DynamicGridConfiguration.DefaultRowIndexMin;
            }
            int rightMax;
            if (max > i)
            {
                rightMax = max;
            }
            else
            {
                rightMax = DynamicGridConfiguration.DefaultRowIndexMax;
            }
            while (i <= rightMax)
            {
                yield return i;
                i++;
            }
        }

        #endregion

        #region 简单类型

        protected virtual object BindDynamicGridSimpleProperty(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext
            )
        {
            return UpdateSimplePropertyCollection(controllerContext,
                bindingContext,
                propertyBindingContext);
        }

        protected virtual object UpdateSimplePropertyCollection(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext)
        {
            IEnumerable<int> indexes = GetZeroBasedIndexes(propertyBindingContext.DgAttribute.RowIndexMin,
                propertyBindingContext.DgAttribute.RowIndexMax
            );

            IModelBinder elementBinder = Binders.GetBinder(propertyBindingContext.GenericArgType);
            int userInputIndex = 0;//表示用户实际有输入的值的索引
            foreach (int currentIndex in indexes)
            {
                string formName = GetValueProviderResultFormName(controllerContext,
                    bindingContext,
                    propertyBindingContext,
                    null,
                    currentIndex
                );
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
                        continue;
                    }
                    continue;
                }
                userInputIndex++;
                string formNameForShow = GetValueProviderResultFormName(controllerContext,
                    bindingContext,
                    propertyBindingContext,
                    null,
                    userInputIndex
                );
                //这里需要类型转换
                object realValue = null;
                //判断1：需要判断枚举类型 或者 Nullable<Enum> 类型
                Type nullablePropTypeOrNull = Nullable.GetUnderlyingType(propertyBindingContext.GenericArgType); //如果是 Nullable 类型，则返回不为 Null
                bool isNullableEnum = nullablePropTypeOrNull != null && nullablePropTypeOrNull.IsEnum;
                if (propertyBindingContext.GenericArgType.IsEnum || isNullableEnum)
                {
                    //目标类型为枚举
                    if (isNullableEnum)
                    {
                        object enumObj = TypeHelpers.TryParseEnum(nullablePropTypeOrNull,
                            valueProviderResult.AttemptedValue,
                            !propertyBindingContext.DgAttribute.IfContainsEnumTypeThenEnableCaseSensitive
                        );
                        if (enumObj == null)
                        {
                            //这个值在目标枚举中没有定义
                            bindingContext.ModelState.AddModelError(formNameForShow,
                                DynamicGridConfiguration.FormatEnumNoDefinedErrorMessage(formNameForShow,
                                    valueProviderResult.AttemptedValue,
                                    nullablePropTypeOrNull
                                )
                            );
                            continue;
                        }
                        //到这里说明 enumObj 不为 Null
                        realValue = enumObj;
                    }
                    else
                    {
                        object enumObj = TypeHelpers.TryParseEnum(propertyBindingContext.GenericArgType,
                            valueProviderResult.AttemptedValue,
                            !propertyBindingContext.DgAttribute.IfContainsEnumTypeThenEnableCaseSensitive
                        );
                        if (enumObj == null)
                        {
                            //这个值在目标枚举中没有定义
                            bindingContext.ModelState.AddModelError(formNameForShow, 
                                DynamicGridConfiguration.FormatEnumNoDefinedErrorMessage(formNameForShow,
                                    valueProviderResult.AttemptedValue,
                                    propertyBindingContext.GenericArgType
                                )
                            );
                            continue;
                        }
                        //到这里说明 enumObj 不为 Null
                        realValue = enumObj;
                    }
                }
                if (realValue == null)
                {
                    try
                    {
                        realValue = valueProviderResult.ConvertTo(propertyBindingContext.GenericArgType);
                    }
                    catch (Exception ex)
                    {
                        //下面是获取用户实际有输入的值的索引而生成的表单名，方便提示用户是哪一个值有错误。
                        bindingContext.ModelState.AddModelError(formNameForShow, ex);
                        continue;
                    }
                }
                if (realValue == null)
                {
                    //仍然为空，则忽略
                    continue;
                }
                try
                {
                    propertyBindingContext.ResultList.Add(realValue);
                }
                catch (Exception ex)
                {
                    bindingContext.ModelState.AddModelError(formNameForShow, ex);
                    continue;
                }
            }
            if (propertyBindingContext.ResultList == null || propertyBindingContext.ResultList.Count == 0)
            {
                return null;
            }
            return propertyBindingContext.ResultList;
        }

        #endregion

        #region 复杂类型

        protected virtual object UpdateComplexPropertyCollection(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext)
        {
            IEnumerable<int> indexes = GetZeroBasedIndexes(propertyBindingContext.DgAttribute.RowIndexMin,
                propertyBindingContext.DgAttribute.RowIndexMax
            );

            IModelBinder elementBinder = Binders.GetBinder(propertyBindingContext.GenericArgType);

            #region 这中间的代码主要是 想验证模型。检查该属性上是否标记了 System.ComponentModel.DataAnnotations 中的一些 Attribute

            //下面这一行代码是方案1
            //PropertyInfo[] propInfoList = propertyBindingContext.GenericArgType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //下面这一行代码是方案2
            PropertyDescriptorCollection propCollections = GetPropDescriptorCollection(propertyBindingContext.GenericArgType);

            #endregion

            TransferSingle transferSingle = new TransferSingle();
            foreach (int currentIndex in indexes)
            {
                if (transferSingle.Single != null)
                {
                    //注意：这里的 single 对象应该延迟初始化
                    transferSingle.Single = null;
                }

                //下面是方案1
                //BindComplexProperty(controllerContext,
                //    bindingContext,
                //    propertyBindingContext,
                //    propInfoList,
                //    transferSingle,
                //    currentIndex);

                //下面是方案2
                BindComplexProperty(controllerContext,
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

        /// <summary>
        /// 获取值提供结果
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyBindingContext"></param>
        /// <param name="propName"></param>
        /// <param name="currentIndex"></param>
        /// <returns></returns>
        protected virtual ValueProviderResultWrapper GetValueProviderResult(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext,
            string propName,
            int currentIndex
            )
        {
            string formName = GetValueProviderResultFormName(controllerContext,
                bindingContext,
                propertyBindingContext,
                propName,
                currentIndex
            );
            var result = bindingContext.ValueProvider.GetValue(formName);
            return new ValueProviderResultWrapper
            {
                ValueResult = result,
                FormName = formName
            };
        }

        /*

        /// <summary>
        /// 这个是方案1（测试通过，但采用方案2最优）
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyBindingContext"></param>
        /// <param name="propInfoList"></param>
        /// <param name="transferSingle"></param>
        /// <param name="currentIndex"></param>
        protected virtual void BindComplexProperty(ControllerContext controllerContext,
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

        */

        /// <summary>
        /// 延迟初始化对象
        /// </summary>
        /// <param name="propertyBindingContext"></param>
        /// <param name="transferSingle"></param>
        protected virtual void DelayNewObject(DynamicGridPropertyBindingContext propertyBindingContext,
            TransferSingle transferSingle)
        {
            if (transferSingle.Single == null)
            {
                transferSingle.Single = CreateModelInstance(propertyBindingContext.GenericArgType, propertyBindingContext);
            }
        }

        protected virtual void BindComplexProperty(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            DynamicGridPropertyBindingContext propertyBindingContext,
            PropertyDescriptorCollection propCollections,
            TransferSingle transferSingle,
            int currentIndex)
        {
            Predicate<string> propertyFilter = bindingContext.PropertyFilter;
            Queue<KeyValuePair<string, string>> modelErrorDic = new Queue<KeyValuePair<string, string>>();
            for (int i = 0; i < propCollections.Count; i++)
            {
                PropertyDescriptor property = propCollections[i];
                //下面这个方法已经排除了只读属性
                if (!TypeHelpers.ShouldUpdateProperty(property, propertyFilter))
                {
                    continue;
                }
                PropertyMappingAttribute propertyMappingAttribute = property.Attributes.OfType<PropertyMappingAttribute>().FirstOrDefault();
                string propName;
                if (propertyMappingAttribute != null && !string.IsNullOrEmpty(propertyMappingAttribute.Name))
                {
                    propName = propertyMappingAttribute.Name;
                }
                else
                {
                    propName = property.Name;
                }
                ValueProviderResultWrapper valueProviderResultWrapper = GetValueProviderResult(controllerContext,
                    bindingContext,
                    propertyBindingContext,
                    propName,
                    currentIndex
                );
                string formNameForShow = GetValueProviderResultFormName(controllerContext,
                    bindingContext,
                    propertyBindingContext,
                    propName,
                    propertyBindingContext.ResultList.Count + 1
                );//这里的 formNameForShow 仅仅用来显示错误消息
                if (valueProviderResultWrapper.ValueResult == null
                    || string.IsNullOrEmpty(valueProviderResultWrapper.ValueResult.AttemptedValue))
                {
                    //说明没有从提交的表单中获取到值，那么先缓存到临时的集合中，因为有可能客户端一整个行都没有填写。
                    RequiredAttribute requiredAttribute = property.Attributes.OfType<RequiredAttribute>().FirstOrDefault();
                    if (requiredAttribute != null && !requiredAttribute.AllowEmptyStrings)
                    {
                        if (property.PropertyType.IsValueType && !TypeHelpers.IsNullableValueType(property.PropertyType))
                        {
                            modelErrorDic.Enqueue(new KeyValuePair<string, string>(formNameForShow,
                                    requiredAttribute.FormatErrorMessage(formNameForShow)
                                )
                            );
                        }
                        //由于 string 在最后会统一去验证，所以这里不能加 string 的判断。
                    }
                    continue;
                }
                //这里需要类型转换
                object realValue = null;
                //判断1：枚举类型
                Type nullablePropTypeOrNull = Nullable.GetUnderlyingType(property.PropertyType); //如果是 Nullable 类型，则返回不为 Null
                bool isNullableEnum = nullablePropTypeOrNull != null && nullablePropTypeOrNull.IsEnum;
                if (property.PropertyType.IsEnum || isNullableEnum)
                {
                    //目标类型为枚举
                    if (isNullableEnum)
                    {
                        object enumObj = TypeHelpers.TryParseEnum(nullablePropTypeOrNull, 
                            valueProviderResultWrapper.ValueResult.AttemptedValue, 
                            !propertyBindingContext.DgAttribute.IfContainsEnumTypeThenEnableCaseSensitive);
                        if (enumObj == null)
                        {
                            //这个值在目标枚举中没有定义
                            modelErrorDic.Enqueue(new KeyValuePair<string, string>(formNameForShow,
                                    DynamicGridConfiguration.FormatEnumNoDefinedErrorMessage(formNameForShow,
                                        valueProviderResultWrapper.ValueResult.AttemptedValue,
                                        nullablePropTypeOrNull
                                    )
                                )
                            );
                            continue;
                        }
                        //到这里说明 enumObj 不为 Null
                        realValue = enumObj;
                    }
                    else
                    {
                        object enumObj = TypeHelpers.TryParseEnum(property.PropertyType, 
                            valueProviderResultWrapper.ValueResult.AttemptedValue,
                            !propertyBindingContext.DgAttribute.IfContainsEnumTypeThenEnableCaseSensitive
                        );
                        if (enumObj == null)
                        {
                            //这个值在目标枚举中没有定义
                            modelErrorDic.Enqueue(new KeyValuePair<string, string>(formNameForShow,
                                    DynamicGridConfiguration.FormatEnumNoDefinedErrorMessage(formNameForShow,
                                        valueProviderResultWrapper.ValueResult.AttemptedValue,
                                        property.PropertyType
                                    )
                                )
                            );
                            continue;
                        }
                        //到这里说明 enumObj 不为 Null
                        realValue = enumObj;
                    }
                }
                if (realValue == null)
                {
                    try
                    {
                        //注意1：如果 property.PropertyType 的类型是枚举类型，即使是非法的值，也能转换成功，这不是我们期望的。
                        realValue = valueProviderResultWrapper.ValueResult.ConvertTo(property.PropertyType);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ex.Message;
                        if (string.IsNullOrEmpty(errorMessage) && ex.InnerException != null)
                        {
                            errorMessage = ex.InnerException.Message;
                        }
                        modelErrorDic.Enqueue(new KeyValuePair<string, string>(formNameForShow,
                                        errorMessage
                                    )
                                );
                        continue;
                    }
                    if (realValue == null)
                    {
                        continue;
                    }
                }
                DelayNewObject(propertyBindingContext, transferSingle);
                try
                {
                    property.SetValue(transferSingle.Single, realValue);
                }
                catch (Exception ex)
                {
                    bindingContext.ModelState.AddModelError(formNameForShow, ex);
                }
            }
            if (transferSingle.Single == null || modelErrorDic.Count == 0)
            {
                return;
            }
            var enumerator = modelErrorDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                bindingContext.ModelState.AddModelError(enumerator.Current.Key, enumerator.Current.Value);
            }
        }

        protected virtual void ValidateComplexModel(ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            string key,
            string errorMessage)
        {
            
        }

        protected virtual PropertyDescriptorCollection GetPropDescriptorCollection(Type type)
        {
            ICustomTypeDescriptor customTypeDescriptor = new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
            return customTypeDescriptor.GetProperties();
        }

        /// <summary>
        /// 当模型被验证以后
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyBindingContext"></param>
        protected virtual void OnComplexModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext, DynamicGridPropertyBindingContext propertyBindingContext)
        {
            if (propertyBindingContext.ResultList == null || propertyBindingContext.ResultList.Count == 0)
            {
                return;
            }
            Dictionary<string, bool> startedValid = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            for (int currentIndex = 0; currentIndex < propertyBindingContext.ResultList.Count; currentIndex++)
            {
                object item = propertyBindingContext.ResultList[currentIndex];
                ModelMetadata modelMetadataList = ModelMetadataProviders.Current.GetMetadataForType(() => item, item.GetType());
                foreach (ModelValidationResult validationResult in ModelValidator.GetModelValidator(modelMetadataList, controllerContext).Validate(null))
                {
                    string subPropertyName = GetValueProviderResultFormName(controllerContext,
                        bindingContext,
                        propertyBindingContext,
                        validationResult.MemberName,
                        currentIndex
                    );
                    if (!startedValid.ContainsKey(subPropertyName))
                    {
                        startedValid[subPropertyName] = bindingContext.ModelState.IsValidField(subPropertyName);
                    }
                    if (startedValid[subPropertyName])
                    {
                        bindingContext.ModelState.AddModelError(subPropertyName, validationResult.Message);
                    }
                }
            }
        }

        #endregion

        #region 私有成员

        protected class TransferSingle
        {
            public object Single { get; set; }
        }

        protected class ValueProviderResultWrapper
        {
            public ValueProviderResult ValueResult { get; set; }

            public string FormName { get; set; }
        }

        #endregion
    }
}