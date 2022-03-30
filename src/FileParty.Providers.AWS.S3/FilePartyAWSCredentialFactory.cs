using System;
using Amazon.Runtime;
using FileParty.Core.Exceptions;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Config;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3
{
    public class FilePartyAWSCredentialFactory : IFilePartyAWSCredentialFactory
    {
        public virtual AWSCredentials GetAmazonCredentials(StorageProviderConfiguration<AWS_S3Module> config)
        {
            try
            {
                if (config is AWSAccessKeyConfiguration accessKeyConfiguration)
                {
                    return new BasicAWSCredentials(accessKeyConfiguration.AccessKey, accessKeyConfiguration.SecretKey);
                }

                if (config is AWSDefaultConfiguration defaultConfiguration)
                {
                    return FallbackCredentialsFactory.GetCredentials(false);
                }
            }
            catch (Exception)
            {
                throw Errors.InvalidConfiguration;    
            }
            
            throw Errors.InvalidConfiguration;    
        }
    }
}