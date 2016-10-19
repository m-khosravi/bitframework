﻿using Foundation.Api.Middlewares;
using Foundation.Core.Contracts;
using System;

namespace Microsoft.Owin
{
    public static class IOwinContextExtensions
    {
        public static IDependencyResolver GetDependencyResolver(this IOwinContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IDependencyResolver dependencyResolver = context.Get<IDependencyResolver>("DependencyResolver");

            if (dependencyResolver == default(IDependencyResolver))
                throw new InvalidOperationException($"DependencyResolver not found in owin context, See {nameof(AutofacScopeBasedDependencyResolverMiddleware)}");

            return dependencyResolver;
        }
    }
}
