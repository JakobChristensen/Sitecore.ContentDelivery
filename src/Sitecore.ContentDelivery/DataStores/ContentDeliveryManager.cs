// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentDelivery.DataStores.Formatters;

namespace Sitecore.ContentDelivery.DataStores
{
    public static class ContentDeliveryManager
    {
        private static readonly ICollection<IDataStore> DataStores = new List<IDataStore>();

        [NotNull]
        public static ICollection<IFieldValueFormatter> FieldValueFormatters { get; } = new List<IFieldValueFormatter>();

        [CanBeNull]
        public static IDataStore GetDataStore([NotNull] string dataStoreName)
        {
            return DataStores.FirstOrDefault(d => d.DataStoreName == dataStoreName);
        }

        [NotNull]
        public static IDataStore MountDataStore([NotNull] IDataStore dataStore)
        {
            DataStores.Add(dataStore);
            return dataStore;
        }

        public static void RegisterFieldValueFormatter([NotNull] FieldValueFormatter fieldValueFormatter)
        {
            FieldValueFormatters.Add(fieldValueFormatter);
        }

        public static void UnregisterFieldValueFormatter([NotNull] FieldValueFormatter fieldValueFormatter)
        {
            FieldValueFormatters.Remove(fieldValueFormatter);
        }

        [NotNull]
        public static IDataStore UnmountDataStore([NotNull] IDataStore dataStore)
        {
            DataStores.Remove(dataStore);
            return dataStore;
        }
    }
}
