// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sitecore.Configuration;
using Sitecore.ContentDelivery.Extensions;
using Sitecore.ContentDelivery.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.ContentDelivery.DataStores.ItemDataStores
{
    public class ItemDataStore : DataStoreBase
    {
        public ItemDataStore(string databaseName) : base(databaseName)
        {
            Database = Factory.GetDatabase(databaseName);
        }

        [NotNull]
        public Database Database { get; }

        [NotNull]
        public static ICollection<IItemDataStoreFilter> Filters { get; } = new List<IItemDataStoreFilter>();

        [NotNull]
        public static ICollection<IItemDataStoreWriter> Writers { get; } = new List<IItemDataStoreWriter>();

        public override ActionResult GetChildren(RequestParameters requestParameters, string itemName)
        {
            var item = Database.GetItem(itemName);
            if (item == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            var children = item.Children as IEnumerable<Item>;

            foreach (var pair in requestParameters.Parameters)
            {
                var fieldName = pair.Key;
                var value = pair.Value;

                foreach (var filter in Filters.OrderBy(f => f.Priority))
                {
                    children = filter.Filter(children, fieldName, value);
                }
            }

            children = children.ToList();
            var count = children.Count();

            if (requestParameters.Skip > 0)
            {
                children = children.Skip(requestParameters.Skip);
            }

            if (requestParameters.Take > 0)
            {
                children = children.Take(requestParameters.Take);
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            output.WritePropertyString("count", count);
            output.WritePropertyString("skip", requestParameters.Skip);
            output.WritePropertyString("take", requestParameters.Take);

            output.WriteStartArray("items");

            foreach (var child in children)
            {
                output.WriteStartObject();
                WriteItemHeader(output, child);
                WriteItemFields(output, requestParameters, child);
                WriteItemChildren(output, requestParameters, child, requestParameters.Levels);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public override ActionResult GetDataStore(RequestParameters requestParameters)
        {
            var rootItem = Database.GetRootItem();
            if (rootItem == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Root item not found");
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            output.WritePropertyString("type", "items");
            output.WritePropertyString("name", DataStoreName);
            output.WritePropertyString("smallIcon", Images.GetThemedImageSource(Database.Icon, ImageDimension.id16x16));
            output.WritePropertyString("largeIcon", Images.GetThemedImageSource(Database.Icon, ImageDimension.id32x32));

            output.WriteStartArray("languages");
            foreach (var language in Database.GetLanguages().OrderBy(l => l.Name))
            {
                output.WriteStartObject();
                output.WritePropertyString("name", language.Name);
                output.WritePropertyString("displayName", language.GetDisplayName());
                output.WritePropertyString("cultureName", language.CultureInfo.Name);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            output.WriteStartObject("root");
            WriteItemHeader(output, rootItem);
            WriteItemFields(output, requestParameters, rootItem);
            output.WriteEndObject();

            return output.ToContentResult();
        }

        public override ActionResult GetItem(RequestParameters requestParameters, string itemName)
        {
            var items = GetItemsByName(itemName).ToList();
            if (!items.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            if (items.Count > 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Ambiguous, "Ambigeous item name");
            }

            var item = items.First();
            if (item == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            WriteItemHeader(output, item);
            WriteItemFields(output, requestParameters, item);
            WriteItemChildren(output, requestParameters, item, requestParameters.Levels);

            return output.ToContentResult();
        }

        public override ActionResult GetItems(RequestParameters requestParameters)
        {
            List<ID> result;
            IEnumerable<Item> items;
            var index = ContentSearchManager.GetIndex("sitecore_" + Database.Name.ToLowerInvariant() + "_index");
            using (var context = index.CreateSearchContext())
            {
                var queryable = Filter(context.GetQueryable<SearchResultItem>(), requestParameters);

                result = queryable.Where(i => i.Name != "$name" && i.Name != "__Standard Values").Select(item => item.ItemId).ToList().Distinct().ToList();

                items = result.Select(Database.GetItem).Where(i => i != null).OrderBy(t => t.Name).ThenBy(t => t.Paths.Path);

                if (requestParameters.Skip > 0)
                {
                    items = items.Skip(requestParameters.Skip);
                }

                if (requestParameters.Take > 0)
                {
                    items = items.Take(requestParameters.Take);
                }

                items = items.ToList();
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            output.WritePropertyString("count", result.Count);
            output.WritePropertyString("skip", requestParameters.Skip);
            output.WritePropertyString("take", requestParameters.Take);

            output.WriteStartArray("items");

            foreach (var item in items)
            {
                output.WriteStartObject();
                WriteItemHeader(output, item);
                WriteItemFields(output, requestParameters, item);
                WriteItemChildren(output, requestParameters, item, requestParameters.Levels);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public override ActionResult GetTemplate(RequestParameters requestParameters, string templateName)
        {
            var templates = GetTemplatesByName(templateName).ToList();
            if (!templates.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Template not found");
            }

            if (templates.Count > 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Ambiguous, "Ambigeous template name");
            }

            var templateItem = templates.First();
            if (templateItem == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Template not found");
            }

            var template = new TemplateItem(templateItem);
            if (template.InnerItem.TemplateID != TemplateIDs.Template)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item found, but it is not a template");
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            WriteItemHeader(output, template);

            output.WriteStartArray("fields");

            foreach (var field in template.Fields.OrderBy(f => f.Section.Sortorder).ThenBy(f => f.Section.Name).ThenBy(f => f.Sortorder).ThenBy(f => f.Name))
            {
                var isOwnField = field.InnerItem.Parent.Parent == template.InnerItem;
                var isSystemField = field.Name.StartsWith("__");

                if (isSystemField && !requestParameters.IncludeSystemFields)
                {
                    continue;
                }

                output.WriteStartObject();
                Writers.OrderBy(w => w.Priority).ForEach(w => w.WriteTemplateField(output, field, isOwnField, isSystemField));
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public override ActionResult GetTemplates(RequestParameters requestParameters)
        {
            List<ID> result;
            IEnumerable<Item> templates;
            var index = ContentSearchManager.GetIndex("sitecore_" + Database.Name.ToLowerInvariant() + "_index");
            using (var context = index.CreateSearchContext())
            {
                var queryable = Filter(context.GetQueryable<SearchResultItem>(), requestParameters);

                result = queryable.Where(t => t.TemplateId == TemplateIDs.Template).Select(item => item.ItemId).ToList().Distinct().ToList();

                templates = result.Select(Database.GetItem).Where(i => i != null).OrderBy(t => t.Name).ThenBy(t => t.Paths.Path);

                if (requestParameters.Skip > 0)
                {
                    templates = templates.Skip(requestParameters.Skip);
                }

                if (requestParameters.Take > 0)
                {
                    templates = templates.Take(requestParameters.Take);
                }

                templates = templates.ToList();
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            output.WritePropertyString("count", result.Count);
            output.WritePropertyString("skip", requestParameters.Skip);
            output.WritePropertyString("take", requestParameters.Take);

            output.WriteStartArray("templates");

            foreach (var template in templates)
            {
                output.WriteStartObject();
                WriteItemHeader(output, template);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public static void RegisterFilter([NotNull] IItemDataStoreFilter filter)
        {
            Filters.Add(filter);
        }

        public static void RegisterWriter([NotNull] IItemDataStoreWriter writer)
        {
            Writers.Add(writer);
        }

        public static void UnregisterFilter([NotNull] IItemDataStoreFilter filter)
        {
            Filters.Remove(filter);
        }

        public static void UnregisterWriter([NotNull] IItemDataStoreWriter writer)
        {
            Writers.Remove(writer);
        }

        protected virtual IQueryable<SearchResultItem> Filter(IQueryable<SearchResultItem> queryable, RequestParameters requestParameters)
        {
            if (!string.IsNullOrEmpty(requestParameters.Path))
            {
                var pathItem = Database.GetItem(requestParameters.Path);
                if (pathItem != null)
                {
                    queryable = queryable.Where(i => i.Paths.Contains(pathItem.ID));
                }
            }

            foreach (var pair in requestParameters.Parameters)
            {
                var fieldName = pair.Key;
                var value = pair.Value;

                foreach (var filter in Filters.OrderBy(f => f.Priority))
                {
                    queryable = filter.Filter(queryable, fieldName, value);
                }
            }

            return queryable;
        }

        [NotNull]
        protected virtual IEnumerable<Item> GetItemsByName(string itemName)
        {
            if (ID.IsID(itemName))
            {
                yield return Database.GetItem(itemName);
            }

            if (itemName.IndexOf('/') >= 0)
            {
                if (!itemName.StartsWith("/"))
                {
                    itemName = "/" + itemName;
                }

                yield return Database.GetItem(itemName);
            }

            foreach (var item in Database.GetItemsByName(itemName))
            {
                yield return item;
            }
        }

        [NotNull]
        protected virtual IEnumerable<Item> GetTemplatesByName(string templateName)
        {
            if (ID.IsID(templateName))
            {
                yield return Database.GetItem(templateName);
            }

            if (templateName.IndexOf('/') >= 0)
            {
                if (!templateName.StartsWith("/"))
                {
                    templateName = "/" + templateName;
                }

                yield return Database.GetItem(templateName);
            }

            foreach (var item in Database.GetItemsByName(templateName, TemplateIDs.Template))
            {
                yield return item;
            }
        }

        protected virtual void WriteItemChildren(JsonTextWriter output, RequestParameters request, Item item, int levels)
        {
            if (levels == 0 || !item.Children.Any())
            {
                return;
            }

            output.WriteStartArray("children");

            foreach (Item child in item.Children)
            {
                output.WriteStartObject();
                WriteItemHeader(output, child);
                WriteItemFields(output, request, child);
                WriteItemChildren(output, request, child, levels - 1);
                output.WriteEndObject();
            }

            output.WriteEndArray();
        }

        protected virtual void WriteItemFields(JsonTextWriter output, RequestParameters request, Item item)
        {
            if (!request.Fields.Any())
            {
                return;
            }

            var includeAllFields = request.Fields.Count == 1 && request.Fields.First().FieldName == "*";
            var includeSystemFields = request.IncludeSystemFields;
            var includeFieldInfo = request.IncludeFieldInfo;

            item.Fields.ReadAll();

            if (includeFieldInfo)
            {
                output.WriteStartArray("fields");
            }
            else
            {
                output.WriteStartObject("fields");
            }

            foreach (var field in item.Fields.OrderBy(f => f.SectionSortorder).ThenBy(f => f.Section).ThenBy(f => f.Sortorder).ThenBy(f => f.Name))
            {
                if (includeAllFields)
                {
                    if (!includeSystemFields && field.Name.StartsWith("__"))
                    {
                        continue;
                    }
                }
                else if (!request.Fields.Any(f => string.Equals(f.FieldName, field.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var value = field.Value;

                if (!includeAllFields)
                {
                    var fieldDescriptor = request.Fields.First(f => string.Equals(f.FieldName, field.Name, StringComparison.OrdinalIgnoreCase));

                    foreach (var formatter in ContentDeliveryManager.FieldValueFormatters.OrderBy(f => f.Priority))
                    {
                        string formattedValue;
                        if (!formatter.TryFormat(fieldDescriptor, value, out formattedValue))
                        {
                            continue;
                        }

                        value = formattedValue;
                        break;
                    }
                }

                if (includeFieldInfo)
                {
                    output.WriteStartObject();
                    Writers.OrderBy(w => w.Priority).ForEach(w => w.WriteItemField(output, field, value));
                    output.WriteEndObject();
                }
                else
                {
                    output.WritePropertyString(field.Name, value);
                }
            }

            if (includeFieldInfo)
            {
                output.WriteEndArray();
            }
            else
            {
                output.WriteEndObject();
            }
        }

        protected virtual void WriteItemHeader(JsonTextWriter output, Item item)
        {
            Writers.OrderBy(w => w.Priority).ForEach(w => w.WriteItemHeader(output, item));
        }

        protected virtual void WriteMetaData(JsonTextWriter output)
        {
            output.WriteStartObject("metadata");
            Writers.OrderBy(w => w.Priority).ForEach(w => w.WriteMetaData(output));
            output.WriteEndObject();
        }
    }
}
