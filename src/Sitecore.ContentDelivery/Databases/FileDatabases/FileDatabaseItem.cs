// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.ContentDelivery.Databases.FileDatabases
{
    public class FileDatabaseItem
    {
        public FileDatabaseItem([NotNull] FileDatabase database, [CanBeNull] FileDatabaseItem parent, Guid id, [NotNull] string name, string displayName, [NotNull] string icon16X16, [NotNull] string icon32X32, [NotNull] string template, Guid templateId, [NotNull] string path, int childCount, string mediaUrl)
        {
            Database = database;
            Parent = parent;
            Id = id;
            Name = name;
            DisplayName = displayName;
            Icon16X16 = icon16X16;
            Icon32X32 = icon32X32;
            Path = path;
            ChildCount = childCount;
            Template = template;
            TemplateId = templateId;
            MediaUrl = mediaUrl;
        }

        public int ChildCount { get; }

        public IEnumerable<FileDatabaseItem> Children => Database.Items.Where(i => i.Parent == this);

        [NotNull]
        public string DisplayName { get; }

        [NotNull]
        public ICollection<FileDatabaseField> Fields { get; } = new List<FileDatabaseField>();

        [NotNull]
        public string Icon16X16 { get; }

        [NotNull]
        public string Icon32X32 { get; }

        public Guid Id { get; }

        [NotNull]
        public string this[string fieldName]
        {
            get
            {
                var field = Fields.FirstOrDefault(f => f.Name == fieldName);
                return field != null ? field.Value : string.Empty;
            }
        }

        [NotNull]
        public string MediaUrl { get; }

        [NotNull]
        public string Name { get; }

        [CanBeNull]
        public FileDatabaseItem Parent { get; set; }

        [NotNull]
        public string Path { get; }

        [NotNull]
        public string Template { get; }

        public Guid TemplateId { get; }

        [NotNull]
        protected FileDatabase Database { get; }
    }
}
