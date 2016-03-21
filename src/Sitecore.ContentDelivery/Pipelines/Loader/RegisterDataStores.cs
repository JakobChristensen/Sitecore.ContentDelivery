// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.ContentDelivery.DataStores.FileDataStores;
using Sitecore.ContentDelivery.DataStores.Formatters;
using Sitecore.ContentDelivery.DataStores.ItemDataStores;
using Sitecore.IO;
using Sitecore.Pipelines;
using Sitecore.Web;

namespace Sitecore.ContentDelivery.Pipelines.Loader
{
    public class RegisterDataStores
    {
        public void Process([NotNull] PipelineArgs args)
        {
            // mount data stores
            ContentDeliveryManager.MountDataStore(new ItemDataStore("master"));
            ContentDeliveryManager.MountDataStore(new ItemDataStore("core"));
            ContentDeliveryManager.MountDataStore(new ItemDataStore("web"));

            // file data stores
            var fileDataStoreFolder = FileUtil.MapPath("/App_Data/DataStores");
            foreach (var fileName in Directory.GetFiles(fileDataStoreFolder))
            {
                var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

                switch (extension)
                {
                    case ".json":
                        ContentDeliveryManager.MountDataStore(new JsonFileDataStore(fileName));
                        break;
                }
            }

            // register formatters
            ContentDeliveryManager.RegisterFieldValueFormatter(new FieldValueFormatter());
        }
    }
}
