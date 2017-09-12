// © 2015-2017 by Jakob Christensen. All rights reserved.

using System.Collections.Generic;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.ContentDelivery.Databases;
using Sitecore.ContentDelivery.Databases.Formatters;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Pipelines;
using Sitecore.Reflection;
using Sitecore.Xml;

namespace Sitecore.ContentDelivery.Pipelines.Loader
{
    public class MapRoutes
    {                                              
        public void Process([NotNull] PipelineArgs args)
        {
            // register routes
            var basePath = Settings.GetSetting(Constants.BasePath, "sitecore");
            ContentDeliveryConfig.RegisterRoutes(basePath);

            // register formatters
            ContentDeliveryManager.RegisterFieldValueFormatter(new SitecoreFieldValueFormatter());

            // register databases
            var appDataDirectory = FileUtil.MapPath(Constants.AppDataDatabasesDirectory);
            var currentDirectory = FileUtil.MapPath("/");

            var treeNode = Factory.GetConfigNode("sitecore.contentdelivery/tree");
            foreach (XmlNode rootNode in treeNode.ChildNodes)
            {
                if (rootNode.Attributes == null)
                {
                    Log.Error("Empty root node definition. Skipping.", GetType());
                    continue;
                }

                var database = ReflectionUtil.CreateObject(rootNode) as IDatabase;
                if (database == null)
                {
                    Log.Error("Failed to create database: " + XmlUtil.GetAttribute("type", rootNode), GetType());
                    continue;
                }

                var parameters = new Dictionary<string, string>();
                foreach (XmlAttribute attribute in rootNode.Attributes)
                {
                    parameters[attribute.Name] = attribute.Value;
                }

                database.Initialize(parameters, currentDirectory, appDataDirectory);

                ContentDeliveryManager.MountDatabase(database);
            }
        }
    }
}
