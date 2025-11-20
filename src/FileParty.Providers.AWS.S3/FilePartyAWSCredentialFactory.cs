using System;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime.Credentials;
using Amazon.S3;
using Amazon.SecurityToken;
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
                switch (config)
                {
                    case AWSAccessKeyConfiguration accessKeyConfiguration:
                        return new BasicAWSCredentials(accessKeyConfiguration.AccessKey,
                            accessKeyConfiguration.SecretKey);
                    case AWSSessionCredentials sessionBasedCredentials:
                        return sessionBasedCredentials.GetTemporaryCredentialsAsync(this)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                    case AWSRoleBasedConfiguration roleBasedConfiguration:
                        return roleBasedConfiguration.AssumeRoleAsync(this)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                    case AWSDefaultConfiguration _:
                        return DefaultAWSCredentialsIdentityResolver.GetCredentials();
                    case AWSStoredProfileConfiguration storedProfileConfiguration:
                    {
                        var chain = string.IsNullOrWhiteSpace(storedProfileConfiguration.ProfileLocation)
                            ? new CredentialProfileStoreChain()
                            : new CredentialProfileStoreChain(storedProfileConfiguration.ProfileLocation);

                        var hasCreds = string.IsNullOrWhiteSpace(storedProfileConfiguration.ProfileName)
                            ? chain.TryGetAWSCredentials("default", out var creds)
                            : chain.TryGetAWSCredentials(storedProfileConfiguration.ProfileName, out creds);

                        return hasCreds ? creds : throw Errors.InvalidConfiguration;
                    }
                    case AWSInstanceProfileConfiguration instanceConfiguration:
                        return new InstanceProfileAWSCredentials(instanceConfiguration.Role);
                }
            }
            catch (AmazonSecurityTokenServiceException)
            {
                throw;
            }
            catch (AmazonClientException)
            {
                throw;
            }
            catch (AmazonS3Exception)
            {
                throw;
            }
            catch (Exception)
            {
                throw Errors.InvalidConfiguration;
            }

            throw Errors.InvalidConfiguration;
        }
    }
}