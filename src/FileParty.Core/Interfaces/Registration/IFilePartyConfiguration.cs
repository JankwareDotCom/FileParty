using FileParty.Core.Interfaces;
using FileParty.Core.Models;

namespace FileParty.Core.Registration
{
    public interface IFilePartyConfiguration
    {
        /// <summary>
        ///     Add a module with a default storage provider configuration
        /// </summary>
        /// <param name="defaultStorageProviderConfiguration">default configuration for this type of storage provider</param>
        /// <typeparam name="TModule">module type</typeparam>
        /// <returns>IFilePartyConfiguration (this)</returns>
        IFilePartyConfiguration AddModule<TModule>(
            StorageProviderConfiguration<TModule> defaultStorageProviderConfiguration)
            where TModule : class, IFilePartyModule, new();

        /// <summary>
        ///     Sets a module as default, for which all non-specific factory calls return storage providers, writers, and readers.
        ///     If this method is called multiple times, the last call will be the configured value.
        ///     If the type of module has not been registered a runtime error will be thrown.
        /// </summary>
        /// <typeparam name="TModule">module type</typeparam>
        /// <returns>IFilePartyConfiguration (this)</returns>
        IFilePartyConfiguration SetDefaultModule<TModule>() where TModule : class, IFilePartyModule, new();
    }
}