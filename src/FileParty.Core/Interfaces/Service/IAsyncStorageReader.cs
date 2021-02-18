using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;

namespace FileParty.Core.Interfaces
{
    public interface IAsyncStorageReader : IUseDirectorySeparatorCharacter
    {
        /// <summary>
        /// Get a stream of a file from the storage provider; This will need disposed.
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        Task<Stream> ReadAsync(
            string storagePointer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines if the file exists in the storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>True when the file exists</returns>
        Task<bool> ExistsAsync(
            string storagePointer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines if files exists in the storage pointer
        /// </summary>
        /// <param name="storagePointers">Generally the path where the file is stored, as an enumerable</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Dictionary with Storage Pointer as key, and boolean indication existence as value</returns>
        Task<IDictionary<string, bool>> ExistsAsync(
            IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tries to get a stored item type (Directory / File)
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>StoredItemType or null when unable</returns>
        Task<StoredItemType?> TryGetStoredItemTypeAsync(
            string storagePointer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns basic information about a stored file or directory
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns><see cref="IStoredItemInformation"/></returns>
        Task<IStoredItemInformation> GetInformationAsync(
            string storagePointer,
            CancellationToken cancellationToken = default);
    }
}