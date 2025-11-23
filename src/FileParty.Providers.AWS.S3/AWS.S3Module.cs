using System;
using FileParty.Core;
using FileParty.Core.Registration;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3
{
    public class AWS_S3Module : BaseFilePartyModule<S3StorageProvider, S3StorageProvider>
    {
        public static readonly bool IsAwsSdkV4 = Type.GetType("Amazon.Runtime.Credentials.DefaultAWSCredentialsIdentityResolver, AWSSDK.Core") != null;
        
        public AWS_S3Module()
        {
            this.RegisterModuleDependency<AWS_S3Module,
                IFilePartyS3ClientFactory, FilePartyS3ClientFactory>();
            this.RegisterModuleDependency<AWS_S3Module,
                IFilePartyAWSCredentialFactory, FilePartyAWSCredentialFactory>();
            this.RegisterModuleDependency<AWS_S3Module,
                IFilePartyS3BucketInformationProvider, FilePartyS3BucketInformationProvider>();
        }
    }
}