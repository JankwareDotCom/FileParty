using System.Collections.Generic;
using System.Linq;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Core.Registration;

namespace FileParty.Core.Factory
{
    public class FilePartyFactoryModifier : IFilePartyFactoryModifier
    {
        private readonly IConfiguredFileParty _filePartyConfiguration;
        private readonly IEnumerable<IFilePartyModuleConfiguration> _moduleConfigurations;

        public FilePartyFactoryModifier(
            IConfiguredFileParty filePartyConfiguration,
            IEnumerable<IFilePartyModuleConfiguration> moduleConfigurations)
        {
            _filePartyConfiguration = filePartyConfiguration;
            _moduleConfigurations = moduleConfigurations;
        }

        public void ChangeDefaultModuleConfig<TModule>(StorageProviderConfiguration<TModule> configuration = null)
            where TModule : class, IFilePartyModule, new()
        {
            _filePartyConfiguration.DefaultModuleType = typeof(TModule);

            if (configuration == null) return;

            if (!(_moduleConfigurations
                        .FirstOrDefault(f =>
                            f is FilePartyModuleConfiguration<TModule>)
                    is FilePartyModuleConfiguration<TModule> moduleConfiguration))
            {
                throw Errors.SPNotFound;
            }

            moduleConfiguration.SetDefaultConfiguration(configuration);
        }
    }
}