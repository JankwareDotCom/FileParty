using FileParty.Core.Exceptions;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Config;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3
{
    internal class FilePartyS3BucketInformationProvider : IFilePartyS3BucketInformationProvider
    {
        private readonly StorageProviderConfiguration<AWS_S3Module> _config;

        public FilePartyS3BucketInformationProvider(StorageProviderConfiguration<AWS_S3Module> config)
        {
            _config = config;
        }

        public virtual IAWSBucketInformation GetBucketInfo()
        {
            return GetBucketInfo(_config);
        }

        public virtual IAWSBucketInformation GetBucketInfo(StorageProviderConfiguration<AWS_S3Module> config)
        {
            if (config is IAWSBucketInformation bucketInfo)
            {
                return bucketInfo;
            }

            throw Errors.InvalidConfiguration;
        }
    }
}