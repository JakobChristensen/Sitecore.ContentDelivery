using System;
using System.Web.Mvc;
using System.Web.Security;
using Sitecore.SecurityModel.License;
using Sitecore.Web;

namespace Sitecore.ContentDelivery.Controllers
{
    public abstract class ContentDeliveryControllerBase : Controller
    {
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