// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Web.Mvc;

namespace Sitecore.ContentDelivery.Web
{
    public interface IContentDeliveryCall
    {
        [NotNull]
        ActionResult Execute([NotNull] Controller controller);
    }
}