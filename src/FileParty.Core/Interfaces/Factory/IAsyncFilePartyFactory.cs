using System.Threading.Tasks;
using FileParty.Core.Models;

namespace FileParty.Core.Interfaces
{
    public interface IAsyncFilePartyFactory
    {
        /// <summary>
        /// Gets default or last registered storage provider with default configuration
        /// </summary>
        /// <returns>IAsyncStorageProvider</returns>
        Task<IAsyncStorageProvider> GetAsyncStorageProvider();

        /// <summary>
        /// Gets storage provider from a designated module with default configuration
        /// </summary>
        /// <typeparam name="TModule">Module to which the storage provider belongs</typeparam>
        /// <returns>IAsyncStorageProvider</returns>
        Task<IAsyncStorageProvider> GetAsyncStorageProvider<TModule>() 
            where TModule : class, IFilePartyModule, new();

        /// <summary>
        /// Gets storage provider from a designated module with a passed-in configuration
        /// </summary>
        /// <param name="configuration">configuration for storage provider</param>
        /// <typeparam name="TModule">Module to which the storage provider belongs</typeparam>
        /// <returns>IAsyncStorageProvider</returns>
        Task<IAsyncStorageProvider> GetAsyncStorageProvider<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new();
        
        /// <summary>
        /// Gets storage reader from the default or last registered storage provider with default configuration
        /// </summary>
        /// <returns>IAsyncStorageReader</returns>
        Task<IAsyncStorageReader> GetAsyncStorageReader();

        /// <summary>
        /// Gets storage reader from a designated module with default configuration
        /// </summary>
        /// <typeparam name="TModule">Module to which the storage reader belongs</typeparam>
        /// <returns>IAsyncStorageReader</returns>
        Task<IAsyncStorageReader> GetAsyncStorageReader<TModule>() 
            where TModule : class, IFilePartyModule, new();

        /// <summary>
        /// Gets storage reader from a designated module with a passed-in configuration
        /// </summary>
        /// <param name="configuration">configuration for storage reader</param>
        /// <typeparam name="TModule">Module to which the storage reader belongs</typeparam>
        /// <returns>IAsyncStorageReader</returns>
        Task<IAsyncStorageReader> GetAsyncStorageReader<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new();
        
        /// <summary>
        /// Gets storage writer from the default or last registered storage provider with default configuration
        /// </summary>
        /// <returns>IAsyncStorageReader</returns>
        Task<IAsyncStorageWriter> GetAsyncStorageWriter();

        /// <summary>
        /// Gets storage writer from a designated module with default configuration
        /// </summary>
        /// <typeparam name="TModule">Module to which the storage writer belongs</typeparam>
        /// <returns>IAsyncStorageReader</returns>
        Task<IAsyncStorageWriter> GetAsyncStorageWriter<TModule>() 
            where TModule : class, IFilePartyModule, new();

        /// <summary>
        /// Gets storage writer from a designated module with a passed-in configuration
        /// </summary>
        /// <param name="configuration">configuration for storage writer</param>
        /// <typeparam name="TModule">Module to which the storage writer belongs</typeparam>
        /// <returns>IAsyncStorageReader</returns>
        Task<IAsyncStorageWriter> GetAsyncStorageWriter<TModule>(StorageProviderConfiguration<TModule> configuration)
            where TModule : class, IFilePartyModule, new();
    }
}