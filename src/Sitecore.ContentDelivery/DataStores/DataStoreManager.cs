// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace Sitecore.ContentDelivery.DataStores
{
    public static class DataStoreManager
    {
        private static readonly ICollection<IDataStore> DataStores = new List<IDataStore>();

        static DataStoreManager()
        {
            Mount(new ItemDataStore("master"));
            Mount(new ItemDataStore("core"));
            Mount(new ItemDataStore("web"));
        }

        [CanBeNull]
        public static IDataStore GetDataStore([NotNull] string dataStoreName)
        {
            return DataStores.FirstOrDefault(d => d.DataStoreName == dataStoreName);
        }

        [NotNull]
        public static IDataStore Mount([NotNull] IDataStore dataStore)
        {
            DataStores.Add(dataStore);
            return dataStore;
        }

        [NotNull]
        public static IDataStore Unmount([NotNull] IDataStore dataStore)
        {
            DataStores.Remove(dataStore);
            return dataStore;
        }
    }
}
