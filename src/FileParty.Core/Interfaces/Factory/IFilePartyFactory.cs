using FileParty.Core.Models;

namespace FileParty.Core.Interfaces
{
    public interface IFilePartyFactory
    {
        /// <summary>
        /// Gets the default or last registered storage provider with default configuration
        /// </summary>
        /// <returns>IStorageProvider</returns>
        IStorageProvider GetStorageProvider();

        /// <summary>
        /// Gets storage provider from a designated module with default configuration
        /// </summary>
        /// <typeparam name="TModule">Module to which the storage provider belongs</typeparam>
        /// <returns>IStorageProvider</returns>
        IStorageProvider GetStorageProvider<TModule>() 
            where TModule : class, IFilePartyModule, new();

        /// <summary>
        /// Gets storage provider from a designated module with a passed-in configuration
        /// </summary>
        /// <param name="configuration">configuration for storage provider</param>
        /// <typeparam name="TModule">Module to which the storage provider belongs</typeparam>
        /// <returns>IStorageProvider</returns>
        IStorageProvider GetStorageProvider<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new();
        
        /// <summary>
        /// Gets storage reader from the default or last registered storage provider with default configuration
        /// </summary>
        /// <returns>IStorageReader</returns>
        IStorageReader GetStorageReader();

        /// <summary>
        /// Gets storage reader from a designated module with default configuration
        /// </summary>
        /// <typeparam name="TModule">Module to which the storage reader belongs</typeparam>
        /// <returns>IStorageReader</returns>
        IStorageReader GetStorageReader<TModule>() 
            where TModule : class, IFilePartyModule, new();

        /// <summary>
        /// Gets storage reader from a designated module with a passed-in configuration
        /// </summary>
        /// <param name="configuration">configuration for storage reader</param>
        /// <typeparam name="TModule">Module to which the storage reader belongs</typeparam>
        /// <returns>IStorageReader</returns>
        IStorageReader GetStorageReader<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new();
        
        /// <summary>
        /// Gets storage writer from the default or last registered storage provider with default configuration
        /// </summary>
        /// <returns>IStorageReader</returns>
        IStorageWriter GetStorageWriter();

        /// <summary>
        /// Gets storage writer from a designated module with default configuration
        /// </summary>
        /// <typeparam name="TModule">Module to which the storage writer belongs</typeparam>
        /// <returns>IStorageReader</returns>
        IStorageWriter GetStorageWriter<TModule>() 
            where TModule : class, IFilePartyModule, new();

        /// <summary>
        /// Gets storage writer from a designated module with a passed-in configuration
        /// </summary>
        /// <param name="configuration">configuration for storage writer</param>
        /// <typeparam name="TModule">Module to which the storage writer belongs</typeparam>
        /// <returns>IStorageReader</returns>
        IStorageWriter GetStorageWriter<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new();
    }
}