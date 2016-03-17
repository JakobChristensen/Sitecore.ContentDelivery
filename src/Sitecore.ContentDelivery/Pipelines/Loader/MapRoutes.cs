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
            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetBundle", "cd/bundle", new
            {
                controller = "ContentDelivery",
                action = "GetBundle"
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetDataStore", "cd/{dataStoreName}", new
            {
                controller = "ContentDelivery",
                action = "GetDataStore",
                dataStoreName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetChildren", "cd/{dataStoreName}/children/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "GetChildren",
                dataStoreName = "",
                itemName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetItems", "cd/{dataStoreName}/items", new
            {
                controller = "ContentDelivery",
                action = "GetItems",
                dataStoreName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetItem", "cd/{dataStoreName}/items/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "GetItem",
                dataStoreName = "",
                itemName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetTemplates", "cd/{dataStoreName}/templates", new
            {
                controller = "ContentDelivery",
                action = "GetTemplates",
                dataStoreName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetTemplate", "cd/{dataStoreName}/templates/{*templateName}", new
            {
                controller = "ContentDelivery",
                action = "GetTemplate",
                dataStoreName = "",
                templateName = ""
            });
        }
    }
}
