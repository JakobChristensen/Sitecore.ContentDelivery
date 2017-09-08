// © 2015-2017 by Jakob Christensen. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sitecore.ContentDelivery.Extensions;
using Sitecore.ContentDelivery.Web;

namespace Sitecore.ContentDelivery.Databases.FileDatabases
{
    public abstract class FileDatabase : IDatabase
    {
        protected FileDatabase(string fileName)
        {
            FileName = fileName;
            DatabaseName = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
        }

        public string DatabaseName { get; }

        public string FileName { get; set; }

        [NotNull]
        public ICollection<FileDatabaseItem> Items { get; } = new List<FileDatabaseItem>();

        public ActionResult AddItem(RequestParameters requestParameters, string itemPath, string templateName) => new HttpStatusCodeResult(HttpStatusCode.NotImplemented);

        public ActionResult DeleteItems(RequestParameters requestParameters, IEnumerable<string> items) => new HttpStatusCodeResult(HttpStatusCode.NotImplemented);

        public virtual ActionResult GetChildren(RequestParameters requestParameters, string itemName)
        {
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

            var children = item.Children;

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
                WriteItemChildren(output, requestParameters, child, requestParameters.Children);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public virtual ActionResult GetDatabase(RequestParameters requestParameters)
        {
            var rootItem = Items.FirstOrDefault(i => i.Parent == null);
            if (rootItem == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Root item not found");
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            output.WritePropertyString("type", "items");
            output.WritePropertyString("name", DatabaseName);
            output.WritePropertyString("icon16x16", string.Empty);
            output.WritePropertyString("icon32x32", string.Empty);

            output.WriteStartArray("languages");
            foreach (var language in Items.First(i => i.Path == "/sitecore/system/Languages").Children.OrderBy(l => l.Name))
            {
                output.WriteStartObject();
                output.WritePropertyString("name", language.Name);
                output.WritePropertyString("displayName", language.Name);
                output.WritePropertyString("cultureName", language["Iso"]);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            output.WriteStartObject("root");
            WriteItemHeader(output, rootItem);
            WriteItemFields(output, requestParameters, rootItem);
            output.WriteEndObject();

            return output.ToContentResult();
        }

        public virtual ActionResult GetItem(RequestParameters requestParameters, string itemName)
        {
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

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            WriteItemHeader(output, item);
            WriteItemFields(output, requestParameters, item);
            WriteItemChildren(output, requestParameters, item, requestParameters.Children);

            return output.ToContentResult();
        }

        public virtual ActionResult GetItems([NotNull] RequestParameters requestParameters)
        {
            var result = Filter(Items.AsQueryable(), requestParameters).Where(i => i.Name != "$name" && i.Name != "__Standard Values").ToList();
            var items = result.OrderBy(t => t.Name).ThenBy(i => i.Path) as IEnumerable<FileDatabaseItem>;

            if (requestParameters.Skip > 0)
            {
                items = items.Skip(requestParameters.Skip);
            }

            if (requestParameters.Take > 0)
            {
                items = items.Take(requestParameters.Take);
            }

            items = items.ToList();

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

        public virtual ActionResult GetTemplate(RequestParameters requestParameters, string templateName)
        {
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

            if (templateItem.TemplateId != TemplateIDs.Template.Guid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item found, but it is not a template");
            }

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            WriteItemHeader(output, templateItem);

            output.WriteStartArray("fields");

            foreach (var field in templateItem.Children.SelectMany(section => section.Children))
            {
                var isSystemField = field.Name.StartsWith("__");

                if (isSystemField && !requestParameters.IncludeSystemFields)
                {
                    continue;
                }

                output.WriteStartObject();
                output.WritePropertyString("id", field.Id.ToString("B").ToUpperInvariant());
                output.WritePropertyString("uri", DatabaseName + "/" + templateItem.Id + "/" + field.Id.ToString("B").ToUpperInvariant());
                output.WritePropertyString("name", field.Name);
                output.WritePropertyString("displayName", field.DisplayName);
                output.WritePropertyString("type", field["Type"]);
                output.WritePropertyString("source", field["Source"]);
                output.WritePropertyString("sharing", field["Shared"] == "1" ? "shared" : field["Unversioned"] == "1" ? "unversioned" : "versioned");
                output.WritePropertyString("section", field.Parent?.Name ?? string.Empty);
                output.WritePropertyString("kind", "own");
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public virtual ActionResult GetTemplates([NotNull] RequestParameters requestParameters)
        {
            var result = Filter(Items.AsQueryable(), requestParameters).Where(t => t.TemplateId == TemplateIDs.Template.Guid).Distinct().ToList();
            var items = result.OrderBy(t => t.Name).ThenBy(i => i.Path) as IEnumerable<FileDatabaseItem>;

            if (requestParameters.Skip > 0)
            {
                items = items.Skip(requestParameters.Skip);
            }

            if (requestParameters.Take > 0)
            {
                items = items.Take(requestParameters.Take);
            }

            items = items.ToList();

            var output = new JsonContentResultWriter(new StringWriter());
            WriteMetaData(output);

            output.WritePropertyString("count", result.Count);
            output.WritePropertyString("skip", requestParameters.Skip);
            output.WritePropertyString("take", requestParameters.Take);

            output.WriteStartArray("templates");

            foreach (var item in items)
            {
                output.WriteStartObject();
                WriteItemHeader(output, item);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        public virtual ActionResult SaveItems(RequestParameters requestParameters, Dictionary<string, string> fields) => new HttpStatusCodeResult(HttpStatusCode.NotImplemented);

        [NotNull]
        protected virtual IQueryable<FileDatabaseItem> Filter([NotNull] IQueryable<FileDatabaseItem> queryable, [NotNull] RequestParameters requestParameters)
        {
            if (!string.IsNullOrEmpty(requestParameters.Path))
            {
                queryable = queryable.Where(i => i.Path.StartsWith(requestParameters.Path));
            }

            foreach (var pair in requestParameters.Parameters)
            {
                var fieldName = pair.Key;
                var value = pair.Value;

                if (!fieldName.EndsWith("]"))
                {
                    queryable = queryable.Where(i => i[fieldName] == value);
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
                        queryable = queryable.Where(i => i[fieldName] != value);
                        break;

                    case "in":
                        var l1 = value.Split('|');
                        queryable = queryable.Where(i => l1.Contains(i[fieldName]));
                        break;

                    case "not in":
                        var l2 = value.Split('|');
                        queryable = queryable.Where(i => !l2.Contains(i[fieldName]));
                        break;

                    case "has":
                        queryable = queryable.Where(t => !string.IsNullOrEmpty(t[fieldName]));
                        break;
                }
            }

            return queryable;
        }

        [NotNull]
        protected virtual IEnumerable<FileDatabaseItem> GetItemsByName(string itemName)
        {
            if (itemName.StartsWith("{"))
            {
                if (!Guid.TryParse(itemName, out var guid))
                {
                    throw new InvalidOperationException("ID is not valid");
                }

                yield return Items.FirstOrDefault(i => i.Id == guid);

                yield break;
            }

            if (itemName.IndexOf('/') >= 0)
            {
                if (!itemName.StartsWith("/"))
                {
                    itemName = "/" + itemName;
                }

                yield return Items.FirstOrDefault(i => i.Path == itemName);

                yield break;
            }

            foreach (var item in Items.Where(i => i.Name == itemName))
            {
                yield return item;
            }
        }

        [NotNull]
        protected virtual IEnumerable<FileDatabaseItem> GetTemplatesByName(string templateName)
        {
            if (templateName.StartsWith("{"))
            {
                if (!Guid.TryParse(templateName, out var guid))
                {
                    throw new InvalidOperationException("ID is not valid");
                }

                yield return Items.FirstOrDefault(i => i.Id == guid);

                yield break;
            }

            if (templateName.IndexOf('/') >= 0)
            {
                if (!templateName.StartsWith("/"))
                {
                    templateName = "/" + templateName;
                }

                yield return Items.FirstOrDefault(i => i.Path == templateName);

                yield break;
            }

            foreach (var item in Items.Where(i => i.Name == templateName && i.TemplateId == TemplateIDs.Template.Guid))
            {
                yield return item;
            }
        }

        protected virtual void WriteItemChildren(JsonTextWriter output, RequestParameters request, FileDatabaseItem item, int children)
        {
            if (children == 0 || !item.Children.Any())
            {
                return;
            }

            output.WriteStartArray("children");

            foreach (var child in item.Children)
            {
                output.WriteStartObject();
                WriteItemHeader(output, child);
                WriteItemFields(output, request, child);
                WriteItemChildren(output, request, child, children - 1);
                output.WriteEndObject();
            }

            output.WriteEndArray();
        }

        protected virtual void WriteItemFields(JsonTextWriter output, [NotNull] RequestParameters request, FileDatabaseItem item)
        {
            if (!request.Fields.Any())
            {
                return;
            }

            var includeAllFields = request.Fields.Count == 1 && request.Fields.First().FieldName == "*";
            var includeSystemFields = request.IncludeSystemFields;
            var includeFieldInfo = request.IncludeFieldInfo;

            if (includeFieldInfo)
            {
                output.WriteStartArray("fields");
            }
            else
            {
                output.WriteStartObject("fields");
            }

            foreach (var field in item.Fields)
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

                if (!request.IncludeEmptyFields && string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (includeFieldInfo)
                {
                    output.WriteStartObject();
                    output.WritePropertyString("id", field.Id.ToString("B").ToUpperInvariant());
                    output.WritePropertyString("uri", DatabaseName + "/" + item.Id.ToString("B").ToUpperInvariant() + "/en/1/" + field.Id.ToString("B").ToUpperInvariant());
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

        protected virtual void WriteItemHeader([NotNull] JsonTextWriter output, [NotNull] FileDatabaseItem item)
        {
            output.WritePropertyString("id", item.Id.ToString("B").ToUpperInvariant());
            output.WritePropertyString("uri", DatabaseName + "/" + item.Id.ToString("B").ToUpperInvariant() + "/en/1/");
            output.WritePropertyString("name", item.Name);
            output.WritePropertyString("displayName", item.DisplayName);
            output.WritePropertyString("database", DatabaseName);
            output.WritePropertyString("icon16x16", item.Icon16X16);
            output.WritePropertyString("icon32x32", item.Icon32X32);
            output.WritePropertyString("path", item.Path);
            output.WritePropertyString("templateId", item.TemplateId.ToString("B").ToUpperInvariant());
            output.WritePropertyString("templateName", item.Template);
            output.WritePropertyString("childCount", item.ChildCount);

            if (item.Path.StartsWith("/sitecore/media library/"))
            {
                output.WritePropertyString("mediaurl", item.MediaUrl);
            }
        }

        protected virtual void WriteMetaData([NotNull] JsonTextWriter output)
        {
            output.WriteStartObject("metadata");
            output.WritePropertyString("version", "1");
            output.WritePropertyString("user", "sitecore\\admin");
            output.WritePropertyString("language", "en");
            output.WriteEndObject();
        }
    }
}
