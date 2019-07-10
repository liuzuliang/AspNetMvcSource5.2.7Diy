using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    /// <summary>
    /// 动态给模型 Property 绑定（或注册） DynamicGridAttribute。
    /// 备注：由于某些原因，开发者无法修改某个模型，从而无法在这个模型的 Property 上标记 DynamicGridAttribute，
    /// 利用此类，可以达到动态注册 DynamicGridAttribute。
    /// </summary>
    public static class DynamicGridTypeContainer
    {
        private static readonly Dictionary<string, DynamicGridAttribute> _dicDynamicGridTypes = new Dictionary<string, DynamicGridAttribute>();
        private const string SEPARATOR = "=";

        /// <summary>
        /// 动态注册
        /// </summary>
        /// <param name="pInfo"></param>
        /// <param name="dgConfig"></param>
        public static void Register(PropertyInfo pInfo, DynamicGridConfig dgConfig)
        {
            if (pInfo == null)
            {
                throw new ArgumentNullException(nameof(pInfo));
            }
            if (dgConfig == null)
            {
                throw new ArgumentNullException(nameof(dgConfig));
            }
            if (!IsValidPropertyType(pInfo.PropertyType))
            {
                throw new ArgumentException(
                    string.Format("被 DynamicGridAttribute 标记的类 \"{0}\"的属性 \"public {1} {2}  {{ get; set; }}\"必须是 IEnumerable 的实现类，且不能是 string 类型。",
                        pInfo.DeclaringType.FullName,
                        pInfo.PropertyType.FullName,
                        pInfo.Name
                    ), 
                    nameof(pInfo)
                );
            }
            if (!pInfo.CanWrite)
            {
                throw new ArgumentException(
                    string.Format("被 DynamicGridAttribute 标记的类 \"{0}\"的属性 \"public {1} {2}  {{ get; }}\"必须是具有 public，且可以写入的属性。",
                        pInfo.DeclaringType.FullName,
                        pInfo.PropertyType.FullName,
                        pInfo.Name
                    ),
                    nameof(pInfo)
                );
            }
            string key = GetCacheKey(pInfo);
            AddOrReplaceDgAttribute(key, dgConfig);
        }

        /// <summary>
        /// 动态注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propExpression"></param>
        /// <param name="dgConfig"></param>
        public static void Register<T, TProperty>(Expression<Func<T, TProperty>> propExpression, DynamicGridConfig dgConfig)
            where T: class, new()
            where TProperty : IEnumerable
        {
            if (propExpression == null)
            {
                throw new ArgumentNullException(nameof(propExpression));
            }
            if (dgConfig == null)
            {
                throw new ArgumentNullException(nameof(dgConfig));
            }
            MemberExpression memberExpression;
            InspectIsPropertyExpression(propExpression, true, out memberExpression);
            if (!IsValidPropertyType(memberExpression.Type))
            {
                throw new ArgumentException(
                    string.Format("被 DynamicGridAttribute 标记的类 \"{0}\"的属性 \"public {1} {2}  {{ get; set; }}\"必须是 IEnumerable 的实现类，且不能是 string 类型。",
                        memberExpression.Member.DeclaringType.FullName, 
                        memberExpression.Type.FullName, 
                        memberExpression.Member.Name
                    ),
                    nameof(propExpression)
                );
            }
            if (!(memberExpression.Member as PropertyInfo).CanWrite)
            {
                throw new ArgumentException(
                    string.Format("被 DynamicGridAttribute 标记的类 \"{0}\"的属性 \"public {1} {2}  {{ get; set; }}\"必须是具有 public，且可以写入的属性。",
                        memberExpression.Member.DeclaringType.FullName,
                        memberExpression.Type.FullName,
                        memberExpression.Member.Name
                    ),
                    nameof(propExpression)
                );
            }
            string key = GetCacheKey(memberExpression);
            AddOrReplaceDgAttribute(key, dgConfig);
        }

        /// <summary>
        /// 给某个类动态注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static DynamicGridTypeFor<T> For<T>()
            where T : class, new()
        {
            return new DynamicGridTypeFor<T>();
        }

        /// <summary>
        /// 尝试着从注册的类型中获取动态注册的 <see cref="DynamicGridAttribute"/> ，通过方法 <see cref="Register"/> 注册。如果找不到，则返回 Null.
        /// </summary>
        /// <param name="propertyDescriptor"></param>
        /// <returns></returns>
        public static DynamicGridAttribute TryGetDgAttributeFromRegisterd(PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor == null)
            {
                throw new ArgumentNullException(nameof(propertyDescriptor));
            }
            if (!IsValidPropertyType(propertyDescriptor.PropertyType))
            {
                //被 DynamicGridAttribute 标记的属性必须是 IEnumerable 的实现类
                return null;
            }
            string key = GetCacheKey(propertyDescriptor);
            if (_dicDynamicGridTypes.ContainsKey(key))
            {
                return _dicDynamicGridTypes[key];
            }
            return null;
        }

        #region Private Method

        private static void AddOrReplaceDgAttribute(string key, DynamicGridConfig dgConfig)
        {
            _dicDynamicGridTypes[key] = new DynamicGridAttribute
            {
                ElementNameFormat = dgConfig.ElementNameFormat,
                FormatSeparator = dgConfig.FormatSeparator,
                GridName = dgConfig.GridName,
                IfContainsEnumTypeThenEnableCaseSensitive = dgConfig.IfContainsEnumTypeThenEnableCaseSensitive,
                RowIndexMax = dgConfig.RowIndexMax,
                RowIndexMin = dgConfig.RowIndexMin
            };
        }

        /// <summary>
        /// 判断是否是合法的属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValidPropertyType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type != typeof(string)
                && type.GetInterface(typeof(IEnumerable).Name) != null;
        }

        private static bool InspectIsPropertyExpression<T, TProperty>(Expression<Func<T, TProperty>> express, bool throwExOnNull, out MemberExpression memberExpression)
        {
            if (express == null)
            {
                throw new ArgumentNullException("express");
            }
            memberExpression = express.Body as MemberExpression;
            if (memberExpression == null)
            {
                if (throwExOnNull)
                {
                    throw new ArgumentException("请为类型 \"" + typeof(T).FullName + "\" 的指定一个属性（Property）作为 Lambda 的主体（Body）。");
                }
                return false;
            }
            if (memberExpression.Member.MemberType != MemberTypes.Property)
            {
                if (throwExOnNull)
                {
                    throw new ArgumentException(
                        string.Format("被 DynamicGridAttribute 标记的类 \"{0}\"的成员 \"{1} {2}\"错误，必须是属性（Property）类型。",
                            memberExpression.Member.DeclaringType.FullName,
                            memberExpression.Type.FullName,
                            memberExpression.Member.Name
                        )
                    );
                }
                return false;
            }
            return true;
        }

        private static string GetCacheKey(PropertyInfo pInfo)
        {
            return GetCacheKey(pInfo.DeclaringType.FullName, pInfo.PropertyType.FullName, pInfo.Name);
        }

        private static string GetCacheKey(PropertyDescriptor pInfo)
        {
            string classFullName = pInfo.ComponentType.FullName;
            return GetCacheKey(classFullName, pInfo.PropertyType.FullName, pInfo.Name);
        }

        private static string GetCacheKey(MemberExpression memberExpression)
        {
            return GetCacheKey(memberExpression.Member.DeclaringType.FullName, memberExpression.Type.FullName, memberExpression.Member.Name);
        }

        private static string GetCacheKey(string classFullName, string propertyFullName, string propertyName)
        {
            return classFullName + SEPARATOR + propertyFullName + SEPARATOR + propertyName;
        }

        #endregion
    }
}