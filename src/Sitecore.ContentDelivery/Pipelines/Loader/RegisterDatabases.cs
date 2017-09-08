// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.ContentDelivery.Databases.FileDatabases;
using Sitecore.ContentDelivery.Databases.Formatters;
using Sitecore.ContentDelivery.Databases.ItemDatabases;
using Sitecore.IO;
using Sitecore.Pipelines;

namespace Sitecore.ContentDelivery.Pipelines.Loader
{
    public class RegisterDatabases
    {
        public void Process([NotNull] PipelineArgs args)
        {
            // mount databases
            ContentDeliveryManager.MountDatabase(new ItemDatabase("master"));
            ContentDeliveryManager.MountDatabase(new ItemDatabase("core"));
            ContentDeliveryManager.MountDatabase(new ItemDatabase("web"));

            // register formatters
            ContentDeliveryManager.RegisterFieldValueFormatter(new FieldValueFormatter());

            // file databases
            var fileDatabaseFolder = FileUtil.MapPath(Constants.AppDataDatabasesDirectory);
            if (!Directory.Exists(fileDatabaseFolder))
            {
                return;
            }

            foreach (var fileName in Directory.GetFiles(fileDatabaseFolder))
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
    }
}
