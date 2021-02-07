using System.Threading.Tasks;

namespace FileParty.Core.Interfaces
{
    public interface IFilePartyModuleConfiguration
    {
        /// <summary>
        /// Gets storage provider with default configuration
        /// </summary>
        /// <returns>IStorageProvider</returns>
        IStorageProvider GetStorageProvider();

        /// <summary>
        /// Gets async storage provider with default configuration
        /// </summary>
        /// <returns></returns>
        Task<IAsyncStorageProvider> GetAsyncStorageProvider();
    }
}