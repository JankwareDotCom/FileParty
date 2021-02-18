using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;

namespace FileParty.Providers.FileSystem
{
    public class FileSystemStorageProvider : IAsyncStorageProvider, IStorageProvider
    {
        private StorageProviderConfiguration<FileSystemModule> _config;

        public FileSystemStorageProvider(StorageProviderConfiguration<FileSystemModule> config)
        {
            _config = config;
            DirectorySeparatorCharacter = config.DirectorySeparationCharacter;
        }
        
        public virtual char DirectorySeparatorCharacter { get; }

        public async Task WriteAsync(FilePartyWriteRequest request, CancellationToken cancellationToken)
        {
            var storagePointer = request.StoragePointer;
            var writeMode = request.WriteMode;
            
            ApplyBaseDirectoryToStoragePointer(ref storagePointer);
            
            if (request.Stream is null) throw new ArgumentNullException(nameof(request), "Request Stream may not be null");
            
            var exists = await ExistsAsync(storagePointer, cancellationToken);

            switch (exists)
            {
                case true when writeMode == WriteMode.Create:
                    throw Errors.FileAlreadyExistsException;
                case false when writeMode == WriteMode.Replace:
                    throw Errors.FileNotFoundException;
            }

            var directoryPath = Path.GetDirectoryName(storagePointer);

            if (!string.IsNullOrWhiteSpace(directoryPath)) Directory.CreateDirectory(directoryPath);

            await using var writeStream = File.Create(storagePointer);

            if (request.Stream.CanSeek) request.Stream.Seek(0, SeekOrigin.Begin);

            if (writeStream.CanSeek) writeStream.Seek(0, SeekOrigin.End);

            var buffer = new byte[4096];
            long totalBytesWritten = 0;
            var streamSize = request.Stream.Length;
            int bytesRead;

            do
            {
                bytesRead = await request.Stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                await writeStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                await writeStream.FlushAsync(cancellationToken);
                totalBytesWritten += bytesRead;

                WriteProgressEvent?.Invoke(this, new WriteProgressEventArgs(request.Id, storagePointer, totalBytesWritten, streamSize));
            } while (bytesRead > 0 && totalBytesWritten < streamSize);
        }

        public virtual async Task WriteAsync(string storagePointer, Stream stream, WriteMode writeMode, CancellationToken cancellationToken = default)
        {
            var request = FilePartyWriteRequest.Create(storagePointer, stream, out _, writeMode);
            await WriteAsync(request, cancellationToken);
        }

        public virtual Task<Stream> ReadAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            ApplyBaseDirectoryToStoragePointer(ref storagePointer);
            
            if (!TryGetStoredItemType(storagePointer, out var type)) throw Errors.FileNotFoundException;

            if (type != StoredItemType.File) throw Errors.MustBeFile;

            Stream fs = File.OpenRead(storagePointer);
            return Task.FromResult(fs);
        }

        public virtual Task DeleteAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            ApplyBaseDirectoryToStoragePointer(ref storagePointer);
            
            if (!TryGetStoredItemType(storagePointer, out var type) || type == null)
                throw Errors.FileNotFoundException;

            if (type is StoredItemType.File)
                File.Delete(storagePointer);
            else if (type is StoredItemType.Directory) Directory.Delete(storagePointer, true);

            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(IEnumerable<string> storagePointers, CancellationToken cancellationToken = default)
        {
            var storagePointerArray = storagePointers
                .Select(s => ApplyBaseDirectoryToStoragePointer(ref s))
                .ToArray();
            
            foreach (var storagePointer in storagePointerArray)
            {
                if (!TryGetStoredItemType(storagePointer, out var type) || type == null)
                    continue;
                
                if (type is StoredItemType.File)
                    File.Delete(storagePointer);
                else if (type is StoredItemType.Directory) Directory.Delete(storagePointer, true);
            }

            return Task.CompletedTask;
        }

        public virtual Task<bool> ExistsAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            ApplyBaseDirectoryToStoragePointer(ref storagePointer);
            
            return Task.FromResult(
                TryGetStoredItemType(storagePointer, out var _));
        }

        public virtual Task<IDictionary<string, bool>> ExistsAsync(IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default)
        {
            var storagePointerArray = storagePointers
                .Select(s => ApplyBaseDirectoryToStoragePointer(ref s))
                .ToArray();
            
            IDictionary<string, bool> result = storagePointerArray
                .ToDictionary(
                    k => k,
                    v => ExistsAsync(v, cancellationToken).Result);

            return Task.FromResult(result);
        }

        public async Task<StoredItemType?> TryGetStoredItemTypeAsync(string storagePointer,
            CancellationToken cancellationToken = default)
        {
            TryGetStoredItemType(storagePointer, out var type);
            return type;
        }

        public virtual Task<IStoredItemInformation> GetInformationAsync(string storagePointer,
            CancellationToken cancellationToken = default)
        {
            ApplyBaseDirectoryToStoragePointer(ref storagePointer);
            
            if (!TryGetStoredItemType(storagePointer, out var type) || type == null) return null;

            var fileInfo = new FileInfo(storagePointer);
            var directoryPath = Path.GetDirectoryName(storagePointer);

            IStoredItemInformation result = new StoredItemInformation
            {
                StoredType = (StoredItemType) type,
                DirectoryPath = directoryPath,
                Name = fileInfo.Name,
                StoragePointer = fileInfo.FullName,
                Size = fileInfo.Length,
                CreatedTimestamp = fileInfo.CreationTimeUtc,
                LastModifiedTimestamp = fileInfo.LastWriteTimeUtc
            };

            return Task.FromResult(result);
        }

        public virtual event EventHandler<WriteProgressEventArgs> WriteProgressEvent;

        public virtual bool TryGetStoredItemType(string storagePointer, out StoredItemType? type)
        {
            ApplyBaseDirectoryToStoragePointer(ref storagePointer);
            
            type = null;

            try
            {
                if (File.Exists(storagePointer))
                    type = StoredItemType.File;
                else if (Directory.Exists(storagePointer)) type = StoredItemType.Directory;

                return type != null;
            }
            catch
            {
                return false;
            }
        }

        public void Write(FilePartyWriteRequest request)
        {
            WriteAsync(request, CancellationToken.None).Wait();
        }

        public virtual void Write(string storagePointer, Stream stream, WriteMode writeMode)
        {
            Write(new FilePartyWriteRequest(storagePointer, stream, writeMode));
        }

        public virtual void Delete(string storagePointer)
        {
            DeleteAsync(storagePointer).Wait();
        }

        public virtual void Delete(IEnumerable<string> storagePointers)
        {
            DeleteAsync(storagePointers).Wait();
        }

        public virtual Stream Read(string storagePointer)
        {
            return ReadAsync(storagePointer).Result;
        }

        public virtual bool Exists(string storagePointer)
        {
            return ExistsAsync(storagePointer).Result;
        }

        public virtual IDictionary<string, bool> Exists(IEnumerable<string> storagePointers)
        {
            return ExistsAsync(storagePointers).Result;
        }

        public virtual IStoredItemInformation GetInformation(string storagePointer)
        {
            return GetInformationAsync(storagePointer).Result;
        }

        protected virtual string GetBasePath()
        {
            if (_config is FileSystemConfiguration fsc)
            {
                return fsc.BasePath;
            }

            throw Errors.InvalidConfiguration;
        }

        protected virtual string ApplyBaseDirectoryToStoragePointer(ref string storagePointer)
        {
            var basePath = GetBasePath();
            
            if (string.IsNullOrWhiteSpace(storagePointer))
            {
                throw Errors.StoragePointerMustHaveValue;
            }
            
            if (string.IsNullOrWhiteSpace(basePath))
            {
                return storagePointer;
            }
            
            if (storagePointer.StartsWith(basePath + DirectorySeparatorCharacter))
            {
                return storagePointer;
            }

            storagePointer = basePath + DirectorySeparatorCharacter + storagePointer;

            return storagePointer;
        }
    }
}