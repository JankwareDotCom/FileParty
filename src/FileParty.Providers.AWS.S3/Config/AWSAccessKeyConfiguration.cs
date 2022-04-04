namespace FileParty.Providers.AWS.S3.Config
{
    public class AWSAccessKeyConfiguration : BaseAWSCredential, IAccessKeyConfiguration
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}