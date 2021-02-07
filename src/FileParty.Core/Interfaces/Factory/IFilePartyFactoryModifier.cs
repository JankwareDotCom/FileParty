using FileParty.Core.Models;

namespace FileParty.Core.Interfaces
{
    public interface IFilePartyFactoryModifier
    {
        /// <summary>
        /// Replaces the storage provider returned when calling <see cref="IFilePartyFactory.GetStorageProvider()"/> or
        /// <see cref="IAsyncFilePartyFactory.GetAsyncStorageProvider()"/>
        /// </summary>
        /// <param name="configuration">Storage Provider configuration (optional, only required if re-configuration is desired)</param>
        /// <typeparam name="TModule">Module to which the storage providers belong</typeparam>
        void ChangeDefaultModuleConfig<TModule>(StorageProviderConfiguration<TModule> configuration = null)
            where TModule : class, IFilePartyModule, new();
    }
}