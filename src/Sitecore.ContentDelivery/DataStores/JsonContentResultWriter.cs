// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Sitecore.ContentDelivery.DataStores
{
    public class JsonContentResultWriter : JsonTextWriter
    {
        private readonly TextWriter _textWriter;

        public JsonContentResultWriter(TextWriter textWriter) : base(textWriter)
        {
            _textWriter = textWriter;

            Formatting = Formatting.Indented;

            // ReSharper disable once VirtualMemberCallInContructor
            WriteStartObject();
        }

        public ContentResult ToContentResult()
        {
            WriteEndObject();

            return new ContentResult
            {
                Content = _textWriter.ToString(),
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8
            };
        }
    }
}
