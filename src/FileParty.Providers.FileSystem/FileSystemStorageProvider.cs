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
    public class FileSystemStorageProvider : IStorageProvider
    {
        public void Dispose()
        {
            // nothing to dispose
        }

        public async Task WriteAsync(string storagePointer, Stream stream, WriteMode writeMode,
            CancellationToken cancellationToken = default)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

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

            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

            if (writeStream.CanSeek) writeStream.Seek(0, SeekOrigin.End);

            var buffer = new byte[4096];
            long totalBytesWritten = 0;
            var streamSize = stream.Length;
            int bytesRead;

            do
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                await writeStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                await writeStream.FlushAsync(cancellationToken);
                totalBytesWritten += bytesRead;

                WriteProgressEvent?.Invoke(this, new WriteProgressEventArgs
                {
                    StoragePointer = storagePointer,
                    TotalBytesTransferred = totalBytesWritten,
                    TotalBytesRemaining = streamSize - totalBytesWritten,
                    TotalFileBytes = streamSize,
                    PercentComplete = (int) Math.Round((double) (totalBytesWritten * 100) / streamSize)
                });
            } while (bytesRead > 0 && totalBytesWritten < streamSize);
        }

        public Task<Stream> ReadAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            if (!TryGetStoredItemType(storagePointer, out var type)) throw Errors.FileNotFoundException;

            if (type != StoredItemType.File) throw Errors.MustBeFile;

            using Stream fs = File.OpenRead(storagePointer);
            return Task.FromResult(fs);
        }

        public Task DeleteAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            if (!TryGetStoredItemType(storagePointer, out var type) || type == null)
                throw Errors.FileNotFoundException;

            if (type is StoredItemType.File)
                File.Delete(storagePointer);
            else if (type is StoredItemType.Directory) Directory.Delete(storagePointer, true);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(IEnumerable<string> storagePointers, CancellationToken cancellationToken = default)
        {
            foreach (var storagePointer in storagePointers)
            {
                if (!TryGetStoredItemType(storagePointer, out var type) || type == null)
                    continue;
                
                if (type is StoredItemType.File)
                    File.Delete(storagePointer);
                else if (type is StoredItemType.Directory) Directory.Delete(storagePointer, true);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                TryGetStoredItemType(storagePointer, out var _));
        }

        public Task<IDictionary<string, bool>> ExistsAsync(IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default)
        {
            IDictionary<string, bool> result = storagePointers
                .ToDictionary(
                    k => k,
                    v => ExistsAsync(v, cancellationToken).Result);

            return Task.FromResult(result);
        }

        public Task<IStoredItemInformation> GetInformation(string storagePointer,
            CancellationToken cancellationToken = default)
        {
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

        public event EventHandler<WriteProgressEventArgs> WriteProgressEvent;

        public bool TryGetStoredItemType(string storagePointer, out StoredItemType? type)
        {
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
    }
}