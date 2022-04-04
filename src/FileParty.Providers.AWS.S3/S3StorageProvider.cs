using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3
{
    public class S3StorageProvider : IAsyncStorageProvider, IStorageProvider
    {
        private const int BufferSize = 81920;
        private readonly IFilePartyS3BucketInformationProvider _bucketInfoProvider;
        private readonly IFilePartyS3ClientFactory _s3ClientFactory;

        public S3StorageProvider(
            IFilePartyS3ClientFactory s3ClientFactory,
            IFilePartyS3BucketInformationProvider bucketInfoProvider)
        {
            _s3ClientFactory = s3ClientFactory;
            _bucketInfoProvider = bucketInfoProvider;
        }

        public virtual char DirectorySeparatorCharacter { get; } = '/';

        public async Task WriteAsync(FilePartyWriteRequest request, CancellationToken cancellationToken = default)
        {
            var transferRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucketInfoProvider.GetBucketInfo().Name,
                InputStream = request.Stream,
                Key = request.StoragePointer
            };

            if (WriteProgressEvent != null)
            {
                transferRequest.UploadProgressEvent += (_, args) =>
                {
                    WriteProgressEvent.Invoke(
                        this,
                        new WriteProgressEventArgs(
                            request.Id,
                            request.StoragePointer,
                            args.TransferredBytes,
                            args.TotalBytes,
                            request.RequestCreatedAt));
                };
            }

            using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
            {
                await s3Wrapper.ExecuteAsync(async (s3Client) =>
                {
                    if (await ExistsAsync(
                            s3Client,
                            request.StoragePointer,
                            cancellationToken) &&
                        request.WriteMode == WriteMode.Create)
                    {
                        throw Errors.FileAlreadyExistsException;
                    }

                    using (var transferUtility = new TransferUtility(s3Client))
                    {
                        await transferUtility.UploadAsync(transferRequest, cancellationToken);
                    }
                });
            }
        }

        public virtual Task WriteAsync(string storagePointer, Stream stream, WriteMode writeMode,
            CancellationToken cancellationToken = default)
        {
            var request = FilePartyWriteRequest.Create(storagePointer, stream, out _, writeMode);
            return WriteAsync(request, cancellationToken);
        }

        public virtual async Task<Stream> ReadAsync(string storagePointer,
            CancellationToken cancellationToken = default)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketInfoProvider.GetBucketInfo().Name,
                Key = storagePointer
            };

            using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
            {
                return await s3Wrapper.ExecuteAsync<Stream>(async (s3Client) =>
                {
                    using (var response = await s3Client.GetObjectAsync(getRequest, cancellationToken))
                    {
                        // check if exists / throw
                        if (!await ExistsAsync(s3Client, storagePointer, cancellationToken))
                        {
                            throw Errors.FileNotFoundException;
                        }

                        var resultStream = new MemoryStream();
                        await response.ResponseStream.CopyToAsync(resultStream, BufferSize, cancellationToken);
                        resultStream.Position = 0;
                        return resultStream;
                    }
                });
            }
        }

        public virtual async Task DeleteAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
            {
                await s3Wrapper.ExecuteAsync(async (s3Client) =>
                    await DeleteAsync(s3Client, storagePointer, cancellationToken));
            }
        }

        public virtual async Task DeleteAsync(IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default)
        {
            using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
            {
                await s3Wrapper.ExecuteAsync(async (s3Client) =>
                    await DeleteAsync(s3Client, storagePointers, cancellationToken));
            }
        }

        public virtual async Task<bool> ExistsAsync(string storagePointer,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
                {
                    var _ = await s3Wrapper.ExecuteAsync(
                        (s3Client) => GetInformationAsync(s3Client, storagePointer, cancellationToken));
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual Task<IDictionary<string, bool>> ExistsAsync(IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default)
        {
            using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
            {
                return s3Wrapper.ExecuteAsync((s3Client) =>
                {
                    IDictionary<string, bool> result = storagePointers
                        .ToDictionary(
                            k => k,
                            v => ExistsAsync(s3Client, v, cancellationToken).GetAwaiter().GetResult());

                    return Task.FromResult(result);
                });
            }
        }

        public Task<StoredItemType?> TryGetStoredItemTypeAsync(string storagePointer,
            CancellationToken cancellationToken = default)
        {
            using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
            {
                return s3Wrapper.ExecuteAsync((s3Client) =>
                    TryGetStoredItemTypeAsync(s3Client, storagePointer, cancellationToken));
            }
        }

        public virtual async Task<IStoredItemInformation> GetInformationAsync(
            string storagePointer,
            CancellationToken cancellationToken = default)
        {
            using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
            {
                return await s3Wrapper.ExecuteAsync(async (s3Client) =>
                    await GetInformationAsync(
                        s3Client,
                        storagePointer,
                        cancellationToken));
            }
        }

        public virtual event EventHandler<WriteProgressEventArgs> WriteProgressEvent;

        public virtual Stream Read(string storagePointer)
        {
            return ReadAsync(storagePointer).GetAwaiter().GetResult();
        }

        public virtual bool Exists(string storagePointer)
        {
            return ExistsAsync(storagePointer).GetAwaiter().GetResult();
        }

        public virtual IDictionary<string, bool> Exists(IEnumerable<string> storagePointers)
        {
            return ExistsAsync(storagePointers).GetAwaiter().GetResult();
        }

        public virtual bool TryGetStoredItemType(string storagePointer, out StoredItemType? type)
        {
            using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
            {
                StoredItemType? tmpType = null;
                var res = s3Wrapper.Execute((s3Client) =>
                    TryGetStoredItemType(s3Client, storagePointer, out tmpType));
                type = tmpType;
                return res;
            }
        }

        public virtual IStoredItemInformation GetInformation(
            string storagePointer)
        {
            return GetInformationAsync(storagePointer)
                .GetAwaiter()
                .GetResult();
        }

        public void Write(FilePartyWriteRequest request)
        {
            WriteAsync(request, CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual void Write(string storagePointer, Stream stream, WriteMode writeMode)
        {
            Write(new FilePartyWriteRequest(storagePointer, stream, writeMode));
        }

        public virtual void Delete(string storagePointer)
        {
            DeleteAsync(storagePointer).GetAwaiter().GetResult();
        }

        public virtual void Delete(IEnumerable<string> storagePointers)
        {
            DeleteAsync(storagePointers).GetAwaiter().GetResult();
        }

        public virtual void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask();
        }

        #region privateMethods

        protected virtual async Task<StoredItemInformation> GetFileInformation(AmazonS3Client s3Client,
            string storagePointer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var result = new StoredItemInformation();
                var omInfo = await s3Client
                    .GetObjectMetadataAsync(_bucketInfoProvider.GetBucketInfo().Name, storagePointer, cancellationToken)
                    .ConfigureAwait(false);

                result.StoredType = StoredItemType.File;
                result.Size = omInfo.ContentLength;
                result.LastModifiedTimestamp = omInfo.LastModified.ToUniversalTime();
                result.StoragePointer = storagePointer;
                return result;
            }
            catch (ObjectDisposedException e)
            {
                if (e.ObjectName != "Amazon.S3.AmazonS3Client") throw;

                using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
                {
                    return await s3Wrapper.ExecuteAsync((client) =>
                        GetFileInformation(client, storagePointer, cancellationToken));
                }
            }
        }

        protected virtual async Task<StoredItemInformation> GetDirectoryInformation(AmazonS3Client s3Client,
            string storagePointer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var result = new StoredItemInformation();
                storagePointer = storagePointer.EndsWith(DirectorySeparatorCharacter.ToString())
                    ? storagePointer
                    : storagePointer + DirectorySeparatorCharacter;

                var loInfo = await s3Client
                    .ListObjectsAsync(_bucketInfoProvider.GetBucketInfo().Name, storagePointer, cancellationToken)
                    .ConfigureAwait(false);

                if (!loInfo.S3Objects.Any())
                    throw Errors.FileNotFoundException;

                result.StoredType = StoredItemType.Directory;
                result.Size = null;

                return result;
            }
            catch (ObjectDisposedException e)
            {
                if (e.ObjectName != "Amazon.S3.AmazonS3Client") throw;

                using (var s3Wrapper = new FilePartyS3ClientWrapper(_s3ClientFactory))
                {
                    return await s3Wrapper.ExecuteAsync((client) =>
                        GetDirectoryInformation(client, storagePointer, cancellationToken));
                }
            }
        }

        protected virtual async Task<IStoredItemInformation> GetInformationAsync(AmazonS3Client s3Client,
            string storagePointer, CancellationToken cancellationToken = default)
        {
            try
            {
                StoredItemInformation result;

                try
                {
                    result = await GetFileInformation(s3Client, storagePointer, cancellationToken);
                }
                catch (AmazonS3Exception s3Exception) when (s3Exception.StatusCode == HttpStatusCode.NotFound)
                {
                    result = await GetDirectoryInformation(s3Client, storagePointer, cancellationToken);
                }

                var pathParts =
                    storagePointer
                        .Split(DirectorySeparatorCharacter)
                        .Where(part => !string.IsNullOrWhiteSpace(part))
                        .ToList();

                var name = pathParts.Last();
                pathParts.Remove(name);
                var dirPath = string.Join(DirectorySeparatorCharacter.ToString(), pathParts);

                if (result.StoredType == StoredItemType.Directory) name += DirectorySeparatorCharacter;

                result.DirectoryPath = dirPath;
                result.Name = name;
                result.CreatedTimestamp = null;

                return result;
            }
            catch (AmazonS3Exception s3Exception) when (s3Exception.StatusCode == HttpStatusCode.NotFound)
            {
                throw Errors.FileNotFoundException;
            }
        }

        protected virtual async Task<bool> ExistsAsync(AmazonS3Client s3Client, string storagePointer,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await GetInformationAsync(s3Client, storagePointer, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected virtual async Task DeleteAsync(AmazonS3Client s3Client, string storagePointer,
            CancellationToken cancellationToken = default)
        {
            await DeleteAsync(s3Client, new[] {storagePointer}, cancellationToken);
        }

        protected bool TryGetStoredItemType(AmazonS3Client s3Client, string storagePointer, out StoredItemType? type)
        {
            type = TryGetStoredItemTypeAsync(s3Client, storagePointer, CancellationToken.None).GetAwaiter().GetResult();
            return type != null;
        }

        protected async Task<StoredItemType?> TryGetStoredItemTypeAsync(AmazonS3Client s3Client, string storagePointer,
            CancellationToken cancellationToken = default)
        {
            StoredItemType? type = null;
            try
            {
                var info = await GetInformationAsync(s3Client, storagePointer, cancellationToken);
                type = info.StoredType;
                return type;
            }
            catch (Exception)
            {
                return type;
            }
        }

        protected virtual async Task DeleteAsync(AmazonS3Client s3Client, IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default)
        {
            var spArray = storagePointers as string[] ?? storagePointers.ToArray();

            if (spArray.Length == 0) return;

            var storagePointerTypeDict = spArray.ToDictionary(
                s => s,
                s => TryGetStoredItemType(s3Client, s, out var type) ? type : null);

            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketInfoProvider.GetBucketInfo().Name,
                Objects = storagePointerTypeDict
                    .Where(w => w.Value == StoredItemType.File)
                    .Select(s => new KeyVersion {Key = s.Key})
                    .ToList()
            };

            if (deleteRequest.Objects.Any())
            {
                await s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
            }

            foreach (var dir in storagePointerTypeDict
                         .Where(w => w.Value == StoredItemType.Directory))
            {
                var prefix = dir.Key.EndsWith(DirectorySeparatorCharacter.ToString())
                    ? dir.Key
                    : dir.Key + DirectorySeparatorCharacter;

                while (true)
                {
                    var directoryContents = await s3Client
                        .ListObjectsV2Async(new ListObjectsV2Request
                        {
                            BucketName = _bucketInfoProvider.GetBucketInfo().Name,
                            MaxKeys = 1000,
                            Prefix = prefix
                        }, cancellationToken)
                        .ConfigureAwait(false);

                    if (!directoryContents.S3Objects.Any()) break;

                    await DeleteAsync(directoryContents.S3Objects.Select(s => s.Key).ToArray(), cancellationToken);
                }
            }
        }

        #endregion
    }
}