using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
                        return GetDefaultCredentials();
                    case AWSStoredProfileConfiguration storedProfileConfiguration:
                    {
                        var profileLocation = string.IsNullOrWhiteSpace(storedProfileConfiguration.ProfileLocation)
                            ? null
                            : Directory.Exists(storedProfileConfiguration.ProfileLocation)
                                ? Path.Combine(storedProfileConfiguration.ProfileLocation, "credentials")
                                : storedProfileConfiguration.ProfileLocation;

                        var profileName = string.IsNullOrWhiteSpace(storedProfileConfiguration.ProfileName)
                            ? "default"
                            : storedProfileConfiguration.ProfileName;
                        
                        var chain = string.IsNullOrWhiteSpace(profileLocation)
                            ? new CredentialProfileStoreChain()
                            : new CredentialProfileStoreChain(profileLocation);
                        
                        return chain.TryGetAWSCredentials(profileName, out var creds) 
                            ? creds 
                            : throw Errors.InvalidConfiguration;
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
        
        private static AWSCredentials GetDefaultCredentials()
        {
            return !AWS_S3Module.IsAwsSdkV4
                ? (AWSCredentials) V3GetCredentialsMethod
                      ?.Invoke(null, new object[]{false})
                  ?? throw new InvalidOperationException("Unable to get v3 credentials")
                : (AWSCredentials) V4GetCredentialsMethod
                      ?.Invoke(null, V4GetCredentialsMethod.GetParameters()
                          .Select(s => Convert.ChangeType(null, s.ParameterType)).ToArray())
                  ?? throw new InvalidOperationException("Unable to get credentials");
        }

        
        
        private static readonly MethodInfo V4GetCredentialsMethod =
            AWS_S3Module.IsAwsSdkV4
                ? Type.GetType("Amazon.Runtime.Credentials.DefaultAWSCredentialsIdentityResolver, AWSSDK.Core")
                    ?.GetMethod("GetCredentials")
                : null;
        
        private static readonly MethodInfo V3GetCredentialsMethod =
            !AWS_S3Module.IsAwsSdkV4
                ? Type.GetType("Amazon.Runtime.FallbackCredentialsFactory, AWSSDK.Core")
                    ?.GetMethod("GetCredentials", new[]{typeof(bool)})
                : null;
    }
}