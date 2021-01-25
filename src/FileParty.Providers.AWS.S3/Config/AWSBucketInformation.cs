using Amazon;

namespace FileParty.Providers.AWS.S3.Config
{
    public abstract class AWSBucketInformation
    {
        /// <summary>
        ///     AWS Region Endpoint System Name.  e.g. us-west-1
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        ///     Bucket Name
        /// </summary>
        public string Name { get; set; }

        internal RegionEndpoint GetRegionEndpoint()
        {
            return RegionEndpoint.GetBySystemName(Region);
        }
    }
}