using Amazon.S3;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Config;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3
{
    public class FilePartyS3ClientFactory : IFilePartyS3ClientFactory
    {
        private readonly IFilePartyAWSCredentialFactory _awsCredentialFactory;
        private readonly IFilePartyS3BucketInformationProvider _bucketInfoProvider;
        private readonly StorageProviderConfiguration<AWS_S3Module> _config;

        public FilePartyS3ClientFactory(
            StorageProviderConfiguration<AWS_S3Module> config,
            IFilePartyAWSCredentialFactory awsCredentialFactory,
            IFilePartyS3BucketInformationProvider bucketInfoProvider)
        {
            _config = config;
            _awsCredentialFactory = awsCredentialFactory;
            _bucketInfoProvider = bucketInfoProvider;
        }

        public virtual AmazonS3Client GetClient()
        {
            return GetClient(_config);
        }

        public virtual AmazonS3Client GetClient(StorageProviderConfiguration<AWS_S3Module> config)
        {
            return new AmazonS3Client(
                _awsCredentialFactory.GetAmazonCredentials(config),
                _bucketInfoProvider.GetBucketInfo(config).GetRegionEndpoint()
            );
        }
    }
}