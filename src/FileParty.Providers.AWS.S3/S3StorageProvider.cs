using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Config;

namespace FileParty.Providers.AWS.S3
{
    public class S3StorageProvider : IAsyncStorageProvider, IStorageProvider
    {
        
        private StorageProviderConfiguration<AWS_S3Module> _config;

        public S3StorageProvider(StorageProviderConfiguration<AWS_S3Module> awsConfiguration)
        {
            _config = awsConfiguration;
        }

        public virtual void Dispose()
        {
            _config = null;
        }

        public virtual char DirectorySeparatorCharacter { get; } = '/';

        public async Task WriteAsync(FilePartyWriteRequest request, CancellationToken cancellationToken)
        {
            if (await ExistsAsync(request.StoragePointer, cancellationToken) && request.WriteMode == WriteMode.Create)
                throw Errors.FileAlreadyExistsException;

            var transferRequest = new TransferUtilityUploadRequest
            {
                BucketName = GetBucketInfo().Name,
                InputStream = request.Stream,
                Key = request.StoragePointer
            };

            if (WriteProgressEvent != null)
            {
                transferRequest.UploadProgressEvent += (_, args) =>
                {
                    WriteProgressEvent.Invoke(this, new WriteProgressEventArgs(request.Id, request.StoragePointer, args.TransferredBytes, args.TotalBytes));
                };
            }
                
            var creds = GetAmazonCredentials();
            using var s3Client = new AmazonS3Client(creds, GetBucketInfo().GetRegionEndpoint());
            using var transferUtility = new TransferUtility(s3Client);
            await transferUtility.UploadAsync(transferRequest, cancellationToken);
        }

        public virtual async Task WriteAsync(string storagePointer, Stream stream, WriteMode writeMode,
            CancellationToken cancellationToken = default)
        {
            var request = FilePartyWriteRequest.Create(storagePointer, stream, out _, writeMode);
            await WriteAsync(request, cancellationToken);
        }

        public virtual async Task<Stream> ReadAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            // check if exists / throw
            await GetInformationAsync(storagePointer, cancellationToken);

            var getRequest = new GetObjectRequest
            {
                BucketName = GetBucketInfo().Name,
                Key = storagePointer
            };

            using var s3Client = new AmazonS3Client(GetAmazonCredentials(), GetBucketInfo().GetRegionEndpoint());
            using var response = await s3Client.GetObjectAsync(getRequest, cancellationToken);
            var responseStream = response.ResponseStream;
            return responseStream;
        }

        public virtual async Task DeleteAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            var info = await GetInformationAsync(storagePointer, cancellationToken);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = GetBucketInfo().Name,
                Key = storagePointer
            };

            using var s3Client = new AmazonS3Client(GetAmazonCredentials(), GetBucketInfo().GetRegionEndpoint());

            if (info.StoredType == StoredItemType.File)
            {
                await s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            }
            else
            {
                var prefix = storagePointer.EndsWith(DirectorySeparatorCharacter)
                    ? storagePointer
                    : storagePointer + DirectorySeparatorCharacter;

                while (true)
                {
                    var directoryContents = await s3Client
                        .ListObjectsV2Async(new ListObjectsV2Request
                        {
                            BucketName = GetBucketInfo().Name,
                            MaxKeys = 1000,
                            Prefix = prefix,

                        }, cancellationToken)
                        .ConfigureAwait(false);

                    if (!directoryContents.S3Objects.Any()) break;

                    await DeleteAsync(directoryContents.S3Objects.Select(s => s.Key).ToArray(), cancellationToken);
                }
            }
        }

        public virtual async Task DeleteAsync(IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default)
        {
            var spArray = storagePointers as string[] ?? storagePointers.ToArray();

            if (spArray.Length == 0) return;

            var storagePointerTypeDict = spArray.ToDictionary(
                s => s,
                s => TryGetStoredItemType(s, out var type) ? type : null);

            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = GetBucketInfo().Name,
                Objects = storagePointerTypeDict
                    .Where(w=>w.Value == StoredItemType.File)
                    .Select(s => new KeyVersion {Key = s.Key})
                    .ToList()
            };
            using var s3Client = new AmazonS3Client(GetAmazonCredentials(), GetBucketInfo().GetRegionEndpoint());
            await s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);

            foreach (var dir in storagePointerTypeDict
                .Where(w => w.Value == StoredItemType.Directory))
            {
                var prefix = dir.Key.EndsWith(DirectorySeparatorCharacter)
                    ? dir.Key
                    : dir.Key + DirectorySeparatorCharacter;
                
                while (true)
                {
                    var directoryContents = await s3Client
                        .ListObjectsV2Async(new ListObjectsV2Request
                        {
                            BucketName = GetBucketInfo().Name,
                            MaxKeys = 1000,
                            Prefix = prefix,

                        }, cancellationToken)
                        .ConfigureAwait(false);

                    if (!directoryContents.S3Objects.Any()) break;

                    await DeleteAsync(directoryContents.S3Objects.Select(s => s.Key).ToArray(), cancellationToken);
                }
            }
        }

        public virtual async Task<bool> ExistsAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            try
            {
                await GetInformationAsync(storagePointer, cancellationToken);
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
            IDictionary<string, bool> result = storagePointers
                .ToDictionary(
                    k => k,
                    v => ExistsAsync(v, cancellationToken).Result);

            return Task.FromResult(result);
        }

        public async Task<StoredItemType?> TryGetStoredItemTypeAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            StoredItemType? type = null;
            try
            {
                var info = await GetInformationAsync(storagePointer, cancellationToken);
                type = info.StoredType;
                return type;
            }
            catch
            {
                return type;
            }
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

        public virtual bool TryGetStoredItemType(string storagePointer, out StoredItemType? type)
        {
            type = TryGetStoredItemTypeAsync(storagePointer, CancellationToken.None).Result;
            return type != null;
        }

        public virtual IStoredItemInformation GetInformation(string storagePointer)
        {
            return GetInformationAsync(storagePointer).Result;
        }

        public virtual async Task<IStoredItemInformation> GetInformationAsync(string storagePointer,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var s3Client = new AmazonS3Client(GetAmazonCredentials(), GetBucketInfo().GetRegionEndpoint());

                var result = new StoredItemInformation();


                try
                {
                    var omInfo = await s3Client
                        .GetObjectMetadataAsync(GetBucketInfo().Name, storagePointer, cancellationToken)
                        .ConfigureAwait(false);

                    result.StoredType = StoredItemType.File;
                    result.Size = omInfo.ContentLength;
                    result.LastModifiedTimestamp = omInfo.LastModified.ToUniversalTime();
                    result.StoragePointer = storagePointer;
                }
                catch (AmazonS3Exception s3Exception) when (s3Exception.StatusCode == HttpStatusCode.NotFound)
                {
                    storagePointer = storagePointer.EndsWith(DirectorySeparatorCharacter)
                        ? storagePointer
                        : storagePointer + DirectorySeparatorCharacter;

                    var loInfo = await s3Client
                        .ListObjectsAsync(GetBucketInfo().Name, storagePointer, cancellationToken)
                        .ConfigureAwait(false);

                    if (!loInfo.S3Objects.Any()) throw;

                    result.StoredType = StoredItemType.Directory;
                    result.Size = null;
                }

                var pathParts =
                    storagePointer
                        .Split(DirectorySeparatorCharacter, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                var name = pathParts.Last();
                pathParts.Remove(name);
                var dirPath = string.Join(DirectorySeparatorCharacter, pathParts);

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
            catch (Exception)
            {
                throw Errors.UnknownException;
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

        public virtual event EventHandler<WriteProgressEventArgs> WriteProgressEvent;

        protected virtual AWSCredentials GetAmazonCredentials()
        {
            if (_config is AWSAccessKeyConfiguration accessKeyConfiguration)
            {
                return new BasicAWSCredentials(accessKeyConfiguration.AccessKey, accessKeyConfiguration.SecretKey);
            }

            throw Errors.InvalidConfiguration;
        }

        protected virtual IAWSBucketInformation GetBucketInfo()
        {
            if (_config is IAWSBucketInformation bucketInfo)
            {
                return bucketInfo;
            }

            throw Errors.InvalidConfiguration;
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask();
        }
    }
}