// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.ContentDelivery.Extensions
{
    public static class DatabaseExtensions
    {
        public static IEnumerable<Item> GetItemsByName([NotNull] this Database database, [NotNull] string itemName, [NotNull] params ID[] templateId)
        {
            var indexName = "sitecore_" + database.Name.ToLowerInvariant() + "_index";

            var index = ContentSearchManager.GetIndex(indexName);
            using (var context = index.CreateSearchContext())
            {
                IEnumerable<SearchResultItem> queryable;

                if (!templateId.Any())
                {
                    queryable = context.GetQueryable<SearchResultItem>().Where(item => item.Name == itemName);
                }
                else
                {
                    queryable = context.GetQueryable<SearchResultItem>().Where(item => item.Name == itemName && templateId.Contains(item.TemplateId));
                }

                var itemIds = queryable.Select(item => item.ItemId).ToList().Distinct();

                return itemIds.Select(database.GetItem).Where(i => i != null);
            }
        }

        public static IEnumerable<Item> GetItemsByTemplate([NotNull] this Database database, [NotNull] params ID[] templateId)
        {
            var indexName = "sitecore_" + database.Name.ToLowerInvariant() + "_index";

            var index = ContentSearchManager.GetIndex(indexName);
            using (var context = index.CreateSearchContext())
            {
                var queryable = context.GetQueryable<SearchResultItem>().Where(item => templateId.Contains(item.TemplateId)).Select(item => item.ItemId).ToList().Distinct();

                return queryable.Select(database.GetItem).Where(i => i != null && !StandardValuesManager.IsStandardValuesHolder(i) && i.Name != "$name");
            }
        }
    }
}
