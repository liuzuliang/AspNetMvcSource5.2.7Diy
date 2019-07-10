using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    public class DynamicGridModelBinderConfigurationException : Exception
    {
        public DynamicGridModelBinderConfigurationException(string errorMessage)
            :base(errorMessage)
        {

        }
    }
}