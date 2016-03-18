// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.ContentDelivery.DataStores
{
    public class FieldInfo
    {
        public FieldInfo([NotNull] string fieldName, [NotNull] string format)
        {
            FieldName = fieldName;
            Format = format;
        }

        [NotNull]
        public string FieldName { get; }

        [NotNull]
        public string Format { get; }
    }
}
