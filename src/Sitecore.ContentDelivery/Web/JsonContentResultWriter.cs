// © 2015-2017 by Jakob Christensen. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sitecore.ContentDelivery.Extensions;

namespace Sitecore.ContentDelivery.Web
{
    public class JsonContentResultWriter : JsonTextWriter
    {
        private readonly List<string> _errors = new List<string>();

        private readonly TextWriter _textWriter;

        public JsonContentResultWriter(TextWriter textWriter) : base(textWriter)
        {
            _textWriter = textWriter;

            Formatting = Formatting.Indented;

            // ReSharper disable once VirtualMemberCallInContructor
            WriteStartObject();
        }

        [NotNull]
        public ActionResult ToContentResult()
        {
            if (_errors.Any())
            {
                this.WriteStartArray("errors");

                foreach (var error in _errors)
                {
                    WriteValue(error);
                }

                WriteEndArray();
            }

            WriteEndObject();

            if (_errors.Any())
            {
            }

            return new ContentResult
            {
                Content = _textWriter.ToString(),
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8
            };
        }

        public void WriteError(string text) => _errors.Add(text);
    }
}
