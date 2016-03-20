// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Extensions.StringExtensions;

namespace Sitecore.ContentDelivery.DataStores.ItemDataStores
{
    public class ItemDataStoreFilter : IItemDataStoreFilter
    {
        public double Priority { get; } = 1000;

        public IEnumerable<Item> Filter(IEnumerable<Item> children, string key, string value)
        {
            if (!key.EndsWith("]"))
            {
                children = children.Where(i => i[key] == value);
                return children;
            }

            var n = key.LastIndexOf('[');
            if (n < 0)
            {
                throw new SyntaxErrorException("[ expected");
            }

            var op = key.Mid(n + 1, key.Length - n - 2);
            key = key.Left(n).Trim();

            switch (op)
            {
                case "not":
                    return children.Where(i => i[key] != value);

                case "in":
                    var l1 = value.Split('|');
                    return children.Where(i => l1.Contains(i[key]));

                case "not in":
                    var l2 = value.Split('|');
                    return children.Where(i => !l2.Contains(i[key]));

                case "has":
                    return children.Where(t => !string.IsNullOrEmpty(t[key]));
            }

            return children;
        }

        public IQueryable<SearchResultItem> Filter(IQueryable<SearchResultItem> queryable, string fieldName, string value)
        {
            if (fieldName == "query")
            {
                return queryable.Where(item => item.Content.Contains(value));
            }

            if (!fieldName.EndsWith("]"))
            {
                switch (fieldName.ToLowerInvariant())
                {
                    case "id":
                        Guid itemGuid;
                        if (!Guid.TryParse(value, out itemGuid))
                        {
                            throw new InvalidOperationException("Not a valid guid");
                        }

                        var itemId = new ID(itemGuid);
                        return queryable.Where(item => item.TemplateId == itemId);

                    case "name":
                        return queryable.Where(item => item.Name == value);

                    case "path":
                        return queryable.Where(item => item.Path == value);

                    case "templateid":
                        Guid templateGuid;
                        if (!Guid.TryParse(value, out templateGuid))
                        {
                            throw new InvalidOperationException("Not a valid guid");
                        }

                        var templateId = new ID(templateGuid);
                        return queryable.Where(item => item.TemplateId == templateId);

                    case "templatename":
                        return queryable.Where(item => item.TemplateName == value);

                    default:
                        return queryable.Where(item => item[fieldName] == value);

                }
            }

            var n = fieldName.LastIndexOf('[');
            if (n < 0)
            {
                throw new SyntaxErrorException("[ expected");
            }

            var op = fieldName.Mid(n + 1, fieldName.Length - n - 2);
            fieldName = fieldName.Left(n).Trim();

            switch (op)
            {
                case "not":
                    return queryable.Where(item => item[fieldName] != value);

                case "in":
                    var l1 = value.Split('|');
                    return queryable.Where(item => l1.Contains(item[fieldName]));

                case "not in":
                    var l2 = value.Split('|');
                    return queryable.Where(item => !l2.Contains(item[fieldName]));

                case "has":
                    return queryable.Where(item => !string.IsNullOrEmpty(item[fieldName]));
            }

            return queryable;
        }
    }
}
