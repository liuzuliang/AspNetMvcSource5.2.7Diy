using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    internal class TypeHelpers
    {
        public static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool ShouldUpdateProperty(PropertyDescriptor property, Predicate<string> propertyFilter)
        {
            if (property.IsReadOnly && !CanUpdateReadonlyTypedReference(property.PropertyType))
            {
                return false;
            }
            if (propertyFilter != null && !propertyFilter(property.Name))
            {
                return false;
            }
            return true;
        }

        public static bool CanUpdateReadonlyTypedReference(Type type)
        {
            if (type.IsValueType)
            {
                return false;
            }
            if (type.IsArray)
            {
                return false;
            }
            if (type == typeof(string))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试着转换成枚举类型。如果返回 Null，则表示转换失败。
        /// </summary>
        /// <param name="enumType">要转换成目标枚举的类型</param>
        /// <param name="value">值</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public static object TryParseEnum(Type enumType, string value, bool ignoreCase)
        {
            object enumObj;
            try
            {
                enumObj = Enum.Parse(enumType, value, ignoreCase);
            }
            catch (Exception)
            {
                return null;
            }
            //到这里说明
            if (Enum.IsDefined(enumType, enumObj))
            {
                //到这里说明是真正的、合法的值
                return enumObj;
            }
            return null; //非法的值
        }
    }
}