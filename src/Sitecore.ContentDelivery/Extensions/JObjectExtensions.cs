using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sitecore.ContentDelivery.Extensions
{
    public static class JObjectExtensions
    {
        [NotNull]
        public static string GetPropertyValue([NotNull] this JObject jobject, [NotNull] string propertyName, [NotNull] string defaultValue = "")
        {
            var property = jobject.Property(propertyName);
            if (property == null)
            {
                return defaultValue;
            }

            return property.Value.ToString();
        }
    }
}