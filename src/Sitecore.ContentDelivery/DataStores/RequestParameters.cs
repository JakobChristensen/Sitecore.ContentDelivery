// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.ContentDelivery.DataStores
{
    public class RequestParameters
    {
        private static readonly char[] Comma =
        {
            ','
        };

        private static readonly HashSet<string> Reserved = new HashSet<string>
        {
            "skip",
            "take",
            "fields",
            "systemfields",
            "username",
            "password",
            "domain",
            "token",
            "path",
            "levels"
        };

        public RequestParameters()
        {
            var parameters = new Dictionary<string, string>();

            foreach (var key in HttpContext.Current.Request.Form.AllKeys)
            {
                parameters[key] = HttpContext.Current.Request.Form[key] ?? string.Empty;
            }

            foreach (var key in HttpContext.Current.Request.QueryString.AllKeys)
            {
                parameters[key] = HttpContext.Current.Request.QueryString[key] ?? string.Empty;
            }

            Parse(parameters);
        }

        public RequestParameters([NotNull] Dictionary<string, string> parameters)
        {
            Parse(parameters);
        }

        [NotNull]
        public List<string> FieldNames { get; } = new List<string>();

        public bool IncludeSystemFields { get; private set; }

        [NotNull]
        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        [NotNull]
        public string Path { get; private set; }

        public int Skip { get; private set; }

        public int Take { get; private set; }
        public int Levels { get; private set; }

        private void Parse([NotNull] Dictionary<string, string> parameters)
        {
            string value;
            if (parameters.TryGetValue("skip", out value))
            {
                int skip;
                if (int.TryParse(value, out skip))
                {
                    Skip = skip;
                }
            }

            if (parameters.TryGetValue("take", out value))
            {
                int take;
                if (int.TryParse(value, out take))
                {
                    Take = take;
                }
            }

            if (parameters.TryGetValue("levels", out value))
            {
                int levels;
                if (int.TryParse(value, out levels))
                {
                    Levels = levels;
                }
            }

            if (parameters.TryGetValue("path", out value))
            {
                Path = value;
            }

            foreach (var pair in parameters)
            {
                if (!Reserved.Contains(pair.Key))
                {
                    Parameters[pair.Key] = pair.Value ?? string.Empty;
                }
            }

            if (parameters.TryGetValue("fields", out value))
            {
                FieldNames.AddRange(value.Split(Comma, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
            }

            IncludeSystemFields = parameters.Keys.Any(k => string.Equals(k, "systemfields", StringComparison.OrdinalIgnoreCase));
        }
    }
}
