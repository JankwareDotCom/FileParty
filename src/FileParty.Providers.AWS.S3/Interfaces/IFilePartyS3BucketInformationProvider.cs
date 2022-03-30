using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Config;

namespace FileParty.Providers.AWS.S3.Interfaces
{
    public interface IFilePartyS3BucketInformationProvider
    {
        /// <summary>
        /// Gets bucket info for default configuration
        /// </summary>
        IAWSBucketInformation GetBucketInfo();
        
        /// <summary>
        /// Gets bucket info for passed in configuration
        /// </summary>
        /// <param name="config">Module Configuration</param>
        IAWSBucketInformation GetBucketInfo(StorageProviderConfiguration<AWS_S3Module> config);
    }
}