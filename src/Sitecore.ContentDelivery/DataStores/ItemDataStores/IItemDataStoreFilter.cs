// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;

namespace Sitecore.ContentDelivery.DataStores.ItemDataStores
{
    public interface IItemDataStoreFilter
    {
        double Priority { get; }

        [NotNull]
        IEnumerable<Item> Filter([NotNull] IEnumerable<Item> children, [NotNull] string key, [NotNull] string value);

        [NotNull]
        IQueryable<SearchResultItem> Filter([NotNull] IQueryable<SearchResultItem> queryable, [NotNull] string fieldName, [NotNull] string value);
    }
}
