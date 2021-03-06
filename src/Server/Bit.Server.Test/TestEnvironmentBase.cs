﻿using Bit.Core;
using Bit.Core.Contracts;
using Bit.Core.Implementations;
using Bit.Core.Models;
using Bit.Owin.Implementations;
using Bit.Test.Implementations;
using Bit.Test.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bit.Test
{
    public class TestEnvironmentArgs
    {
        public string? FullUri { get; set; } = null;

        public string? HostName { get; set; }

        public bool UseRealServer { get; set; }

        public bool UseHttps { get; set; }

        public Action<IDependencyManager, IServiceCollection>? AdditionalDependencies { get; set; }

        public Action<AppEnvironment>? ActiveAppEnvironmentCustomizer { get; set; }

        public IAppModulesProvider? CustomAppModulesProvider { get; set; } = null;

        public IAppEnvironmentsProvider? CustomAppEnvironmentsProvider { get; set; } = null;

        public bool UseProxyBasedDependencyManager { get; set; } = true;

        public int? Port { get; set; } = null;
    }

    public class TestAdditionalDependencies : IAppModule, IAppModulesProvider
    {
        private readonly Action<IDependencyManager, IServiceCollection>? _dependencyManagerDelegate;

        public TestAdditionalDependencies(Action<IDependencyManager, IServiceCollection>? dependencyManagerDelegate)
        {
            _dependencyManagerDelegate = dependencyManagerDelegate;
        }

        public virtual void ConfigureDependencies(IServiceCollection services, IDependencyManager dependencyManager)
        {
            _dependencyManagerDelegate?.Invoke(dependencyManager, services);
        }

        public virtual IEnumerable<IAppModule> GetAppModules()
        {
            yield return this;
        }
    }

    public class TestAppEnvironmentsProvider : IAppEnvironmentsProvider
    {
        private readonly IAppEnvironmentsProvider _appEnvironmentsProvider = default!;
        private readonly Action<AppEnvironment>? _appEnvCustomizer;

        protected TestAppEnvironmentsProvider()
        {

        }

        public TestAppEnvironmentsProvider(IAppEnvironmentsProvider appEnvironmentProvider, Action<AppEnvironment>? appEnvCustomizer = null)
        {
            _appEnvironmentsProvider = appEnvironmentProvider ?? throw new ArgumentNullException(nameof(appEnvironmentProvider));
            _appEnvCustomizer = appEnvCustomizer;
        }

        public virtual AppEnvironment GetActiveAppEnvironment()
        {
            var (success, message) = TryGetActiveAppEnvironment(out AppEnvironment? activeAppEnvironment);
            if (success == true)
                return activeAppEnvironment!;
            throw new InvalidOperationException(message);
        }

        public virtual (bool success, string? message) TryGetActiveAppEnvironment(out AppEnvironment? activeAppEnvironment)
        {
            try
            {
                activeAppEnvironment = _appEnvironmentsProvider.GetActiveAppEnvironment();

                _appEnvCustomizer?.Invoke(activeAppEnvironment);

                return (true, null);
            }
            catch (Exception exp)
            {
                activeAppEnvironment = null;
                return (false, exp.Message);
            }
        }
    }

    public class TestEnvironmentBase : IDisposable
    {
        public TestEnvironmentBase(TestEnvironmentArgs? args = null)
        {
            if (args == null)
                args = new TestEnvironmentArgs();

            if (args.FullUri == null && args.HostName == null)
                args.HostName = "localhost";

            string uri = args.FullUri ?? new Uri($"{(args.UseHttps ? "https" : "http")}://{args.HostName}:{args.Port}/").ToString();

            if (args.UseProxyBasedDependencyManager == true)
            {
                DefaultDependencyManager.Current = new AutofacTestDependencyManager();

                TestDependencyManager.CurrentTestDependencyManager.AutoProxyCreationIgnoreRules.AddRange(GetAutoProxyCreationIgnoreRules());
                TestDependencyManager.CurrentTestDependencyManager.AutoProxyCreationIncludeRules.AddRange(GetAutoProxyCreationIncludeRules());
            }

            DefaultAppModulesProvider.Current = GetAppModulesProvider(args);
            DefaultAppEnvironmentsProvider.Current = GetAppEnvironmentsProvider(args);

            Server = GetTestServer(args);

            Server.Initialize(uri);
        }

        protected virtual ITestServer GetTestServer(TestEnvironmentArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.UseRealServer == true)
            {
                return new AspNetCoreSelfHostTestServer();
            }
            else
            {
                return new AspNetCoreEmbeddedTestServer();
            }
        }

        protected virtual List<Func<TypeInfo, bool>> GetAutoProxyCreationIgnoreRules()
        {
            return new List<Func<TypeInfo, bool>>
            {
                implementationType => GetBaseTypes(implementationType).Any(t => t.Name == "DbContext"),
                implementationType => GetBaseTypes(implementationType).Any(t => t.Name == "Hub"),
                implementationType => GetBaseTypes(implementationType).Any(t => t.Name == "Profile"), /*AutoMapper*/
                implementationType => GetBaseTypes(implementationType).Any(t => t.Name == "ODataEdmTypeSerializer"),
                implementationType => GetBaseTypes(implementationType).Any(t => t.Name == "DefaultODataDeserializerProvider"),
                implementationType => implementationType == typeof(AspNetCoreAppEnvironmentsProvider).GetTypeInfo(),
                implementationType => implementationType.IsArray
            };
        }

        protected virtual List<Func<TypeInfo, bool>> GetAutoProxyCreationIncludeRules()
        {
            return new List<Func<TypeInfo, bool>>
            {
                implementationType => AssemblyContainer.Current.GetServerCoreAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerDataAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerDataEntityFrameworkCoreAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerHangfireAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerIdentityServerAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerMetadataAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerODataAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerOwinAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerSignalRAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerTestAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetServerWebApiAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetUniversalCoreAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetUniversalDataAssembly() == implementationType.Assembly,
                implementationType => AssemblyContainer.Current.GetUniversalModelAssembly() == implementationType.Assembly
            };
        }

        protected virtual IEnumerable<Type> GetBaseTypes(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type? baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        protected virtual IAppEnvironmentsProvider GetAppEnvironmentsProvider(TestEnvironmentArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return new TestAppEnvironmentsProvider(args.CustomAppEnvironmentsProvider ?? DefaultAppEnvironmentsProvider.Current, args.ActiveAppEnvironmentCustomizer);
        }

        protected virtual IAppModulesProvider GetAppModulesProvider(TestEnvironmentArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return new CompositeAppModulesProvider(args.CustomAppModulesProvider ?? DefaultAppModulesProvider.Current, new TestAdditionalDependencies(args.AdditionalDependencies));
        }

        public virtual ITestServer Server { get; }

        public virtual IDependencyResolver DependencyResolver => DefaultDependencyManager.Current;

        public virtual IEnumerable<T> GetObjects<T>()
        {
            return TestDependencyManager.CurrentTestDependencyManager
                .Objects
                .OfType<T>()
                .Distinct();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Server?.Dispose();
        }
    }
}
