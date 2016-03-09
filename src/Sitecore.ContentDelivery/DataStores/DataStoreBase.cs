// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web.Mvc;

namespace Sitecore.ContentDelivery.DataStores
{
    public abstract class DataStoreBase : IDataStore
    {
        protected DataStoreBase(string dataStoreName)
        {
            DataStoreName = dataStoreName;
        }

        public string DataStoreName { get; }

        public abstract ActionResult GetChildren(string itemName);

        public abstract ActionResult GetDataStore();

        public abstract ActionResult GetItems();

        public abstract ActionResult GetTemplate(string templateName);

        public abstract ActionResult GetItem(string itemName);

        public abstract ActionResult GetTemplates();
    }
}
