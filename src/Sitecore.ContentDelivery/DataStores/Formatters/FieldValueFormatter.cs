// © 2015 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Resources;
using Sitecore.Web;
using Sitecore.Web.UI;

namespace Sitecore.ContentDelivery.DataStores.Formatters
{
    public class FieldValueFormatter : IFieldValueFormatter
    {
        public double Priority { get; } = 1000;

        public bool TryFormat(FieldDescriptor fieldDescriptor, string value, out string formattedValue)
        {
            formattedValue = string.Empty;

            switch (fieldDescriptor.Format.ToLowerInvariant())
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
            }

            return false;
        }
    }
}
