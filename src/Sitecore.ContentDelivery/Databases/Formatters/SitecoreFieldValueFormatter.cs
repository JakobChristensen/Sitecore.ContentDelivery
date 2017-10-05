// © 2015-2017 by Jakob Christensen. All rights reserved.

using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;
using Sitecore.ContentDelivery.Extensions;
using Sitecore.Data.Fields;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI;

namespace Sitecore.ContentDelivery.Databases.Formatters
{
    public class SitecoreFieldValueFormatter : IFieldValueFormatter
    {
        public double Priority { get; } = 1000;

        public virtual bool TryFormat(object field, FieldInfo fieldInfo, string value, out string formattedValue)
        {
            formattedValue = string.Empty;

            switch (fieldInfo.Format.ToLowerInvariant())
            {
                case "icon16x16":
                    formattedValue = WebUtil.GetFullUrl(Images.GetThemedImageSource(value, ImageDimension.id16x16));
                    return true;
                case "icon24x24":
                    formattedValue = WebUtil.GetFullUrl(Images.GetThemedImageSource(value, ImageDimension.id24x24));
                    return true;
                case "icon32x32":
                    formattedValue = WebUtil.GetFullUrl(Images.GetThemedImageSource(value, ImageDimension.id32x32));
                    return true;
                case "icon48x48":
                    formattedValue = WebUtil.GetFullUrl(Images.GetThemedImageSource(value, ImageDimension.id48x48));
                    return true;
                case "url":
                    formattedValue = WebUtil.GetFullUrl(value);
                    return true;
                case "img":
                    var imageField = new ImageField((Field)field);
                    var url = MediaManager.GetMediaUrl(imageField.MediaItem);
                    formattedValue = WebUtil.GetFullUrl(url);
                    return true;
                case "json-layout":
                    formattedValue = FormatJsonLayout((Field)field);
                    return true;
            }

            return false;
        }

        [NotNull]
        protected virtual string FormatJsonLayout([NotNull] Field field)
        {
            var layoutString = LayoutField.GetFieldValue(field);
            if (string.IsNullOrEmpty(layoutString))
            {
                return string.Empty;
            }

            var layout = XDocument.Parse(layoutString);
            if (layout.Root == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new JsonTextWriter(writer);

            output.WriteStartObject();

            output.WritePropertyString("$schema", "http://sitecore.net/contentdelivery/jsonlayout");
            output.WritePropertyString("version", "1.0.0");

            output.WriteStartArray("devices");
            foreach (var deviceElement in layout.Root.Elements())
            {
                output.WriteStartObject();

                var deviceId = deviceElement.GetAttributeValue("id");
                var layoutId = deviceElement.GetAttributeValue("l");

                output.WritePropertyString("deviceId", deviceId);
                output.WritePropertyString("layoutId", layoutId);

                var deviceItem = field.Database.GetItem(deviceId);
                if (deviceItem != null)
                {
                    output.WritePropertyString("deviceName", deviceItem.Name);
                    output.WritePropertyString("deviceItemPath", deviceItem.Paths.Path);
                }

                var layoutItem = field.Database.GetItem(layoutId);
                if (layoutItem != null)
                {
                    output.WritePropertyString("layoutName", layoutItem.Name);
                    output.WritePropertyString("layoutItemPath", layoutItem.Paths.Path);
                }

                output.WriteStartArray("renderings");

                foreach (var renderingElement in deviceElement.Elements())
                {
                    output.WriteStartObject();

                    var renderingId = renderingElement.GetAttributeValue("id");
                    var uniqueId = renderingElement.GetAttributeValue("uid");
                    var placeholderName = renderingElement.GetAttributeValue("ph");
                    var dataSource = renderingElement.GetAttributeValue("ds");
                    var parameters = new UrlString(renderingElement.GetAttributeValue("par"));
                    var cacheable = renderingElement.GetAttributeValue("cac") == "1";
                    var varyByData = renderingElement.GetAttributeValue("vbd") == "1";
                    var varyByDevice = renderingElement.GetAttributeValue("vbdev") == "1";
                    var varyByLogin = renderingElement.GetAttributeValue("vbl") == "1";
                    var varyByParameters = renderingElement.GetAttributeValue("vbp") == "1";
                    var varyByQueryString = renderingElement.GetAttributeValue("vbqs") == "1";
                    var varyByUser = renderingElement.GetAttributeValue("vbu") == "1";

                    output.WritePropertyString("renderingId", renderingId);
                    var renderingItem = field.Database.GetItem(renderingId);
                    if (renderingItem != null)
                    {
                        output.WritePropertyString("renderingName", renderingItem.Name);
                        output.WritePropertyString("renderingItemPath", renderingItem.Paths.Path);
                    }

                    output.WritePropertyStringIf("uniqueId", uniqueId);
                    output.WritePropertyStringIf("placeholder", placeholderName);
                    output.WritePropertyStringIf("datasource", dataSource);
                    output.WritePropertyStringIf("cacheable", cacheable);
                    output.WritePropertyStringIf("varyByData", varyByData);
                    output.WritePropertyStringIf("varyByDevice", varyByDevice);
                    output.WritePropertyStringIf("varyByLogin", varyByLogin);
                    output.WritePropertyStringIf("varyByParameters", varyByParameters);
                    output.WritePropertyStringIf("varyByQueryString", varyByQueryString);
                    output.WritePropertyStringIf("varyByUser", varyByUser);

                    if (parameters.Parameters.Count > 0)
                    {
                        output.WriteStartArray("parameters");

                        foreach (var key in parameters.Parameters.AllKeys)
                        {
                            output.WriteStartObject();
                            output.WritePropertyString("key", key);
                            output.WritePropertyString("value", parameters.Parameters[key]);
                            output.WriteEndObject();
                        }

                        output.WriteEndArray();
                    }

                    output.WriteEndObject();
                }

                output.WriteEndArray();
                output.WriteEndObject();
            }

            output.WriteEndArray();
            output.WriteEndObject();

            return writer.ToString();
        }
    }
}
