namespace FileParty.Providers.AWS.S3.Config
{
    public class AWSAccessKeyConfiguration : AWSBucketInformation
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}