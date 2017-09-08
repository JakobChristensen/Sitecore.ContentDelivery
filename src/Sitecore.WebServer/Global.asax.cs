// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web;
using Sitecore.ContentDelivery;

namespace Sitecore.WebServer
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ContentDeliveryConfig.RegisterRoutes();
            ContentDeliveryConfig.MountDatabases();
        }
    }
}
