using Amazon.S3;
using FileParty.Core.Models;

namespace FileParty.Providers.AWS.S3.Interfaces
{
    /// <summary>
    ///     Provides direct access to AWS S3 Client, but using FileParty.Providers.AWS.S3 Configurations
    /// </summary>
    public interface IFilePartyS3ClientFactory
    {
        /// <summary>
        ///     Gets <see cref="AmazonS3Client" /> using default module configuration
        /// </summary>
        AmazonS3Client GetClient();

        /// <summary>
        ///     Gets <see cref="AmazonS3Client" /> using passed in module configuration
        /// </summary>
        AmazonS3Client GetClient(StorageProviderConfiguration<AWS_S3Module> config);
    }
}