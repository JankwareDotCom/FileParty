using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;
using FileParty.Core.Models;

namespace FileParty.Core.Interfaces
{
    public interface IAsyncStorageWriter : IUseDirectorySeparatorCharacter
    {
        /// <summary>
        /// Write a stream to a storage provider
        /// </summary>
        /// <param name="request">Write Request Model</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        Task WriteAsync(FilePartyWriteRequest request, CancellationToken cancellationToken);
        
        /// <summary>
        /// Write a stream to a storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path to store the file</param>
        /// <param name="stream">Stream to store, assumes you manage disposal</param>
        /// <param name="writeMode">Determine the write type.  Create, Replace, or CreateOrReplace</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Identifier for tracking write progress for this specific call</returns>
        Task WriteAsync(
            string storagePointer,
            Stream stream,
            WriteMode writeMode,
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
        /// EventHandler for Write Progress, uses <see cref="WriteProgressEventArgs"/>
        /// </summary>
        event EventHandler<WriteProgressEventArgs> WriteProgressEvent;
    }
}