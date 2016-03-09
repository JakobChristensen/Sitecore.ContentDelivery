// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Web.Mvc;

namespace Sitecore.ContentDelivery.DataStores
{
    public interface IDataStore
    {
        [NotNull]
        string DataStoreName { get; }

        [NotNull]
        ActionResult GetChildren([NotNull] string itemName);

        [NotNull]
        ActionResult GetDataStore();

        [NotNull]
        ActionResult GetItem([NotNull] string itemName);

        [NotNull]
        ActionResult GetItems();

        [NotNull]
        ActionResult GetTemplate([NotNull] string templateName);

        [NotNull]
        ActionResult GetTemplates();
    }
}
