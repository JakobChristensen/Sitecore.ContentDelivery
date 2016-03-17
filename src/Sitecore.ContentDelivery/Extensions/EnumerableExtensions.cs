// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;

namespace Sitecore.ContentDelivery.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>([NotNull] this IEnumerable<T> objects, [NotNull] Action<T> action)
        {
            foreach (var o in objects)
            {
                action(o);
            }
        }
    }
}
