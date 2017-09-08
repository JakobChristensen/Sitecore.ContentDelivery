// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sitecore.ContentDelivery.Extensions;

namespace Sitecore.ContentDelivery.Databases.FileDatabases
{
    public class JsonFileDatabase : FileDatabase
    {
        public JsonFileDatabase(string fileName) : base(fileName)
        {
            var content = File.ReadAllText(FileName);

            var root = JObject.Parse(content);

            ReadItem(root, null);
        }

        protected void ReadItem(JObject jobject, [CanBeNull] FileDatabaseItem parent)
        {
            var itemName = jobject.GetPropertyValue("name");
            var itemDisplayName = jobject.GetPropertyValue("displayName");
            var icon16X16 = jobject.GetPropertyValue("icon16x16");
            var icon32X32 = jobject.GetPropertyValue("icon32x32");
            var template = jobject.GetPropertyValue("templateName");
            var path = jobject.GetPropertyValue("path");
            var mediaUrl = jobject.GetPropertyValue("mediaUrl");

            var itemIdString = jobject.GetPropertyValue("id");
            Guid itemId;
            if (!Guid.TryParse(itemIdString, out itemId))
            {
                throw new InvalidOperationException("id is not a valid Guid");
            }

            var templateIdString = jobject.GetPropertyValue("templateId");
            Guid templateId;
            if (!Guid.TryParse(templateIdString, out templateId))
            {
                throw new InvalidOperationException("templateId is not a valid Guid");
            }

            var childCountString = jobject.GetPropertyValue("childCount");
            int childCount;
            if (!int.TryParse(childCountString, out childCount))
            {
                throw new InvalidOperationException("childCount is not a valid integer");
            }

            var item = new FileDatabaseItem(this, parent, itemId, itemName, itemDisplayName, icon16X16, icon32X32, template, templateId, path, childCount, mediaUrl);

            var fields = (JObject)jobject.Property("fields").Value;
            foreach (var property in fields.Properties())
            {
                var fieldName = property.Name;
                var value = property.Value.ToString();

                var field = new FileDatabaseField(fieldName, value);

                item.Fields.Add(field);
            }

            Items.Add(item);

            var childrenProperty = jobject.Property("children");
            if (childrenProperty == null)
            {
                return;
            }

            var children = (JArray)childrenProperty.Value;
            foreach (var child in children.OfType<JObject>())
            {
                ReadItem(child, item);
            }
        }
    }
}
