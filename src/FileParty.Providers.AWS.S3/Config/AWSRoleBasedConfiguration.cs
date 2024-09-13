using System;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using FileParty.Core.Exceptions;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3.Config
{
    public class AWSRoleBasedConfiguration : BaseAWSCredential
    {
        private readonly StorageProviderConfiguration<AWS_S3Module> _baseConfig;
        private Credentials _credentials = null;

        private AWSRoleBasedConfiguration(StorageProviderConfiguration<AWS_S3Module> baseConfig)
        {
            _baseConfig = baseConfig;
        }

        /// <summary>
        ///     AWS Default Configuration
        /// </summary>
        public AWSRoleBasedConfiguration()
            : this(new AWSDefaultConfiguration())
        {
        }

        /// <summary>
        ///     Access Key Configuration
        /// </summary>
        public AWSRoleBasedConfiguration(string accessKey, string secretKey)
            : this(new AWSAccessKeyConfiguration {AccessKey = accessKey, SecretKey = secretKey})
        {
        }

        /// <summary>
        ///     Session Credentials
        /// </summary>
        public AWSRoleBasedConfiguration(string accessKey, string secretKey, int durationSeconds)
            : this(new AWSSessionCredentials(accessKey, secretKey) {DurationSeconds = durationSeconds})
        {
        }

        private Guid _internalIdentifier => Guid.NewGuid();
        public string RoleArn { get; set; }
        public string ExternalId { get; set; }

        public string RoleSessionName { get; set; }

        internal async Task<AWSCredentials> AssumeRoleAsync(IFilePartyAWSCredentialFactory credFactory)
        {
            if (_credentials != null)
            {
                return _credentials;
            }

            RoleSessionName = string.IsNullOrWhiteSpace(RoleSessionName)
                ? nameof(FileParty) + "_" + nameof(AWS_S3Module) + "_" + _internalIdentifier
                : RoleSessionName;

            using (var stsClient = new AmazonSecurityTokenServiceClient(credFactory.GetAmazonCredentials(_baseConfig)))
            {
                try
                {
                    var req = new AssumeRoleRequest
                    {
                        RoleArn = RoleArn,
                        RoleSessionName = RoleSessionName
                    };

                    if (_baseConfig is AWSSessionCredentials ses)
                    {
                        req.DurationSeconds = ses.DurationSeconds;
                    }

                    if (!string.IsNullOrWhiteSpace(ExternalId))
                    {
                        req.ExternalId = ExternalId;
                    }

                    var role = await stsClient.AssumeRoleAsync(req);

                    _credentials = role.Credentials;
                    return role.Credentials;
                }
                catch (AmazonSecurityTokenServiceException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw Errors.InvalidConfiguration;
                }
            }
        }
    }
}