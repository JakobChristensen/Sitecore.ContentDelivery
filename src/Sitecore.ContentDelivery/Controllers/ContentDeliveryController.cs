// © 2015-2017 by Jakob Christensen. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using Sitecore.ContentDelivery.Databases;
using Sitecore.ContentDelivery.Extensions;
using Sitecore.ContentDelivery.Web;
using Sitecore.SecurityModel.License;
using Sitecore.Web;

namespace Sitecore.ContentDelivery.Controllers
{
    public class ContentDeliveryController : Controller
    {
        [NotNull]
        public virtual ActionResult AddItem(string databaseName, string itemPath, string template)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            if (actionResult != null)
            {
                return actionResult;
            }

            return database.AddItem(requestParameters, itemPath, template);
        }

        [NotNull]
        public virtual ActionResult DeleteItems(string databaseName, string itemUri)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            if (actionResult != null)
            {
                return actionResult;
            }

            var itemUris = GetItemUris(itemUri);

            return database.DeleteItems(requestParameters, itemUris);
        }

        [NotNull]
        public virtual ActionResult DumpDatabase(string databaseName)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            if (actionResult != null)
            {
                return actionResult;
            }

            var dictionary = new Dictionary<string, string>
            {
                ["children"] = "9999",
                ["fields"] = "*",
                ["systemfields"] = "true"
            };

            requestParameters = new RequestParameters(dictionary);

            return database.GetItem(requestParameters, "{11111111-1111-1111-1111-111111111111}");
        }

        [NotNull, HttpPost]
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
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            return actionResult ?? database.GetChildren(requestParameters, itemName);
        }

        [NotNull]
        public virtual ActionResult GetDatabase(string databaseName)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            return actionResult ?? database.GetDatabase(requestParameters);
        }

        [NotNull]
        public virtual ActionResult GetDatabases()
        {
            var output = new JsonContentResultWriter(new StringWriter());
            output.WriteStartObject("metadata");
            output.WritePropertyString("version", Constants.Version);
            output.WriteEndObject();

            output.WriteStartArray("databases");

            foreach (var database in ContentDeliveryManager.Databases)
            {
                output.WriteStartObject();
                output.WritePropertyString("name", database.DatabaseName);
                output.WritePropertyString("type", database.GetType().FullName ?? string.Empty);
                output.WriteEndObject();
            }

            output.WriteEndArray();

            return output.ToContentResult();
        }

        [NotNull]
        public virtual ActionResult GetItem(string databaseName, string itemName)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            return actionResult ?? database.GetItem(requestParameters, itemName);
        }

        [NotNull]
        public virtual ActionResult GetInsertOptions(string databaseName, string itemName)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            return actionResult ?? database.GetInsertOptions(requestParameters, itemName);
        }

        [NotNull]
        public virtual ActionResult GetItems(string databaseName)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            return actionResult ?? database.GetItems(requestParameters);
        }

        [NotNull]
        public virtual ActionResult GetTemplate(string databaseName, string templateName)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            return actionResult ?? database.GetTemplate(requestParameters, templateName);
        }

        [NotNull]
        public virtual ActionResult GetTemplates(string databaseName)
        {
            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            return actionResult ?? database.GetTemplates(requestParameters);
        }

        [NotNull, HttpPost]
        public virtual ActionResult SaveItems(string databaseName)
        {
            var fields = new Dictionary<string, string>();
            foreach (var key in Request.Form.AllKeys)
            {
                if (key != null)
                {
                    fields[key] = Request.Form[key];
                }
            }

            PreprocessRequest(databaseName, out var actionResult, out var requestParameters, out var database);
            return actionResult ?? database.SaveItems(requestParameters, fields);
        }

        [CanBeNull]
        protected virtual ActionResult AuthenticateUser()
        {
            if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.IndexOf("Sitecore.Kernel", StringComparison.Ordinal) >= 0))
            {
                return null;
            }

            return AuthenticateUsingSitecore();
        }

        [CanBeNull]
        protected ActionResult AuthenticateUsingSitecore()
        {
            // todo: provider better security

            var userName = WebUtil.GetQueryString("username") ?? string.Empty;
            var password = WebUtil.GetQueryString("password") ?? string.Empty;

            if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (Context.IsLoggedIn)
            {
                if (string.Equals(Context.User.Name, userName, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                Context.Logout();
            }

            if (!LicenseManager.HasRuntime)
            {
                return new HttpUnauthorizedResult("A required license is missing");
            }

            var validated = Membership.ValidateUser(userName, password);
            if (!validated)
            {
                return new HttpUnauthorizedResult("Unknown username or password");
            }

            Security.Authentication.AuthenticationManager.Login(userName, password, true);

            return null;
        }

        [NotNull]
        protected virtual IEnumerable<string> GetItemUris([NotNull] string itemUri)
        {
            var itemUris = new List<string>();

            if (!string.IsNullOrEmpty(itemUri))
            {
                itemUris.Add(itemUri);
            }

            var queryString = WebUtil.GetQueryString("itemuris");
            if (!string.IsNullOrEmpty(queryString))
            {
                itemUris.AddRange(queryString.Split(','));
            }

            foreach (var key in Request.Form.AllKeys)
            {
                if (key.StartsWith("itemuri", StringComparison.InvariantCultureIgnoreCase))
                {
                    itemUris.Add(Request.Form[key]);
                }
            }

            return itemUris;
        }

        protected virtual void PreprocessRequest(string databaseName, [CanBeNull] out ActionResult actionResult, [NotNull] out RequestParameters requestParameters, [NotNull] out IDatabase database)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            database = null;
            requestParameters = RequestParameters.Empty;

            actionResult = AuthenticateUser();
            if (actionResult != null)
            {
                return;
            }

            var db = ContentDeliveryManager.GetDatabase(databaseName);
            if (db == null)
            {
                actionResult = new HttpStatusCodeResult(HttpStatusCode.NotFound, "Database not found");
                return;
            }

            database = db;

            requestParameters = new RequestParameters(database.RequestParameters, Request);
        }
    }
}
