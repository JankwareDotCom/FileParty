using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Core.Registration;

namespace FileParty.Core.Factory
{
    public class AsyncFilePartyFactory : IAsyncFilePartyFactory
    {
        private readonly IConfiguredFileParty _filePartyConfiguration;
        private readonly IEnumerable<IFilePartyModuleConfiguration> _moduleConfigurations;
        private readonly IWriteProgressRelay _relay;

        public AsyncFilePartyFactory(IEnumerable<IFilePartyModuleConfiguration> moduleConfigurations,
            IConfiguredFileParty filePartyConfiguration, IWriteProgressRelay relay)
        {
            _moduleConfigurations = moduleConfigurations;
            _filePartyConfiguration = filePartyConfiguration;
            _relay = relay;
        }

        public async Task<IAsyncStorageProvider> GetAsyncStorageProvider()
        {
            var storageProvider = _filePartyConfiguration.DefaultModuleType != null &&
                                  typeof(IFilePartyModule).IsAssignableFrom(_filePartyConfiguration.DefaultModuleType)
                ? await _moduleConfigurations
                    .Last(l =>
                        l.GetType().IsGenericType &&
                        l.GetType().GenericTypeArguments
                            .Contains(_filePartyConfiguration.DefaultModuleType))
                    .GetAsyncStorageProvider()
                : await _moduleConfigurations.Last().GetAsyncStorageProvider();

            storageProvider.WriteProgressEvent += _relay.RelayWriteProgressEvent;
            return storageProvider;
        }

        public async Task<IAsyncStorageProvider> GetAsyncStorageProvider<TModule>()
            where TModule : class, IFilePartyModule, new()
        {
            try
            {
                var storageProvider = await _moduleConfigurations.Last(f => f is FilePartyModuleConfiguration<TModule>)
                    .GetAsyncStorageProvider();
                storageProvider.WriteProgressEvent += _relay.RelayWriteProgressEvent;
                return storageProvider;
            }
            catch (Exception)
            {
                throw Errors.SPNotFound;
            }
        }

        public async Task<IAsyncStorageProvider> GetAsyncStorageProvider<TModule>(
            StorageProviderConfiguration<TModule> configuration) where TModule : class, IFilePartyModule, new()
        {
            if (!(_moduleConfigurations.LastOrDefault(f => f is FilePartyModuleConfiguration<TModule>)
                    is FilePartyModuleConfiguration<TModule> modCfg))
            {
                throw Errors.SPNotFound;
            }

            var storageProvider = await modCfg.GetAsyncStorageProvider(configuration);
            storageProvider.WriteProgressEvent += _relay.RelayWriteProgressEvent;
            return storageProvider;
        }

        public async Task<IAsyncStorageReader> GetAsyncStorageReader()
        {
            return await GetAsyncStorageProvider();
        }

        public async Task<IAsyncStorageReader> GetAsyncStorageReader<TModule>()
            where TModule : class, IFilePartyModule, new()
        {
            return await GetAsyncStorageProvider<TModule>();
        }

        public async Task<IAsyncStorageReader> GetAsyncStorageReader<TModule>(
            StorageProviderConfiguration<TModule> configuration) where TModule : class, IFilePartyModule, new()
        {
            return await GetAsyncStorageProvider(configuration);
        }

        public async Task<IAsyncStorageWriter> GetAsyncStorageWriter()
        {
            return await GetAsyncStorageProvider();
        }

        public async Task<IAsyncStorageWriter> GetAsyncStorageWriter<TModule>()
            where TModule : class, IFilePartyModule, new()
        {
            return await GetAsyncStorageProvider<TModule>();
        }

        public async Task<IAsyncStorageWriter> GetAsyncStorageWriter<TModule>(
            StorageProviderConfiguration<TModule> configuration) where TModule : class, IFilePartyModule, new()
        {
            return await GetAsyncStorageProvider(configuration);
        }
    }
}