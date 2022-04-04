using FileParty.Core.Models;

namespace FileParty.Providers.AWS.S3.Config
{
    public abstract class BaseAWSCredential : StorageProviderConfiguration<AWS_S3Module>, IAWSBucketInformation
    {
        public string Region { get; set; }
        public string Name { get; set; }
    }
}