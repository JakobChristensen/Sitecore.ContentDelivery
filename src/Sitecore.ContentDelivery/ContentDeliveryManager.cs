// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentDelivery.Databases;
using Sitecore.ContentDelivery.Databases.Formatters;

namespace Sitecore.ContentDelivery
{
    public static class ContentDeliveryManager
    {
        private static readonly ICollection<IDatabase> Databases = new List<IDatabase>();

        [NotNull]
        public static ICollection<IFieldValueFormatter> FieldValueFormatters { get; } = new List<IFieldValueFormatter>();

        [CanBeNull]
        public static IDatabase GetDatabase([NotNull] string databaseName)
        {
            return Databases.FirstOrDefault(d => d.DatabaseName == databaseName);
        }

        [NotNull]
        public static IDatabase MountDatabase([NotNull] IDatabase database)
        {
            Databases.Add(database);
            return database;
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
        public static IDatabase UnmountDatabase([NotNull] IDatabase database)
        {
            Databases.Remove(database);
            return database;
        }
    }
}
