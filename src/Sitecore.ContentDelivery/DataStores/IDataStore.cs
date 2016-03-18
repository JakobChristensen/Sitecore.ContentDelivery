// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Web.Mvc;
using Sitecore.ContentDelivery.Web;

namespace Sitecore.ContentDelivery.DataStores
{
    public interface IDataStore
    {
        [NotNull]
        string DataStoreName { get; }

        [NotNull]
        ActionResult GetChildren([NotNull] RequestParameters requestParameters, [NotNull] string itemName);

        [NotNull]
        ActionResult GetDataStore(RequestParameters requestParameters);

        [NotNull]
        ActionResult GetItem(RequestParameters requestParameters, [NotNull] string itemName);

        [NotNull]
        ActionResult GetItems(RequestParameters requestParameters);

        [NotNull]
        ActionResult GetTemplate(RequestParameters requestParameters, [NotNull] string templateName);

        [NotNull]
        ActionResult GetTemplates(RequestParameters requestParameters);
    }
}
