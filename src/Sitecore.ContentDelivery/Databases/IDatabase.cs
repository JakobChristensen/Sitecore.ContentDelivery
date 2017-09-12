// © 2015-2017 by Jakob Christensen. All rights reserved.

using System.Collections.Generic;
using System.Web.Mvc;
using Sitecore.ContentDelivery.Web;

namespace Sitecore.ContentDelivery.Databases
{
    public interface IDatabase
    {
        [NotNull]
        string DatabaseName { get; }

        [NotNull]
        ActionResult AddItem([NotNull] RequestParameters requestParameters, [NotNull] string itemPath, [NotNull] string templateName);

        [NotNull]
        ActionResult DeleteItems([NotNull] RequestParameters requestParameters, [NotNull] IEnumerable<string> items);

        [NotNull]
        ActionResult GetChildren([NotNull] RequestParameters requestParameters, [NotNull] string itemName);

        [NotNull]
        ActionResult GetDatabase([NotNull] RequestParameters requestParameters);

        [NotNull]
        ActionResult GetItem([NotNull] RequestParameters requestParameters, [NotNull] string itemName);

        [NotNull]
        ActionResult GetItems([NotNull] RequestParameters requestParameters);

        [NotNull]
        ActionResult GetTemplate([NotNull] RequestParameters requestParameters, [NotNull] string templateName);

        [NotNull]
        ActionResult GetTemplates([NotNull] RequestParameters requestParameters);

        void Initialize([NotNull] IDictionary<string, string> parameters, [NotNull] string currentDirectory, [NotNull] string appDataDirectory);

        [NotNull]
        ActionResult SaveItems([NotNull] RequestParameters requestParameters, [NotNull] Dictionary<string, string> fields);
    }
}
