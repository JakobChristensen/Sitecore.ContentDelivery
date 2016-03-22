// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Pipelines;

namespace Sitecore.ContentDelivery.Pipelines.Loader
{
    public class MapRoutes
    {
        public void Process([NotNull] PipelineArgs args)
        {
            ContentDeliveryConfig.RegisterRoutes();
        }
    }
}
