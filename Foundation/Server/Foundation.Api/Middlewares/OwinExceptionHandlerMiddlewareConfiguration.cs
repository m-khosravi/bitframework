using System;
using Owin;
using Foundation.Api.Contracts;

namespace Foundation.Api.Middlewares
{
    public class OwinExceptionHandlerMiddlewareConfiguration : IOwinMiddlewareConfiguration
    {
        public virtual void Configure(IAppBuilder owinApp)
        {
            if (owinApp == null)
                throw new ArgumentNullException(nameof(owinApp));

            owinApp.Use<OwinExceptionHandlerMiddleware>();
        }
    }
}