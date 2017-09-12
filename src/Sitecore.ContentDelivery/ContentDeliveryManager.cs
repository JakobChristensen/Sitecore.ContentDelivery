// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentDelivery.Databases;
using Sitecore.ContentDelivery.Databases.Formatters;

namespace Sitecore.ContentDelivery
{
    public static class ContentDeliveryManager
    {
        public static readonly ICollection<IDatabase> Databases = new List<IDatabase>();

        [NotNull]
        public static ICollection<IFieldValueFormatter> FieldValueFormatters { get; } = new List<IFieldValueFormatter>();

        [CanBeNull]
        public static IDatabase GetDatabase([NotNull] string databaseName) => Databases.FirstOrDefault(d => d.DatabaseName == databaseName);

        [NotNull]
        public static IDatabase MountDatabase([NotNull] IDatabase database)
        {
            Databases.Add(database);
            return database;
        }

        public static void RegisterFieldValueFormatter([NotNull] SitecoreFieldValueFormatter sitecoreFieldValueFormatter)
        {
            FieldValueFormatters.Add(sitecoreFieldValueFormatter);
        }

        public static void UnregisterFieldValueFormatter([NotNull] SitecoreFieldValueFormatter sitecoreFieldValueFormatter)
        {
            FieldValueFormatters.Remove(sitecoreFieldValueFormatter);
        }

        [NotNull]
        public static IDatabase UnmountDatabase([NotNull] IDatabase database)
        {
            Databases.Remove(database);
            return database;
        }
    }
}
