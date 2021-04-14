using FileParty.Core.Models;

namespace FileParty.Providers.AWS.S3.Config
{
    /// <summary>
    /// Uses aws credentials default factory
    /// </summary>
    public class AWSDefaultConfiguration : StorageProviderConfiguration<AWS_S3Module>, IAWSBucketInformation
    {
        public string Region { get; set; }
        public string Name { get; set; }
    }
}