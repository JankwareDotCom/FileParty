using System;
using System.Collections.Generic;
using System.Linq;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Core.Registration;

namespace FileParty.Core.Factory
{
    public class FilePartyFactory : IFilePartyFactory
    {
        private readonly IConfiguredFileParty _filePartyConfiguration;
        private readonly IEnumerable<IFilePartyModuleConfiguration> _moduleConfigurations;
        private readonly IWriteProgressRelay _relay;

        public FilePartyFactory(
            IEnumerable<IFilePartyModuleConfiguration> moduleConfigurations,
            IConfiguredFileParty filePartyConfiguration,
            IWriteProgressRelay relay)
        {
            _moduleConfigurations = moduleConfigurations;
            _filePartyConfiguration = filePartyConfiguration;
            _relay = relay;
        }

        public IStorageProvider GetStorageProvider()
        {
            var storageProvider = _filePartyConfiguration.DefaultModuleType != null &&
                                  typeof(IFilePartyModule).IsAssignableFrom(_filePartyConfiguration.DefaultModuleType)
                ? _moduleConfigurations
                    .Last(l =>
                        l.GetType().IsGenericType &&
                        l.GetType().GenericTypeArguments
                            .Contains(_filePartyConfiguration.DefaultModuleType))
                    .GetStorageProvider()
                : _moduleConfigurations.Last().GetStorageProvider();

            storageProvider.WriteProgressEvent += _relay.RelayWriteProgressEvent;
            return storageProvider;
        }

        public IStorageProvider GetStorageProvider<TModule>() where TModule : class, IFilePartyModule, new()
        {
            try
            {
                var storageProvider = _moduleConfigurations.Last(f => f is FilePartyModuleConfiguration<TModule>)
                    .GetStorageProvider();
                storageProvider.WriteProgressEvent += _relay.RelayWriteProgressEvent;
                return storageProvider;
            }
            catch (Exception)
            {
                throw Errors.SPNotFound;
            }
        }

        public IStorageProvider GetStorageProvider<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new()
        {
            if (!(_moduleConfigurations.LastOrDefault(f => f is FilePartyModuleConfiguration<TModule>)
                    is FilePartyModuleConfiguration<TModule> modCfg))
            {
                throw Errors.SPNotFound;
            }

            var storageProvider = modCfg.GetStorageProvider(configuration);
            storageProvider.WriteProgressEvent += _relay.RelayWriteProgressEvent;
            return storageProvider;
        }

        public IStorageReader GetStorageReader()
        {
            return GetStorageProvider();
        }

        public IStorageReader GetStorageReader<TModule>() where TModule : class, IFilePartyModule, new()
        {
            return GetStorageProvider<TModule>();
        }

        public IStorageReader GetStorageReader<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new()
        {
            return GetStorageProvider(configuration);
        }

        public IStorageWriter GetStorageWriter()
        {
            return GetStorageProvider();
        }

        public IStorageWriter GetStorageWriter<TModule>() where TModule : class, IFilePartyModule, new()
        {
            return GetStorageProvider<TModule>();
        }

        public IStorageWriter GetStorageWriter<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new()
        {
            return GetStorageProvider<TModule>();
        }
    }
}