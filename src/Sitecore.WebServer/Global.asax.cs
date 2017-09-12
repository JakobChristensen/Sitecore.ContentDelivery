// © 2015-2017 by Jakob Christensen. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using Sitecore.ContentDelivery;
using Sitecore.ContentDelivery.Databases.FileDatabases;

namespace Sitecore.WebServer
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // register routes
            ContentDeliveryConfig.RegisterRoutes("sitecore");

            // register databases
            var appDataDirectory = HttpContext.Current.Server.MapPath(Constants.AppDataDatabasesDirectory);
            if (!Directory.Exists(appDataDirectory))
            {
                return;
            }

            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? appDataDirectory;

            foreach (var fileName in Directory.GetFiles(appDataDirectory))
            {
                var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
                switch (extension)
                {
                    case ".json":
                        var parameters = new Dictionary<string, string>
                        {
                            ["file"] = fileName
                        };        

                        var jsonFileDatabase = new JsonFileDatabase();
                        jsonFileDatabase.Initialize(parameters, currentDirectory, appDataDirectory);

                        ContentDeliveryManager.MountDatabase(jsonFileDatabase);
                        break;
                }
            }
        }
    }
}
