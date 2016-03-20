// © 2015 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Fields;

namespace Sitecore.ContentDelivery.DataStores.Formatters
{
    public interface IFieldValueFormatter
    {
        double Priority { get; }

        bool TryFormat([NotNull] Field field, [NotNull] FieldInfo fieldInfo, [NotNull] string value, [NotNull] out string formattedValue);
    }
}
