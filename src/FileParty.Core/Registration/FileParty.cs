using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FileParty.Core.EventArgs;
using FileParty.Core.Factory;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Core.WriteProgress;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileParty.Core.Registration
{
    public static class FileParty
    {
        private static ConcurrentDictionary<Type, ConcurrentBag<ServiceDescriptor>> ModuleDependencies =
            new ConcurrentDictionary<Type, ConcurrentBag<ServiceDescriptor>>();
        
        internal static IEnumerable<ServiceDescriptor> GetModuleDependencies<TModule>() where TModule : class, IFilePartyModule, new()
        {
            var dependencies =  ModuleDependencies.GetOrAdd(typeof(TModule), new ConcurrentBag<ServiceDescriptor>());
            return dependencies.ToArray();
        }

        private static void EnsureKeyExists<TModule>() where TModule : class, IFilePartyModule, new()
        {
            if (!ModuleDependencies.ContainsKey(typeof(TModule)))
            {
                ModuleDependencies.GetOrAdd(typeof(TModule), new ConcurrentBag<ServiceDescriptor>());
            }
        }
        
        public static void RegisterModuleDependency<TModule, TService>(this TModule module)
            where TModule : class, IFilePartyModule, new()
            where TService : class
        {
            EnsureKeyExists<TModule>();
            ModuleDependencies[typeof(TModule)].Add(new ServiceDescriptor(typeof(TService), ServiceLifetime.Scoped));
        }

        public static void RegisterModuleDependency<TModule, TService, TImplementation>(this TModule module)
            where TModule : class, IFilePartyModule, new()
            where TService : class
            where TImplementation : class, TService
        {
            EnsureKeyExists<TModule>();
            ModuleDependencies[typeof(TModule)]
                .Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped));
        }

        public static void RegisterModuleDependency<TModule, TService>(this TModule module, Func<TModule, TService> implementationFactory)
            where TModule : class, IFilePartyModule, new()
            where TService : class
        {
            EnsureKeyExists<TModule>();
            ModuleDependencies[typeof(TModule)]
                .Add(new ServiceDescriptor(typeof(TService), x=> (TService) implementationFactory.DynamicInvoke(module), ServiceLifetime.Scoped));
        }

        public static void RegisterModuleDependency<TModule, TService, TImplementation>(this TModule module, Func<TModule, TImplementation> implementationFactory)
            where TModule : class, IFilePartyModule, new()
            where TService : class
            where TImplementation : class, TService
        {
            EnsureKeyExists<TModule>();
            ModuleDependencies[typeof(TModule)]
                .Add(new ServiceDescriptor(typeof(TService), x=> (TImplementation) implementationFactory.DynamicInvoke(), ServiceLifetime.Scoped));
        }
        
        /// <summary>
        /// Registers File Party with a service collection.  If the object is not a service collection,
        /// a service collection will be made, then File Party will be registered.
        /// </summary>
        /// <param name="caller">Any object, or service collection</param>
        /// <param name="configure">Configuration action to add and configure modules.
        /// File Party will not work without any configured modules</param>
        /// <returns>IServiceCollection to which File Party is registered.</returns>
        public static IServiceCollection AddFileParty(
            this object caller,
            params Action<IFilePartyConfiguration>[] configure)
        {
            if (!(caller is IServiceCollection sc))
            {
                sc = new ServiceCollection();    
            }
            
            return sc.AddFileParty(configure);
        }

        /// <summary>
        /// Registers File party with a service collection.
        /// </summary>
        /// <param name="serviceCollection">service collection</param>
        /// <param name="configure">Configuration action to add and configure modules.
        /// File Party will not work without any configured modules</param>
        /// <returns>IServiceCollection to which FileParty is registered</returns>
        public static IServiceCollection AddFileParty(
            this IServiceCollection serviceCollection, 
            params Action<IFilePartyConfiguration>[] configure)
        {
            var filePartyConfiguration = new FilePartyConfiguration();
            filePartyConfiguration.SetServiceCollection(serviceCollection);
            
            serviceCollection.TryAddSingleton<IWriteProgressSubscriptionManager, WriteProgressSubscriptionManager>();
            serviceCollection.TryAddSingleton<IWriteProgressRelay, WriteProgressRelay>();
            
            foreach (var configuration in configure)
            {
                configuration(filePartyConfiguration);
            }

            serviceCollection.TryAddSingleton<IFilePartyFactory, FilePartyFactory>();
            serviceCollection.TryAddSingleton<IAsyncFilePartyFactory, AsyncFilePartyFactory>();
            serviceCollection.TryAddSingleton<IFilePartyFactoryModifier, FilePartyFactoryModifier>();
            
            return serviceCollection;
        }
    }
    
    internal class FilePartyConfiguration : IFilePartyConfiguration, IConfiguredFileParty
    {
        private readonly object _serviceCollectionLock = new object();
        private IServiceCollection _serviceCollection;
        
        internal IServiceCollection GetServiceCollection()
        {
            lock (_serviceCollectionLock)
            {
                return _serviceCollection;
            }
        }

        internal void SetServiceCollection(IServiceCollection serviceCollection)
        {
            lock (_serviceCollectionLock)
            {
                _serviceCollection = serviceCollection;
                _serviceCollection.AddSingleton<IConfiguredFileParty>(this);
            }
        }
        
        public IFilePartyConfiguration AddModule<TModule>(StorageProviderConfiguration<TModule> defaultStorageProviderConfiguration)
            where TModule : class, IFilePartyModule, new()
        {
            // register module config with core
            var fpServiceCollection = GetServiceCollection();
            var moduleConfig = new FilePartyModuleConfiguration<TModule>(defaultStorageProviderConfiguration);
            fpServiceCollection.AddSingleton<IFilePartyModuleConfiguration>(moduleConfig);
            fpServiceCollection.AddSingleton(x => moduleConfig);
            
            // register module with module config
            var module = new TModule();
            moduleConfig.AttachModule(module);
            
            // get module service collection and register dependencies
            var dependencies = FileParty.GetModuleDependencies<TModule>();
            var moduleServiceCollection = moduleConfig.GetServiceCollection();
            foreach (var d in dependencies)
            {
                moduleServiceCollection.Add(d);
            }
            
            return this;
        }

        public IFilePartyConfiguration SetDefaultModule<TModule>()
            where TModule : class, IFilePartyModule, new()
        {
            DefaultModuleType = typeof(TModule);
            return this;
        }
        
        private static readonly object DefaultModuleTypeLock = new object();
        
        private Type _defaultModuleType;

        public Type DefaultModuleType
        {
            get
            {
                lock (DefaultModuleTypeLock)
                {
                    return _defaultModuleType;
                }
            }
            set
            {
                lock (DefaultModuleTypeLock)
                {
                    _defaultModuleType = value;
                }
            }
        }
    }
}