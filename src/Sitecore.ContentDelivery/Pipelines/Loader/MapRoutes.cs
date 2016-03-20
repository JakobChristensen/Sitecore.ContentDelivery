// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Pipelines;

namespace Sitecore.ContentDelivery.Pipelines.Loader
{
    public class MapRoutes
    {
        public void Process([NotNull] PipelineArgs args)
        {
            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetBundle", "sitecore/get", new
            {
                controller = "ContentDelivery",
                action = "GetBundle"
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetDataStore", "sitecore/get/{dataStoreName}", new
            {
                controller = "ContentDelivery",
                action = "GetDataStore",
                dataStoreName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetChildren", "sitecore/get/children/{dataStoreName}/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "GetChildren",
                dataStoreName = "",
                itemName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetItems", "sitecore/get/items/{dataStoreName}", new
            {
                controller = "ContentDelivery",
                action = "GetItems",
                dataStoreName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetTemplate", "sitecore/get/template/{dataStoreName}/{*templateName}", new
            {
                controller = "ContentDelivery",
                action = "GetTemplate",
                dataStoreName = "",
                templateName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetTemplates", "sitecore/get/templates/{dataStoreName}", new
            {
                controller = "ContentDelivery",
                action = "GetTemplates",
                dataStoreName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetItem", "sitecore/get/{dataStoreName}/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "GetItem",
                dataStoreName = "",
                itemName = ""
            });
        }
    }
}
