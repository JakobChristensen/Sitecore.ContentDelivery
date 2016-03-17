// © 2015 Sitecore Corporation A/S. All rights reserved.

using Newtonsoft.Json;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Sitecore.ContentDelivery.DataStores.ItemDataStores
{
    public interface IItemDataStoreWriter
    {
        double Priority { get; }

        void WriteItemField([NotNull] JsonTextWriter output, [NotNull] Field field, string value);

        void WriteItemHeader([NotNull] JsonTextWriter output, [NotNull] Item item);

        void WriteMetaData([NotNull] JsonTextWriter output);

        void WriteTemplateField([NotNull] JsonTextWriter output, [NotNull] TemplateFieldItem field, bool isOwnField, bool isSystemField);
    }
}
