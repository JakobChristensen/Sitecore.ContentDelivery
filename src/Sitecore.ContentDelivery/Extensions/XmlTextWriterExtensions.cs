// © 2015-2017 by Jakob Christensen. All rights reserved.

using System.Xml;

namespace Sitecore.ContentDelivery.Extensions
{
    public static class XmlTextWriterExtensions
    {
        public static void WriteAttributeStringIf([NotNull] this XmlTextWriter writer, [NotNull] string localName, [NotNull] string value, [NotNull] string defaultValue = "")
        {
            if (value == defaultValue)
            {
                return;
            }

            writer.WriteAttributeString(localName, value);
        }
        public static void WriteAttributeStringIf([NotNull] this XmlTextWriter writer, [NotNull] string localName, bool value, bool defaultValue = false)
        {
            if (value == defaultValue)
            {
                return;
            }

            writer.WriteAttributeString(localName, value ? "1" : "0");
        }
    }
}
