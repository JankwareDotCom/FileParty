using System.Threading.Tasks;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileParty.Core.Registration
{
    internal class FilePartyModuleConfiguration<TModule> : IFilePartyModuleConfiguration 
        where TModule : class, IFilePartyModule, new()
    {
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();
        private StorageProviderConfiguration<TModule> _defaultConfiguration;
        private TModule _module;
        public FilePartyModuleConfiguration(StorageProviderConfiguration<TModule> defaultConfiguration)
        {
            SetDefaultConfiguration(defaultConfiguration);
        }

        internal void SetDefaultConfiguration(StorageProviderConfiguration<TModule> defaultConfiguration)
        {
            _defaultConfiguration = defaultConfiguration;
        }

        internal IServiceCollection GetServiceCollection() => _serviceCollection;
        internal IServiceCollection AttachModule(TModule module)
        {
            _module = module;
            var sc = GetServiceCollection();
            sc.AddSingleton(typeof(IStorageProvider), _module.GetStorageProviderType());
            sc.AddSingleton(typeof(IAsyncStorageProvider), _module.GetAsyncStorageProviderType());
            return sc;
        }

        internal IStorageProvider GetStorageProvider(StorageProviderConfiguration<TModule> configuration)
        {
            var sc = new ServiceCollection {GetServiceCollection()};
            var config = configuration ?? _defaultConfiguration;
            sc.AddSingleton<StorageProviderConfiguration<TModule>>(x => config);
            sc.AddSingleton<IStorageProviderConfiguration>(x => config);
            using var sp = sc.BuildServiceProvider();
            var service = sp.GetRequiredService<IStorageProvider>();
            
            return service;
        }
        
        internal async Task<IAsyncStorageProvider> GetAsyncStorageProvider(StorageProviderConfiguration<TModule> configuration)
        {
            var sc = new ServiceCollection {GetServiceCollection()};
            var config = configuration ?? _defaultConfiguration;
            sc.AddSingleton<StorageProviderConfiguration<TModule>>(x => config);
            sc.AddSingleton<IStorageProviderConfiguration>(x => config);
            await using var sp = sc.BuildServiceProvider();
            var service = sp.GetRequiredService<IAsyncStorageProvider>();
            return service;
        }

        public IStorageProvider GetStorageProvider()
        {
            return GetStorageProvider(null);
        }

        public async Task<IAsyncStorageProvider> GetAsyncStorageProvider()
        {
            return await GetAsyncStorageProvider(null);
        }
    }
}