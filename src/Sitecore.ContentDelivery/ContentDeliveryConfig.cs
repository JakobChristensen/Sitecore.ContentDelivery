// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.ContentDelivery.DataStores.FileDataStores;

namespace Sitecore.ContentDelivery
{
    public static class ContentDeliveryConfig
    {
        public static void MountDataStores()
        {
            var directory = HttpContext.Current.Server.MapPath("/App_Data/DataStores");
            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (var fileName in Directory.GetFiles(directory))
            {
                var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

                switch (extension)
                {
                    case ".json":
                        ContentDeliveryManager.MountDataStore(new JsonFileDataStore(fileName));
                        break;
                }
            }
        }

        public static void RegisterRoutes()
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

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.DumpDataStore", "sitecore/get/dump/{dataStoreName}", new
            {
                controller = "ContentDelivery",
                action = "DumpDataStore",
                dataStoreName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetItem", "sitecore/get/item/{dataStoreName}/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "GetItem",
                dataStoreName = "",
                itemName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.Call", "sitecore/call/{*className}", new
            {
                controller = "ContentDelivery",
                action = "Call",
                className = ""
            });
        }
    }
}
