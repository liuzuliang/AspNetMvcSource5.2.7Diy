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
            // if this property is rejected by the filter, move on
            if (propertyFilter != null && !propertyFilter(property.Name))
            {
                return false;
            }
            // otherwise, allow
            return true;
        }

        public static bool CanUpdateReadonlyTypedReference(Type type)
        {
            // value types aren't strictly immutable, but because they have copy-by-value semantics
            // we can't update a value type that is marked readonly
            if (type.IsValueType)
            {
                return false;
            }

            // arrays are mutable, but because we can't change their length we shouldn't try
            // to update an array that is referenced readonly
            if (type.IsArray)
            {
                return false;
            }

            // special-case known common immutable types
            if (type == typeof(string))
            {
                return false;
            }

            return true;
        }
    }
}