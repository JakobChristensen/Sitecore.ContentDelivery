// © 2015-2017 by Jakob Christensen. All rights reserved.

using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sitecore.Configuration;
using Sitecore.ContentDelivery.Extensions;
using Sitecore.ContentDelivery.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Web;
using Sitecore.Web.UI;

namespace Sitecore.ContentDelivery.Databases.ItemDatabases
{
    public class ItemDatabase : DatabaseBase
    {
        [NotNull]
        public Database Database { get; protected set; }

        public string RootItemPath { get; protected set; } = string.Empty;

        public override ActionResult AddItem(RequestParameters requestParameters, string itemPath, string templateName)
        {
            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            var n = itemPath.LastIndexOf('/');
            if (n < 0)
            {
                output.WriteError("Parent path not found: " + itemPath);
                return output.ToContentResult();
            }

            var parentPath = itemPath.Left(n);
            var parentItem = Database.GetItem(parentPath);

            if (parentItem == null)
            {
                output.WriteError("Parent item not found: " + parentPath);
                return output.ToContentResult();
            }

            if (!parentItem.Security.CanCreate(Context.User))
            {
                output.WriteError("You do not have permission to create items under item: " + parentItem.Paths.Path);
                return output.ToContentResult();
            }

            var templateItem = Database.GetItem(templateName);
            if (templateItem == null)
            {
                output.WriteError("Template not found: " + templateName);
                return output.ToContentResult();
            }

            var itemName = itemPath.Mid(n + 1);

            try
            {
                var newItem = parentItem.Add(itemName, new TemplateID(templateItem.ID));
                if (newItem != null)
                {
                    output.WriteStartObject("item");
                    WriteItemHeader(output, newItem);
                    output.WriteEndObject();
                }
                else
                {
                    output.WriteError("Failed to create item under " + parentItem.Paths.Path);
                }
            }
            catch (System.Exception ex)
            {
                output.WriteError("Failed to create item under " + parentItem.Paths.Path + ": " + ex.Message);
            }

            return output.ToContentResult();
        }

        public override ActionResult DeleteItems(RequestParameters requestParameters, IEnumerable<string> items)
        {
            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            foreach (var itemPath in items)
            {
                try
                {
                    var item = Database.GetItem(itemPath);
                    if (item == null)
                    {
                        continue;
                    }

                    if (!item.Parent.Security.CanDelete(Context.User))
                    {
                        output.WriteError("You do not have permission to delete item: " + item.Paths.Path);
                        continue;
                    }

                    item.Recycle();
                }
                catch (System.Exception ex)
                {
                    output.WriteError("Failed to delete item: " + ex.Message);
                }
            }

            return output.ToContentResult();
        }

        public override ActionResult GetChildren(RequestParameters requestParameters, string itemName)
        {
            SetContext(requestParameters);

            var items = GetItemsByName(itemName);
            if (!items.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            if (items.Count() > 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Ambiguous, "Ambiguous item name");
            }

            var item = items.First();
            if (item == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            var children = item.Children as IEnumerable<Item>;

            var output = new JsonContentResultWriter(new StringWriter());
            WriteItems(output, requestParameters, children);

            return output.ToContentResult();
        }

        public override ActionResult GetDatabase(RequestParameters requestParameters)
        {
            SetContext(requestParameters);

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            output.WritePropertyString("type", "items");
            output.WritePropertyString("name", DatabaseName);
            output.WritePropertyString("icon16x16", Images.GetThemedImageSource(Database.Icon, ImageDimension.id16x16));
            output.WritePropertyString("icon32x32", Images.GetThemedImageSource(Database.Icon, ImageDimension.id32x32));

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

            output.WriteStartArray("roots");

            ICollection<Item> children = new List<Item>();
            if (string.IsNullOrEmpty(RootItemPath))
            {
                children.Add(Database.GetRootItem());
            }
            else
            {
                var rootItem = Database.GetItem(RootItemPath);
                if (rootItem == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Root Item Path not found");
                }

                foreach (Item child in rootItem.Children)
                {
                    children.Add(child);
                }
            }

            var rootItems = FilterEnumerable(requestParameters, children);

            foreach (var rootItem in rootItems)
            {
                output.WriteStartObject();
                WriteItemHeader(output, rootItem, requestParameters.Flatten == 0);
                WriteItemFields(output, requestParameters, rootItem);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public override ActionResult GetInsertOptions(RequestParameters requestParameters, string itemName)
        {
            SetContext(requestParameters);

            var items = GetItemsByName(itemName).ToList();
            if (!items.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            if (items.Count > 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Ambiguous, "Ambiguous item name");
            }

            var item = items.First();
            if (item == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            if (requestParameters.Version != 0)
            {
                item = item.Database.GetItem(item.ID, item.Language, Version.Parse(requestParameters.Version));
                if (item == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item version not found");
                }
            }

            var branches = new MultilistField(item.Fields[FieldIDs.Branches]);

            var output = new JsonContentResultWriter(new StringWriter());

            WriteItems(output, requestParameters, branches.GetItems());

            return output.ToContentResult();
        }

        public override ActionResult GetItem(RequestParameters requestParameters, string itemName)
        {
            SetContext(requestParameters);

            var items = GetItemsByName(itemName).ToList();
            if (!items.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            if (items.Count > 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Ambiguous, "Ambiguous item name");
            }

            var item = items.First();
            if (item == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found");
            }

            if (requestParameters.Version != 0)
            {
                item = item.Database.GetItem(item.ID, item.Language, Version.Parse(requestParameters.Version));
                if (item == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item version not found");
                }
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            WriteItemHeader(output, item);
            WriteItemFields(output, requestParameters, item);
            WriteItemChildren(output, requestParameters, item, requestParameters.Children);

            return output.ToContentResult();
        }

        public override ActionResult GetItems(RequestParameters requestParameters)
        {
            SetContext(requestParameters);

            List<ID> result;
            IEnumerable<Item> items;
            var index = ContentSearchManager.GetIndex("sitecore_" + Database.Name.ToLowerInvariant() + "_index");
            using (var context = index.CreateSearchContext())
            {
                var queryable = FilterQueryable(context.GetQueryable<SearchResultItem>(), requestParameters);

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
                WriteItemChildren(output, requestParameters, item, requestParameters.Children);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public override ActionResult GetTemplate(RequestParameters requestParameters, string templateName)
        {
            SetContext(requestParameters);

            var templates = GetTemplatesByName(templateName).ToList();
            if (!templates.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Template not found");
            }

            if (templates.Count > 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Ambiguous, "Ambiguous template name");
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
                output.WritePropertyString("id", field.ID.ToString());
                output.WritePropertyString("uri", template.Database.Name + "/" + template.ID + "/" + field.ID);
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

        public override ActionResult GetTemplates(RequestParameters requestParameters)
        {
            SetContext(requestParameters);

            List<ID> result;
            IEnumerable<Item> templates;
            var index = ContentSearchManager.GetIndex("sitecore_" + Database.Name.ToLowerInvariant() + "_index");
            using (var context = index.CreateSearchContext())
            {
                var queryable = FilterQueryable(context.GetQueryable<SearchResultItem>(), requestParameters);

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

        public override void Initialize(IDictionary<string, string> parameters, string currentDirectory, string appDataDirectory)
        {
            base.Initialize(parameters, currentDirectory, appDataDirectory);

            if (parameters.TryGetValue("rootitem", out var rootItem))
            {
                RootItemPath = rootItem;
            }

            parameters.TryGetValue("database", out var databaseName);
            if (string.IsNullOrEmpty(databaseName))
            {
                Log.Error("Missing 'database' attribute", GetType());
                return;
            }

            Database = Factory.GetDatabase(databaseName);
        }

        public override ActionResult SaveItems(RequestParameters requestParameters, Dictionary<string, string> fields)
        {
            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            var fieldList = new List<FieldEditorField>();
            foreach (var pair in fields)
            {
                var key = pair.Key;
                var value = HttpUtility.UrlDecode(pair.Value) ?? string.Empty;

                var parts = key.Split('/');
                if (parts.Length != 5)
                {
                    output.WriteError("Invalid Field Uri: " + key);
                    continue;
                }

                var languageName = parts[2];
                if (languageName == "-")
                {
                    languageName = LanguageManager.DefaultLanguage.Name;
                }

                var versionNumber = parts[3];
                if (versionNumber == "-")
                {
                    versionNumber = "0";
                }

                var databaseName = parts[0];
                var itemId = parts[1];
                var language = Language.Parse(languageName);
                var version = Version.Parse(versionNumber);
                var fieldId = parts[4];

                var field = new FieldEditorField(databaseName, itemId, language, version, fieldId, value);

                fieldList.Add(field);
            }

            var items = new Dictionary<string, Item>();

            foreach (var field in fieldList)
            {
                var key = field.DatabaseName + "/" + field.ItemId + "/" + field.Language + "/" + field.Version;
                if (!items.TryGetValue(key, out var item))
                {
                    var database = Factory.GetDatabase(field.DatabaseName);

                    item = database.GetItem(field.ItemId, field.Language, field.Version);
                    if (item == null)
                    {
                        output.WriteError("Item not found: " + field.ItemId + "/" + field.Language.Name + "/" + field.Version.Number);
                        continue;
                    }

                    if (!item.Security.CanWrite(Context.User))
                    {
                        output.WriteError("You do not have permission to write to this item: " + item.Paths.Path);
                        continue;
                    }

                    items[key] = item;

                    try
                    {
                        item.Editing.BeginEdit();
                    }
                    catch (System.Exception ex)
                    {
                        output.WriteError("An exception occured while saving item: " + item.Paths.Path + "; " + ex.Message);
                        continue;
                    }
                }

                item[field.FieldId] = field.Value;
            }

            foreach (var pair in items)
            {
                pair.Value.Editing.EndEdit();
            }

            return output.ToContentResult();
        }

        [NotNull]
        protected virtual IEnumerable<Item> FilterEnumerable([NotNull] RequestParameters requestParameters, IEnumerable<Item> items)
        {
            if (requestParameters.Flatten > 0)
            {
                items = Flatten(items, requestParameters.Flatten, 0);
            }

            foreach (var pair in requestParameters.Parameters)
            {
                var fieldName = pair.Key;
                var value = pair.Value;
                var op = string.Empty;

                if (fieldName.EndsWith("]", System.StringComparison.OrdinalIgnoreCase))
                {
                    var n = fieldName.LastIndexOf('[');
                    if (n < 0)
                    {
                        throw new SyntaxErrorException("[ expected");
                    }

                    op = fieldName.Mid(n + 1, fieldName.Length - n - 2);
                    fieldName = fieldName.Left(n).Trim();
                }

                switch (op)
                {
                    case "not":
                        items = items.Where(i => GetFieldValue(i, fieldName) != value);
                        break;
                    case "in":
                        var l1 = value.Split('|');
                        items = items.Where(i => l1.Contains(GetFieldValue(i, fieldName)));
                        break;
                    case "not in":
                        var l2 = value.Split('|');
                        items = items.Where(i => !l2.Contains(GetFieldValue(i, fieldName)));
                        break;
                    case "has":
                        items = items.Where(i => !string.IsNullOrEmpty(GetFieldValue(i, fieldName)));
                        break;
                    default:
                        items = items.Where(i => GetFieldValue(i, fieldName) == value);
                        break;
                }
            }

            if (requestParameters.Flatten > 0)
            {
                items = items.OrderBy(i => i.DisplayName);
            }

            return items;

            string GetFieldValue(Item item, string fieldName)
            {
                switch (fieldName.ToLowerInvariant())
                {
                    case "id":
                        return item.ID.ToString();
                    case "name":
                        return item.Name;
                    case "path":
                        return item.Paths.Path;
                    case "templateid":
                        return item.TemplateID.ToString();
                    case "templatename":
                        return item.TemplateName;
                    default:
                        return item[fieldName];
                }
            }
        }

        [NotNull]
        protected virtual IQueryable<SearchResultItem> FilterQueryable(IQueryable<SearchResultItem> queryable, [NotNull] RequestParameters requestParameters)
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
                var op = string.Empty;

                if (fieldName == "query")
                {
                    queryable = queryable.Where(item => item.Content.Contains(value));
                    continue;
                }

                if (fieldName.EndsWith("]", System.StringComparison.OrdinalIgnoreCase))
                {
                    var n = fieldName.LastIndexOf('[');
                    if (n < 0)
                    {
                        throw new SyntaxErrorException("[ expected");
                    }

                    op = fieldName.Mid(n + 1, fieldName.Length - n - 2);
                    fieldName = fieldName.Left(n).Trim();
                }

                queryable = FilterQueryableFieldValue(queryable, fieldName, op, value);
            }

            queryable = queryable.OrderBy(i => i.Name);

            return queryable;
        }

        protected virtual IEnumerable<Item> Flatten([NotNull] IEnumerable<Item> items, int maxLevel, int level)
        {
            foreach (var item in items)
            {
                yield return item;

                if (level + 1 >= maxLevel)
                {
                    continue;
                }

                foreach (var child in Flatten(item.Children, maxLevel, level + 1))
                {
                    yield return child;
                }
            }
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

                yield break;
            }

            if (templateName.IndexOf('/') >= 0)
            {
                if (!templateName.StartsWith("/"))
                {
                    templateName = "/" + templateName;
                }

                yield return Database.GetItem(templateName);

                yield break;
            }

            foreach (var item in Database.GetItemsByName(templateName, TemplateIDs.Template))
            {
                yield return item;
            }
        }

        protected virtual void SetContext([NotNull] RequestParameters requestParameters)
        {
            if (!string.IsNullOrEmpty(requestParameters.Language))
            {
                Context.Language = Language.Parse(requestParameters.Language);
            }
        }

        protected virtual void WriteItemChildren(JsonTextWriter output, RequestParameters request, Item item, int children)
        {
            if (children == 0 || !item.Children.Any())
            {
                return;
            }

            output.WriteStartArray("children");

            foreach (Item child in item.Children)
            {
                output.WriteStartObject();
                WriteItemHeader(output, child);
                WriteItemFields(output, request, child);
                WriteItemChildren(output, request, child, children - 1);
                output.WriteEndObject();
            }

            output.WriteEndArray();
        }

        protected virtual void WriteItemFields(JsonTextWriter output, [NotNull] RequestParameters request, Item item)
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
                else if (!request.Fields.Any(f => string.Equals(f.FieldName, field.Name, System.StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var value = field.Value;

                var fieldInfo = request.Fields.FirstOrDefault(f => string.Equals(f.FieldName, field.Name, System.StringComparison.OrdinalIgnoreCase)) ?? new FieldInfo(field.Name, string.Empty);
                foreach (var formatter in ContentDeliveryManager.FieldValueFormatters.OrderBy(f => f.Priority))
                {
                    if (!formatter.TryFormat(field, fieldInfo, value, out var formattedValue))
                    {
                        continue;
                    }

                    value = formattedValue;
                    break;
                }

                if (!request.IncludeEmptyFields && string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (includeFieldInfo)
                {
                    output.WriteStartObject();
                    output.WritePropertyString("id", field.ID.ToString());
                    output.WritePropertyString("uri", item.Database.Name + "/" + item.ID + "/" + item.Language.Name + "/" + item.Version.Number + "/" + field.ID);
                    output.WritePropertyString("name", field.Name);
                    output.WritePropertyString("displayName", field.DisplayName);
                    output.WritePropertyString("value", value);
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

        protected virtual void WriteItemHeader([NotNull] JsonTextWriter output, [NotNull] Item item, bool isExpandable = true)
        {
            output.WritePropertyString("id", item.ID.ToString());
            output.WritePropertyString("uri", item.Database.Name + "/" + item.ID + "/" + item.Language.Name + "/" + item.Version.Number);
            output.WritePropertyString("name", item.Name);
            output.WritePropertyString("displayName", item.DisplayName);
            output.WritePropertyString("database", item.Database.Name);
            output.WritePropertyString("icon16x16", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));
            output.WritePropertyString("icon32x32", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id32x32));
            output.WritePropertyString("path", item.Paths.Path);
            output.WritePropertyString("longPath", item.Paths.LongID);
            output.WritePropertyString("templateId", item.TemplateID.ToString());
            output.WritePropertyString("templateName", item.TemplateName);
            output.WritePropertyString("childCount", isExpandable ? item.Children.Count : 0);

            if (item.Paths.IsMediaItem)
            {
                output.WritePropertyString("mediaurl", WebUtil.GetFullUrl(MediaManager.GetMediaUrl(new MediaItem(item))));
            }
        }

        protected virtual void WriteItems([NotNull] JsonContentResultWriter output, [NotNull] RequestParameters requestParameters, IEnumerable<Item> items)
        {
            items = FilterEnumerable(requestParameters, items);

            var count = items.Count();

            if (requestParameters.Skip > 0)
            {
                items = items.Skip(requestParameters.Skip);
            }

            if (requestParameters.Take > 0)
            {
                items = items.Take(requestParameters.Take);
            }

            WriteMetaData(output);

            output.WritePropertyString("count", count);
            output.WritePropertyString("skip", requestParameters.Skip);
            output.WritePropertyString("take", requestParameters.Take);

            output.WriteStartArray("items");

            foreach (var child in items)
            {
                output.WriteStartObject();
                WriteItemHeader(output, child, requestParameters.Flatten == 0);
                WriteItemFields(output, requestParameters, child);
                WriteItemChildren(output, requestParameters, child, requestParameters.Children);
                output.WriteEndObject();
            }

            output.WriteEndArray();
        }

        protected virtual void WriteMetaData([NotNull] JsonTextWriter output)
        {
            output.WriteStartObject("metadata");
            output.WritePropertyString("version", "1.0.0");
            output.WritePropertyString("user", Context.GetUserName());
            output.WritePropertyString("language", Context.Language.Name);
            output.WritePropertyString("host", WebUtil.GetServerUrl());
            output.WriteEndObject();
        }

        [NotNull]
        private IQueryable<SearchResultItem> FilterQueryableFieldValue([NotNull] IQueryable<SearchResultItem> queryable, [NotNull] string fieldName, [NotNull] string op, [NotNull] string value)
        {
            switch (fieldName.ToLowerInvariant())
            {
                case "id":
                    switch (op)
                    {
                        case "not":
                            var itemId = ID.Parse(value);
                            return queryable.Where(item => item.ItemId != itemId);
                        case "in":
                            var l1 = value.Split('|');
                            return queryable.Where(item => l1.Contains(item.ItemId.ToString()));
                        case "not in":
                            var l2 = value.Split('|');
                            return queryable.Where(item => !l2.Contains(item.ItemId.ToString()));
                        case "has":
                            return queryable;
                        default:
                            var itemId2 = ID.Parse(value);
                            return queryable.Where(item => item.ItemId == itemId2);
                    }
                case "name":
                    switch (op)
                    {
                        case "not":
                            return queryable.Where(item => item.Name != value);
                        case "in":
                            var l1 = value.Split('|');
                            return queryable.Where(item => l1.Contains(item.Name));
                        case "not in":
                            var l2 = value.Split('|');
                            return queryable.Where(item => !l2.Contains(item.Name));
                        case "has":
                            return queryable;
                        default:
                            return queryable.Where(item => item.Name == value);
                    }
                case "path":
                    switch (op)
                    {
                        case "not":
                            return queryable.Where(item => item.Path != value);
                        case "in":
                            var l1 = value.Split('|');
                            return queryable.Where(item => l1.Contains(item.Path));
                        case "not in":
                            var l2 = value.Split('|');
                            return queryable.Where(item => !l2.Contains(item.Path));
                        case "has":
                            return queryable;
                        default:
                            return queryable.Where(item => item.Path == value);
                    }
                case "templateid":
                    switch (op)
                    {
                        case "not":
                            var templateId = ID.Parse(value);
                            return queryable.Where(item => item.TemplateId != templateId);
                        case "in":
                            var l1 = value.Split('|');
                            return queryable.Where(item => l1.Contains(item.TemplateId.ToString()));
                        case "not in":
                            var l2 = value.Split('|');
                            return queryable.Where(item => !l2.Contains(item.TemplateId.ToString()));
                        case "has":
                            return queryable;
                        default:
                            var templateId2 = ID.Parse(value);
                            return queryable.Where(item => item.TemplateId == templateId2);
                    }
                case "templatename":
                    switch (op)
                    {
                        case "not":
                            return queryable.Where(item => item.TemplateName != value);
                        case "in":
                            var l1 = value.Split('|');
                            return queryable.Where(item => l1.Contains(item.TemplateName));
                        case "not in":
                            var l2 = value.Split('|');
                            return queryable.Where(item => !l2.Contains(item.TemplateName));
                        case "has":
                            return queryable;
                        default:
                            return queryable.Where(item => item.TemplateName == value);
                    }
                default:
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
                        default:
                            return queryable.Where(item => item[fieldName] == value);
                    }
            }
        }

        private class FieldEditorField
        {
            public FieldEditorField([NotNull] string databaseName, [NotNull] string itemId, [NotNull] Language language, [NotNull] Version version, [NotNull] string fieldId, [NotNull] string value)
            {
                DatabaseName = databaseName;
                ItemId = itemId;
                Language = language;
                Version = version;
                FieldId = fieldId;
                Value = value;
            }

            [NotNull]
            public string DatabaseName { get; }

            [NotNull]
            public string FieldId { get; }

            [NotNull]
            public string ItemId { get; }

            [NotNull]
            public Language Language { get; }

            [NotNull]
            public string Value { get; }

            [NotNull]
            public Version Version { get; }
        }
    }
}
