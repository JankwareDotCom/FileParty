namespace FileParty.Providers.AWS.S3.Config
{
    public interface IAccessKeyConfiguration
    {
        string AccessKey { get; set; }
        string SecretKey { get; set; }
    }
}