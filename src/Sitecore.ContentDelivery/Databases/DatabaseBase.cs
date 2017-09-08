// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web.Mvc;
using Sitecore.ContentDelivery.Web;

namespace Sitecore.ContentDelivery.Databases
{
    public abstract class DatabaseBase : IDatabase
    {
        protected DatabaseBase([NotNull] string databaseName)
        {
            DatabaseName = databaseName;
        }

        public string DatabaseName { get; }

        public abstract ActionResult GetChildren(RequestParameters requestParameters, string itemName);

        public abstract ActionResult GetDatabase(RequestParameters requestParameters);

        public abstract ActionResult GetItems(RequestParameters requestParameters);

        public abstract ActionResult GetTemplate(RequestParameters requestParameters, string templateName);

        public abstract ActionResult GetItem(RequestParameters requestParameters, string itemName);

        public abstract ActionResult GetTemplates(RequestParameters requestParameters);
    }
}
