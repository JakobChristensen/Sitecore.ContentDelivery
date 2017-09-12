// © 2015 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Fields;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Web;
using Sitecore.Web.UI;

namespace Sitecore.ContentDelivery.Databases.Formatters
{
    public class SitecoreFieldValueFormatter : IFieldValueFormatter
    {
        public double Priority { get; } = 1000;

        public bool TryFormat(object field, FieldInfo fieldInfo, string value, out string formattedValue)
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
            }

            return false;
        }
    }
}
