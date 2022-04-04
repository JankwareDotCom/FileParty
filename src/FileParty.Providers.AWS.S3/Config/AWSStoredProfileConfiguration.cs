using Amazon.Runtime;
using FileParty.Core.Models;

namespace FileParty.Providers.AWS.S3.Config
{
    public class AWSStoredProfileConfiguration : BaseAWSCredential
    {

        public string ProfileName { get; set; } = "default";
        public string ProfileLocation { get; set; }

        public AWSStoredProfileConfiguration()
        {
            
        }

        internal StoredProfileAWSCredentials GetConfig()
        {
            return string.IsNullOrWhiteSpace(ProfileLocation)
                ? string.IsNullOrWhiteSpace(ProfileName)
                    ? new StoredProfileAWSCredentials()
                    : new StoredProfileAWSCredentials(ProfileName)
                : string.IsNullOrWhiteSpace(ProfileName)
                    ? new StoredProfileAWSCredentials("default", ProfileLocation)
                    : new StoredProfileAWSCredentials(ProfileName, ProfileLocation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName">The name of the profile in which the credentials were stored.</param>
        public AWSStoredProfileConfiguration(string profileName) : this()
        {
            ProfileName = profileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName">The name of the profile in which the credentials were stored.</param>
        /// <param name="profileLocation">
        /// Optional; checks for the profile in the shared credentials file at the
        /// specified location. If not set, the AWS SDK will inspect its own credential store file first before
        /// attempting to locate a shared credential file using either the default location beneath the user's
        /// home profile folder or the location specified in the AWS_SHARED_CREDENTIALS_FILE environment
        /// variable.</param>
        public AWSStoredProfileConfiguration(string profileName, string profileLocation) : this(profileName)
        {
            ProfileLocation = profileLocation;
        }
    }
}