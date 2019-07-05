using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    public class DynamicGridPropertyBindingContext
    {
        public PropertyDescriptor PropDescriptor { get; set; }

        public DynamicGridAttribute DgAttribute { get; set; }

        public IList ResultList { get; set; }

        public Type GenericArgType { get; set; }
    }
}