// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using Sitecore.ContentDelivery.DataStores;
using Sitecore.SecurityModel.License;
using Sitecore.Web;

namespace Sitecore.ContentDelivery.Controllers
{
    public class ContentDeliveryController : Controller
    {
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

            return dataStore.GetChildren(itemName);
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

            return dataStore.GetDataStore();
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

            return dataStore.GetItem(itemName);
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

            return dataStore.GetItems();
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

            return dataStore.GetTemplate(templateName);
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

            return dataStore.GetTemplates();
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
                if (authenticationToken == "1")
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
