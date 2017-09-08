// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.ContentDelivery.Databases.FileDatabases;

namespace Sitecore.ContentDelivery
{
    public static class ContentDeliveryConfig
    {
        public static void MountDatabases()
        {
            var directory = HttpContext.Current.Server.MapPath(Constants.AppDataDatabasesDirectory);
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
                        ContentDeliveryManager.MountDatabase(new JsonFileDatabase(fileName));
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

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetDatabase", "sitecore/get/{databaseName}", new
            {
                controller = "ContentDelivery",
                action = "GetDatabase",
                databaseName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetChildren", "sitecore/get/children/{databaseName}/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "GetChildren",
                databaseName = "",
                itemName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetItems", "sitecore/get/items/{databaseName}", new
            {
                controller = "ContentDelivery",
                action = "GetItems",
                databaseName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetTemplate", "sitecore/get/template/{databaseName}/{*templateName}", new
            {
                controller = "ContentDelivery",
                action = "GetTemplate",
                databaseName = "",
                templateName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetTemplates", "sitecore/get/templates/{databaseName}", new
            {
                controller = "ContentDelivery",
                action = "GetTemplates",
                databaseName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.DumpDatabase", "sitecore/get/dump/{databaseName}", new
            {
                controller = "ContentDelivery",
                action = "DumpDatabase",
                databaseName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.GetItem", "sitecore/get/item/{databaseName}/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "GetItem",
                databaseName = "",
                itemName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.PutItems", "sitecore/put/items/{databaseName}", new
            {
                controller = "ContentDelivery",
                action = "SaveItems",
                databaseName = ""
            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.DeleteItems", "sitecore/delete/items/{databaseName}/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "DeleteItems",
                databaseName = "",
                itemName = ""

            });

            RouteTable.Routes.MapRoute("Sitecore.ContentDelivery.AddItems", "sitecore/put/item/{databaseName}/{*itemName}", new
            {
                controller = "ContentDelivery",
                action = "AddItem",
                databaseName = "",
                itemName = ""
            });
        }
    }
}
