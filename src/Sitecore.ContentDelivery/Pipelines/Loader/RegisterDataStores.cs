// © 2015 Sitecore Corporation A/S. All rights reserved.

using Sitecore.ContentDelivery.DataStores;
using Sitecore.ContentDelivery.DataStores.Formatters;
using Sitecore.ContentDelivery.DataStores.ItemDataStores;
using Sitecore.Pipelines;

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

            // register formatters
            ContentDeliveryManager.RegisterFieldValueFormatter(new FieldValueFormatter());

            // register writers
            ItemDataStore.RegisterWriter(new ItemDataStoreWriter());

            // register filters
            ItemDataStore.RegisterFilter(new ItemDataStoreFilter());
        }
    }
}
