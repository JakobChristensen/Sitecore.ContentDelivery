// © 2015 Sitecore Corporation A/S. All rights reserved.

using Newtonsoft.Json;
using Sitecore.ContentDelivery.Extensions;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.ContentDelivery.DataStores.ItemDataStores
{
    public class ItemDataStoreWriter : IItemDataStoreWriter
    {
        public double Priority { get; } = 1000;

        public virtual void WriteItemField(JsonTextWriter output, Field field, string value)
        {
            output.WritePropertyString("id", field.ID.ToString());
            output.WritePropertyString("name", field.Name);
            output.WritePropertyString("displayName", field.DisplayName);
            output.WritePropertyString("value", value);
        }

        public virtual void WriteItemHeader(JsonTextWriter output, Item item)
        {
            output.WritePropertyString("id", item.ID.ToString());
            output.WritePropertyString("name", item.Name);
            output.WritePropertyString("displayName", item.DisplayName);
            output.WritePropertyString("database", item.Database.Name);
            output.WritePropertyString("icon16x16", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));
            output.WritePropertyString("icon32x32", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id32x32));
            output.WritePropertyString("path", item.Paths.Path);
            output.WritePropertyString("templateId", item.TemplateID.ToString());
            output.WritePropertyString("templateName", item.TemplateName);
            output.WritePropertyString("childCount", item.Children.Count);
        }

        public virtual void WriteMetaData(JsonTextWriter output)
        {
            output.WritePropertyString("version", "1");
            output.WritePropertyString("user", Context.GetUserName());
            output.WritePropertyString("language", Context.Language.Name);
        }

        public virtual void WriteTemplateField(JsonTextWriter output, TemplateFieldItem field, bool isOwnField, bool isSystemField)
        {
            output.WritePropertyString("id", field.ID.ToString());
            output.WritePropertyString("name", field.Name);
            output.WritePropertyString("displayName", field.DisplayName);
            output.WritePropertyString("type", field.Type);
            output.WritePropertyString("source", field.Source);
            output.WritePropertyString("sharing", field.IsShared ? "shared" : field.IsUnversioned ? "unversioned" : "versioned");
            output.WritePropertyString("section", field.Section.Name);
            output.WritePropertyString("kind", isOwnField ? "own" : isSystemField ? "system" : "inherited");
        }
    }
}
