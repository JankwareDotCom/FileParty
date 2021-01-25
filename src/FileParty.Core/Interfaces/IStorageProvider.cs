using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;

namespace FileParty.Core.Interfaces
{
    /// <summary>
    /// A common interface for storage, agnostic to cloud or local hosting.
    /// </summary>
    public interface IStorageProvider : IDisposable
    {
        /// <summary>
        /// Directory Separator Character
        /// In some instances, using Path.DirectorySeparatorCharacter will do the trick, but in others, it should be explicitly defined.
        /// </summary>
        char DirectorySeparatorCharacter { get; }
        
        /// <summary>
        /// Write a stream to a storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path to store the file</param>
        /// <param name="stream">Stream to store, assumes you manage disposal</param>
        /// <param name="writeMode">Determine the write type.  Create, Replace, or CreateOrReplace</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        Task WriteAsync(
            string storagePointer,
            Stream stream,
            WriteMode writeMode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a stream of a file from the storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        Task<Stream> ReadAsync(
            string storagePointer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a file from the storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        Task DeleteAsync(
            string storagePointer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete many files from the storage provider
        /// </summary>
        /// <param name="storagePointers">Generally the path where the file is stored, as an enumerable</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        Task DeleteAsync(
            IEnumerable<string> storagePointers,
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
        /// Determines if files exists in the storage prointer
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
        /// <param name="type">Output of stored item type, returns File or Directory on success, null on failure</param>
        /// <returns>True when the file exists</returns>
        bool TryGetStoredItemType(string storagePointer, out StoredItemType? type);

        /// <summary>
        /// Returns basic information about a stored file or directory
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns><see cref="IStoredItemInformation"/></returns>
        Task<IStoredItemInformation> GetInformation(string storagePointer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// EventHandler for Write Progress, uses <see cref="WriteProgressEventArgs"/>
        /// </summary>
        event EventHandler<WriteProgressEventArgs> WriteProgressEvent;
    }
}