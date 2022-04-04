using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;

namespace FileParty.Core
{
    public abstract class BaseAsyncStorageProvider<TModule> : IAsyncStorageProvider
        where TModule : class, IFilePartyModule, new()
    {
        private readonly Guid _instanceId = Guid.NewGuid();

        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly StorageProviderConfiguration<TModule> Configuration;

        /// <summary>
        ///     Constructor for Storage Provider.
        /// </summary>
        /// <param name="configuration">Configuration for storage providers (writers and readers) in a module</param>
        protected BaseAsyncStorageProvider(StorageProviderConfiguration<TModule> configuration)
        {
            Debug.WriteLine($"{GetType().Name} - {_instanceId}: Created at {DateTime.UtcNow:O}");
            Configuration = configuration;
        }

        public virtual char DirectorySeparatorCharacter => Configuration.DirectorySeparationCharacter;

        public abstract Task<Stream> ReadAsync(string storagePointer, CancellationToken cancellationToken = default);

        public abstract Task<bool> ExistsAsync(string storagePointer, CancellationToken cancellationToken = default);

        public abstract Task<IDictionary<string, bool>> ExistsAsync(IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default);

        public abstract Task<StoredItemType?> TryGetStoredItemTypeAsync(string storagePointer,
            CancellationToken cancellationToken = default);

        public abstract Task<IStoredItemInformation> GetInformationAsync(string storagePointer,
            CancellationToken cancellationToken = default);

        public abstract Task WriteAsync(FilePartyWriteRequest request, CancellationToken cancellationToken);

        public virtual async Task WriteAsync(string storagePointer, Stream stream, WriteMode writeMode,
            CancellationToken cancellationToken = default)
        {
            var request = FilePartyWriteRequest.Create(storagePointer, stream, out _, WriteMode.Create);
            await WriteAsync(request, cancellationToken);
        }

        public abstract Task DeleteAsync(string storagePointer, CancellationToken cancellationToken = default);

        public abstract Task DeleteAsync(IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default);

        public abstract event EventHandler<WriteProgressEventArgs> WriteProgressEvent;

        public virtual ValueTask DisposeAsync()
        {
            Debug.WriteLine($"{GetType().Name} - {_instanceId}: Disposed at {DateTime.UtcNow:O}");
            return new ValueTask();
        }
    }
}