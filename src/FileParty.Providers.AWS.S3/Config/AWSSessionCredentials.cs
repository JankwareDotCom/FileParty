using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3.Config
{
    public class AWSSessionCredentials : BaseAWSCredential
    {
        private readonly StorageProviderConfiguration<AWS_S3Module> _baseConfig;
        private SessionAWSCredentials _sessionCredentials = null;
        public int DurationSeconds { get; set; } = 60 * 15;

        private AWSSessionCredentials(StorageProviderConfiguration<AWS_S3Module> baseConfig)
        {
            _baseConfig = baseConfig;
        }

        /// <summary>
        /// AWS Default Configuration
        /// </summary>
        public AWSSessionCredentials() 
            : this(new AWSDefaultConfiguration())
        {
            
        }

        /// <summary>
        /// AWS AccessKey Configuration
        /// </summary>
        public AWSSessionCredentials(string accessKey, string secretKey) 
            : this(new AWSAccessKeyConfiguration{ AccessKey = accessKey, SecretKey = secretKey})
        {
            
        }
        
        internal async Task<SessionAWSCredentials> GetTemporaryCredentialsAsync(IFilePartyAWSCredentialFactory credFactory)
        {
            if (_sessionCredentials != null)
            {
                return _sessionCredentials;
            }
            
            using (var stsClient = new AmazonSecurityTokenServiceClient(credFactory.GetAmazonCredentials(_baseConfig)))
            {
                var getSessionTokenRequest = new GetSessionTokenRequest
                {
                    DurationSeconds = DurationSeconds
                };

                var sessionTokenResponse = await stsClient.GetSessionTokenAsync(getSessionTokenRequest);
                var credentials = sessionTokenResponse.Credentials;

                var sessionCredentials =
                    new SessionAWSCredentials(credentials.AccessKeyId,
                        credentials.SecretAccessKey,
                        credentials.SessionToken);
                
                _sessionCredentials = sessionCredentials;
                return sessionCredentials;
            }
        }
    }
}