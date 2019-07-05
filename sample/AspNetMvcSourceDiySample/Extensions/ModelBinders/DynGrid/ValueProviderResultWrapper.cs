using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid
{
    public class ValueProviderResultWrapper
    {
        public ValueProviderResult ValueResult { get; set; }

        public string FormName { get; set; }
    }
}