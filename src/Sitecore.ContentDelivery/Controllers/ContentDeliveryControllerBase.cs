// © 2015-2017 by Jakob Christensen. All rights reserved.

using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Sitecore.Security.Authentication;
using Sitecore.SecurityModel.License;
using Sitecore.Web;

namespace Sitecore.ContentDelivery.Controllers
{
    public abstract class ContentDeliveryControllerBase : Controller
    {
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

            AuthenticationManager.Login(userName, password, true);

            return null;
        }
    }
}
