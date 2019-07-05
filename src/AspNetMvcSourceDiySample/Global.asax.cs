using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AspNetMvcSourceDiySample
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public MvcApplication()
        {
            this.AcquireRequestState += MvcApplication_AcquireRequestState;
        }

        private void MvcApplication_AcquireRequestState(object sender, EventArgs e)
        {
            MvcApplication app = sender as MvcApplication;
            if (app.Context.Handler == null)
            {
                return;
            }
            if (app.Context.Handler is MvcHandler)
            {
                string requestUrl = app.Context.Request.Url.ToString();
                string hello = requestUrl;
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
