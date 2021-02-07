using FileParty.Core.Models;

namespace FileParty.Providers.AWS.S3.Config
{
    public class AWSAccessKeyConfiguration : StorageProviderConfiguration<S3StorageProvider>, IAWSBucketInformation
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
    }
}