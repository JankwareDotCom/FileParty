using Amazon;

namespace FileParty.Providers.AWS.S3.Config
{
    public interface IAWSBucketInformation
    {
        /// <summary>
        ///     AWS Region Endpoint System Name.  e.g. us-west-1
        /// </summary>
        string Region { get; set; }

        /// <summary>
        ///     Bucket Name
        /// </summary>
        string Name { get; set; }
    }

    internal static class IAWSBucketInformationExtension
    {
        public static RegionEndpoint GetRegionEndpoint(this IAWSBucketInformation bucketInformation)
        {
            return RegionEndpoint.GetBySystemName(bucketInformation.Region);
        }
    }
}