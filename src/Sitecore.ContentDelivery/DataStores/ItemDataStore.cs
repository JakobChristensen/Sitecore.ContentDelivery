// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sitecore.Configuration;
using Sitecore.ContentDelivery.Extensions;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.ContentDelivery.DataStores
{
    public class ItemDataStore : DataStoreBase
    {
        public ItemDataStore(string databaseName) : base(databaseName)
        {
            Database = Factory.GetDatabase(databaseName);
        }

        [NotNull]
        public Database Database { get; }

        public override ActionResult GetChildren(string itemName)
        {
            var item = Database.GetItem(itemName);
            if (item == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            var requestParameters = new RequestParameters();

            var children = item.Children as IEnumerable<Item>;

            foreach (var pair in requestParameters.Parameters)
            {
                var fieldName = pair.Key;
                var value = pair.Value;

                if (!fieldName.EndsWith("]"))
                {
                    children = children.Where(i => i[fieldName] == value);
                    continue;
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
                        children = children.Where(i => i[fieldName] != value);
                        break;

                    case "in":
                        var l1 = value.Split('|');
                        children = children.Where(i => l1.Contains(i[fieldName]));
                        break;

                    case "not in":
                        var l2 = value.Split('|');
                        children = children.Where(i => !l2.Contains(i[fieldName]));
                        break;

                    case "has":
                        children = children.Where(t => !string.IsNullOrEmpty(t[fieldName]));
                        break;

                    default:
                        throw new InvalidOperationException("Unknown operator: " + op);
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

        public override ActionResult GetDataStore()
        {
            var requestParameters = new RequestParameters();

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

        public override ActionResult GetItem(string itemName)
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

            var requestParameters = new RequestParameters();

            var item = items.First();

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            WriteItemHeader(output, item);
            WriteItemFields(output, requestParameters, item);
            WriteItemChildren(output, requestParameters, item, requestParameters.Levels);

            return output.ToContentResult();
        }

        public override ActionResult GetItems()
        {
            var requestParameters = new RequestParameters();

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

        public override ActionResult GetTemplate(string templateName)
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

            var template = new TemplateItem(templates.First());
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
                var isSystemField = field.InnerItem.Parent.Parent.Name.StartsWith("__");

                output.WriteStartObject();

                output.WritePropertyString("id", field.ID.ToString());
                output.WritePropertyString("name", field.Name);
                output.WritePropertyString("displayName", field.DisplayName);
                output.WritePropertyString("type", field.Type);
                output.WritePropertyString("source", field.Source);
                output.WritePropertyString("sharing", field.IsShared ? "shared" : field.IsUnversioned ? "unversioned" : "versioned");
                output.WritePropertyString("section", field.Section.Name);
                output.WritePropertyString("kind", isOwnField ? "own" : isSystemField ? "system" : "inherited");

                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public override ActionResult GetTemplates()
        {
            var requestParameters = new RequestParameters();

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

                if (fieldName == "query")
                {
                    queryable = queryable.Where(item => item.Content.Contains(value));
                    continue;
                }

                if (!fieldName.EndsWith("]"))
                {
                    queryable = queryable.Where(item => item[fieldName] == value);
                    continue;
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
                        queryable = queryable.Where(item => item[fieldName] != value);
                        break;

                    case "in":
                        var l1 = value.Split('|');
                        queryable = queryable.Where(item => l1.Contains(item[fieldName]));
                        break;

                    case "not in":
                        var l2 = value.Split('|');
                        queryable = queryable.Where(item => !l2.Contains(item[fieldName]));
                        break;

                    case "has":
                        queryable = queryable.Where(item => !string.IsNullOrEmpty(item[fieldName]));
                        break;

                    default:
                        throw new InvalidOperationException("Unknown operator: " + op);
                }
            }

            return queryable;
        }

        [NotNull]
        protected virtual IEnumerable<Item> GetItemsByName(string template)
        {
            if (ID.IsID(template))
            {
                yield return Database.GetItem(template);
            }

            if (template.IndexOf('/') >= 0)
            {
                yield return Database.GetItem(template);
            }

            foreach (var item in Database.GetItemsByName(template))
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
            if (!request.FieldNames.Any())
            {
                return;
            }

            var includeAllFields = request.FieldNames.Count == 1 && request.FieldNames.First() == "*";
            var includeSystemFields = request.IncludeSystemFields;

            item.Fields.ReadAll();

            output.WriteStartArray("fields");

            foreach (var field in item.Fields.OrderBy(f => f.SectionSortorder).ThenBy(f => f.Section).ThenBy(f => f.Sortorder).ThenBy(f => f.Name))
            {
                if (includeAllFields)
                {
                    if (!includeSystemFields && field.Name.StartsWith("__"))
                    {
                        continue;
                    }
                }
                else if (!request.FieldNames.Any(f => string.Equals(f, field.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                output.WriteStartObject();
                output.WritePropertyString("id", field.ID.ToString());
                output.WritePropertyString("name", field.Name);
                output.WritePropertyString("displayName", field.DisplayName);
                output.WritePropertyString("value", field.Value);
                output.WriteEndObject();
            }

            output.WriteEndArray();
        }

        protected virtual void WriteItemHeader(JsonTextWriter output, Item item)
        {
            output.WritePropertyString("id", item.ID.ToString());
            output.WritePropertyString("name", item.Name);
            output.WritePropertyString("displayName", item.DisplayName);
            output.WritePropertyString("database", item.Database.Name);
            output.WritePropertyString("dataStore", DataStoreName);
            output.WritePropertyString("smallIcon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));
            output.WritePropertyString("largeIcon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id32x32));
            output.WritePropertyString("path", item.Paths.Path);
            output.WritePropertyString("templateId", item.TemplateID.ToString());
            output.WritePropertyString("templateName", item.TemplateName);
            output.WritePropertyString("childCount", item.Children.Count);
        }

        protected virtual void WriteMetaData(JsonTextWriter output)
        {
            output.WriteStartObject("metadata");

            output.WritePropertyString("version", "1");
            output.WritePropertyString("user", Context.GetUserName());
            output.WritePropertyString("language", Context.Language.Name);

            output.WriteEndObject();
        }
    }
}
