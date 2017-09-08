// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.ContentDelivery.Databases.FileDatabases
{
    public class FileDatabaseField
    {
        public FileDatabaseField([NotNull] string name, [NotNull] string value)
        {
            Id = Guid.NewGuid();
            Name = name;
            DisplayName = name;
            Value = value;
        }

        public FileDatabaseField(Guid id, [NotNull] string name, [NotNull] string displayName, [NotNull] string value)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
            Value = value;
        }

        [NotNull]
        public string DisplayName { get; }

        public Guid Id { get; }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public string Value { get; }
    }
}
