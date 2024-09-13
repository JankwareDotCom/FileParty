namespace FileParty.Providers.AWS.S3.Config
{
    public class AWSInstanceProfileConfiguration : BaseAWSCredential
    {
        public string Role { get; set; }
    }
}