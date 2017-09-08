// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json.Linq;
using Sitecore.ContentDelivery.Extensions;
using Sitecore.ContentDelivery.Web;

namespace Sitecore.ContentDelivery.Controllers
{
    public class ContentDeliveryController : ContentDeliveryControllerBase
    {
        [NotNull]
        public virtual ActionResult GetBundle()
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var output = new JsonContentResultWriter(new StringWriter());

            // run each bundle entry through the MVC pipeline
            foreach (var key in Request.Form.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (key == "username" || key == "password" || key == "domain")
                {
                    continue;
                }

                var url = Request.Form[key];

                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    uri = new Uri("http://127.0.0.1" + url);
                }

                // find the route
                var textWriter = new StringWriter();
                var httpContext = new HttpContextWrapper(new HttpContext(new HttpRequest(string.Empty, uri.AbsoluteUri, uri.Query.Mid(1)), new HttpResponse(textWriter)));
                var routeData = RouteTable.Routes.GetRouteData(httpContext);
                if (routeData == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Route not found: " + url);
                }

                // execute the controller
                var controllerName = routeData.Values["controller"].ToString();
                var requestContext = new RequestContext(httpContext, routeData);

                var controller = ControllerBuilder.Current.GetControllerFactory().CreateController(requestContext, controllerName);
                controller.Execute(requestContext);

                // if empty - write an empty object
                var result = textWriter.ToString();
                if (string.IsNullOrEmpty(result))
                {
                    output.WritePropertyName(key);
                    output.WriteStartObject();
                    output.WriteEndObject();
                    continue;
                }

                JObject jObject;
                try
                {
                    jObject = JObject.Parse(result);
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, result);
                }

                output.WritePropertyName(key);
                jObject.WriteTo(output);
            }

            return output.ToContentResult();
        }

        [NotNull]
        public virtual ActionResult GetChildren(string databaseName, string itemName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var database = ContentDeliveryManager.GetDatabase(databaseName);
            if (database == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Database not found");
            }

            var requestParameters = new RequestParameters(Request);

            return database.GetChildren(requestParameters, itemName);
        }

        [NotNull]
        public virtual ActionResult GetDatabase(string databaseName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var database = ContentDeliveryManager.GetDatabase(databaseName);
            if (database == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Database not found");
            }

            var requestParameters = new RequestParameters(Request);

            return database.GetDatabase(requestParameters);
        }

        [NotNull]
        public virtual ActionResult GetItem(string databaseName, string itemName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var database = ContentDeliveryManager.GetDatabase(databaseName);
            if (database == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Database not found");
            }

            var requestParameters = new RequestParameters(Request);

            return database.GetItem(requestParameters, itemName);
        }

        [NotNull]
        public virtual ActionResult GetItems(string databaseName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var database = ContentDeliveryManager.GetDatabase(databaseName);
            if (database == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Database not found");
            }

            var requestParameters = new RequestParameters(Request);

            return database.GetItems(requestParameters);
        }

        [NotNull]
        public virtual ActionResult GetTemplate(string databaseName, string templateName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var database = ContentDeliveryManager.GetDatabase(databaseName);
            if (database == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Database not found");
            }

            var requestParameters = new RequestParameters(Request);

            return database.GetTemplate(requestParameters, templateName);
        }

        [NotNull]
        public virtual ActionResult DumpDatabase(string databaseName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var database = ContentDeliveryManager.GetDatabase(databaseName);
            if (database == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Database not found");
            }

            var dictionary = new Dictionary<string, string>();
            dictionary["children"] = "999";
            dictionary["fields"] = "*";
            dictionary["systemfields"] = "true";

            var requestParameters = new RequestParameters(dictionary);

            return database.GetItem(requestParameters, "{11111111-1111-1111-1111-111111111111}");
        }

        [NotNull]
        public virtual ActionResult GetTemplates(string databaseName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var database = ContentDeliveryManager.GetDatabase(databaseName);
            if (database == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Database not found");
            }

            var requestParameters = new RequestParameters(Request);

            return database.GetTemplates(requestParameters);
        }
    }
}
