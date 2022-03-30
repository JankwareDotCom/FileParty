using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;

namespace FileParty.Core
{
    public abstract class BaseStorageProvider<TModule> : IStorageProvider
        where TModule : class, IFilePartyModule, new()
    {
        private readonly Guid _instanceId = Guid.NewGuid();
        public virtual char DirectorySeparatorCharacter => Configuration.DirectorySeparationCharacter;

        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly StorageProviderConfiguration<TModule> Configuration;
        
        /// <summary>
        /// Constructor for Storage Provider.
        /// </summary>
        /// <param name="configuration">Configuration for storage providers (writers and readers) in a module</param>
        protected BaseStorageProvider(StorageProviderConfiguration<TModule> configuration)
        {
            Debug.WriteLine($"{GetType().Name} - {_instanceId}: Created at {DateTime.UtcNow:O}");
            Configuration = configuration;
        }

        public abstract void Write(FilePartyWriteRequest request);

        public virtual void Write(string storagePointer, Stream stream, WriteMode writeMode)
        {
            var request = FilePartyWriteRequest.Create(storagePointer, stream, out _, writeMode);
            Write(request);
        }
        
        public abstract void Delete(string storagePointer);

        public abstract void Delete(IEnumerable<string> storagePointers);
        
        public abstract Stream Read(string storagePointer);

        public abstract bool Exists(string storagePointer);

        public abstract IDictionary<string, bool> Exists(IEnumerable<string> storagePointers);

        public abstract bool TryGetStoredItemType(string storagePointer, out StoredItemType? type);

        public abstract IStoredItemInformation GetInformation(string storagePointer);

        protected virtual void Dispose()
        {
            Debug.WriteLine($"{GetType().Name} - {_instanceId}: Disposed at {DateTime.UtcNow:O}");
        }

        public abstract event EventHandler<WriteProgressEventArgs> WriteProgressEvent;
    }
}