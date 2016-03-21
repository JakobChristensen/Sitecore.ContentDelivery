// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.ContentDelivery.DataStores.FileDataStores
{
    public class FileDataStoreField
    {
        public FileDataStoreField([NotNull] string name, [NotNull] string value)
        {
            Id = Guid.NewGuid();
            Name = name;
            DisplayName = name;
            Value = value;
        }

        public FileDataStoreField(Guid id, [NotNull] string name, [NotNull] string displayName, [NotNull] string value)
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
