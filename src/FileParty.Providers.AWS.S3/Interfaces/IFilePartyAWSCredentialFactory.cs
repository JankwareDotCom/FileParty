using Amazon.Runtime;
using FileParty.Core.Models;

namespace FileParty.Providers.AWS.S3.Interfaces
{
    public interface IFilePartyAWSCredentialFactory
    {
        /// <summary>
        ///     Creates <see cref="AWSCredentials" /> from an AWS_S3Module Config/>
        /// </summary>
        /// <param name="config">Module Configuration</param>
        /// <returns>AWS Credentials for use creating AWS S3 Client (or possibly other AWS SDK Objects)</returns>
        AWSCredentials GetAmazonCredentials(StorageProviderConfiguration<AWS_S3Module> config);
    }
}