// © 2015 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.ContentDelivery.DataStores.Formatters
{
    public interface IFieldValueFormatter
    {
        double Priority { get; }

        bool TryFormat([NotNull] FieldDescriptor fieldDescriptor, [NotNull] string value, [NotNull] out string formattedValue);
    }
}
