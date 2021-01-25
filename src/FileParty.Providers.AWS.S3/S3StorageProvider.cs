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
    public class S3StorageProvider : IStorageProvider
    {
        
        private string _configType;
        private readonly AWSAccessKeyConfiguration _accessKeyConfig;
        private readonly AWSBucketInformation _bucketInfo;

        public S3StorageProvider(AWSAccessKeyConfiguration awsAccessKeyConfiguration)
        {
            _configType = nameof(AWSAccessKeyConfiguration);
            _accessKeyConfig = awsAccessKeyConfiguration;
            _bucketInfo = awsAccessKeyConfiguration;
        }

        public void Dispose()
        {
            _configType = null;
        }

        public char DirectorySeparatorCharacter { get; } = '/';

        public async Task WriteAsync(string storagePointer, Stream stream, WriteMode writeMode,
            CancellationToken cancellationToken = default)
        {
            if (await ExistsAsync(storagePointer, cancellationToken) && writeMode == WriteMode.Create)
                throw Errors.FileAlreadyExistsException;

            var transferRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucketInfo.Name,
                InputStream = stream,
                Key = storagePointer
            };

            if (WriteProgressEvent != null)
                transferRequest.UploadProgressEvent += (_, args) =>
                {
                    WriteProgressEvent.Invoke(this, new WriteProgressEventArgs
                    {
                        StoragePointer = storagePointer,
                        TotalBytesTransferred = args.TransferredBytes,
                        TotalBytesRemaining = stream.Length - args.TransferredBytes,
                        TotalFileBytes = stream.Length,
                        PercentComplete = args.PercentDone
                    });
                };

            var creds = GetAmazonCredentials();
            using var s3Client = new AmazonS3Client(creds, _bucketInfo.GetRegionEndpoint());
            using var transferUtility = new TransferUtility(s3Client);
            await transferUtility.UploadAsync(transferRequest, cancellationToken);
        }

        public async Task<Stream> ReadAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            // check if exists / throw
            await GetInformation(storagePointer, cancellationToken);

            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketInfo.Name,
                Key = storagePointer
            };

            using var s3Client = new AmazonS3Client(GetAmazonCredentials(), _bucketInfo.GetRegionEndpoint());
            using var response = await s3Client.GetObjectAsync(getRequest, cancellationToken);
            await using var responseStream = response.ResponseStream;
            return responseStream;
        }

        public async Task DeleteAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            var info = await GetInformation(storagePointer, cancellationToken);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketInfo.Name,
                Key = storagePointer
            };

            using var s3Client = new AmazonS3Client(GetAmazonCredentials(), _bucketInfo.GetRegionEndpoint());

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
                            BucketName = _bucketInfo.Name,
                            MaxKeys = 1000,
                            Prefix = prefix,

                        }, cancellationToken)
                        .ConfigureAwait(false);

                    if (!directoryContents.S3Objects.Any()) break;

                    await DeleteAsync(directoryContents.S3Objects.Select(s => s.Key).ToArray(), cancellationToken);
                }
            }
        }

        public async Task DeleteAsync(IEnumerable<string> storagePointers,
            CancellationToken cancellationToken = default)
        {
            var spArray = storagePointers as string[] ?? storagePointers.ToArray();

            if (spArray.Length == 0) return;

            var storagePointerTypeDict = spArray.ToDictionary(
                s => s,
                s => TryGetStoredItemType(s, out var type) ? type : null);

            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketInfo.Name,
                Objects = storagePointerTypeDict
                    .Where(w=>w.Value == StoredItemType.File)
                    .Select(s => new KeyVersion {Key = s.Key})
                    .ToList()
            };

            using var s3Client = new AmazonS3Client(GetAmazonCredentials(), _bucketInfo.GetRegionEndpoint());
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
                            BucketName = _bucketInfo.Name,
                            MaxKeys = 1000,
                            Prefix = prefix,

                        }, cancellationToken)
                        .ConfigureAwait(false);

                    if (!directoryContents.S3Objects.Any()) break;

                    await DeleteAsync(directoryContents.S3Objects.Select(s => s.Key).ToArray(), cancellationToken);
                }
            }
        }

        public async Task<bool> ExistsAsync(string storagePointer, CancellationToken cancellationToken = default)
        {
            try
            {
                await GetInformation(storagePointer, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        public bool TryGetStoredItemType(string storagePointer, out StoredItemType? type)
        {
            type = null;
            try
            {
                var info = GetInformation(storagePointer).Result;
                type = info.StoredType;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IStoredItemInformation> GetInformation(string storagePointer,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var s3Client = new AmazonS3Client(GetAmazonCredentials(), _bucketInfo.GetRegionEndpoint());

                var result = new StoredItemInformation();


                try
                {
                    var omInfo = await s3Client
                        .GetObjectMetadataAsync(_bucketInfo.Name, storagePointer, cancellationToken)
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
                        .ListObjectsAsync(_bucketInfo.Name, storagePointer, cancellationToken)
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

        public event EventHandler<WriteProgressEventArgs> WriteProgressEvent;

        private AWSCredentials GetAmazonCredentials()
        {
            return _configType switch
            {
                nameof(AWSAccessKeyConfiguration) =>
                    new BasicAWSCredentials(_accessKeyConfig.AccessKey, _accessKeyConfig.SecretKey),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(S3StorageProvider), "Invalid authentication scheme.")
            };
        }
    }
}