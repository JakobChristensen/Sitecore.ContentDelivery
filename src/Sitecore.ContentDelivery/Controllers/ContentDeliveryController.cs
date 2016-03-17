// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using Sitecore.ContentDelivery.DataStores;
using Sitecore.Extensions.StringExtensions;
using Sitecore.SecurityModel.License;
using Sitecore.Web;

namespace Sitecore.ContentDelivery.Controllers
{
    public class ContentDeliveryController : Controller
    {
        public ActionResult GetBundle()
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var output = new JsonContentResultWriter(new StringWriter());

            foreach (var key in Request.Form.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (key == "username" || key == "password" || key == "token" || key == "domain")
                {
                    continue;
                }

                var url = Request.Form[key];

                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    uri = new Uri("http://127.0.0.1" + url); 
                }

                var textWriter = new StringWriter();
                var httpContext = new HttpContextWrapper(new HttpContext(new HttpRequest(string.Empty, uri.AbsoluteUri, uri.Query.Mid(1)), new HttpResponse(textWriter)));
                var routeData = RouteTable.Routes.GetRouteData(httpContext);
                if (routeData == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Route not found: " + url);
                }

                var controllerName = routeData.Values["controller"].ToString();
                var requestContext = new RequestContext(httpContext, routeData);

                var factory = ControllerBuilder.Current.GetControllerFactory();
                var controller = factory.CreateController(requestContext, controllerName);

                controller.Execute(requestContext);

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

        public ActionResult GetChildren(string dataStoreName, string itemName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var dataStore = DataStoreManager.GetDataStore(dataStoreName);
            if (dataStore == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "DataStore not found");
            }

            var requestParameters = new RequestParameters(Request);

            return dataStore.GetChildren(requestParameters, itemName);
        }

        public ActionResult GetDataStore(string dataStoreName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var dataStore = DataStoreManager.GetDataStore(dataStoreName);
            if (dataStore == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "DataStore not found");
            }

            var requestParameters = new RequestParameters(Request);

            return dataStore.GetDataStore(requestParameters);
        }

        public ActionResult GetItem(string dataStoreName, string itemName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var dataStore = DataStoreManager.GetDataStore(dataStoreName);
            if (dataStore == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "DataStore not found");
            }

            var requestParameters = new RequestParameters(Request);

            return dataStore.GetItem(requestParameters, itemName);
        }

        public ActionResult GetItems(string dataStoreName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var dataStore = DataStoreManager.GetDataStore(dataStoreName);
            if (dataStore == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "DataStore not found");
            }

            var requestParameters = new RequestParameters(Request);

            return dataStore.GetItems(requestParameters);
        }

        public ActionResult GetTemplate(string dataStoreName, string templateName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var dataStore = DataStoreManager.GetDataStore(dataStoreName);
            if (dataStore == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "DataStore not found");
            }

            var requestParameters = new RequestParameters(Request);

            return dataStore.GetTemplate(requestParameters, templateName);
        }

        public ActionResult GetTemplates(string dataStoreName)
        {
            var authenticationResult = AuthenticateUser();
            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            var dataStore = DataStoreManager.GetDataStore(dataStoreName);
            if (dataStore == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "DataStore not found");
            }

            var requestParameters = new RequestParameters(Request);

            return dataStore.GetTemplates(requestParameters);
        }

        protected virtual ActionResult AuthenticateUser()
        {
            var userName = WebUtil.GetQueryString("username") ?? string.Empty;
            var password = WebUtil.GetQueryString("password") ?? string.Empty;
            var authenticationToken = WebUtil.GetQueryString("token") ?? string.Empty;

            if (string.IsNullOrEmpty(authenticationToken) && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(authenticationToken) && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
            {
                // todo: support for authentication tokens
                if (authenticationToken == "1" || authenticationToken == "test")
                {
                    userName = "sitecore\\admin";
                    password = "b";
                }
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
    }
}
