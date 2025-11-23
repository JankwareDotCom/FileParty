using System.Reflection;
using Xunit;

namespace FileParty.Handlers.AWS.S3.Tests;

public class AwsSdkVersionTest
{
    [Theory]
#if AWS_SDK_V3
    [InlineData(3)]
#endif
#if AWS_SDK_V4
    [InlineData(4)]
#endif
    public void ShouldUseExpectedAwsSdkVersion(int expectedVersion)
    {
        var awsS3Assembly = Assembly.Load("AWSSDK.S3");
        
        Assert.NotNull(awsS3Assembly);
        
        var version = awsS3Assembly.GetName().Version;

        // Assert expected version
        Assert.True(
            version?.Major == expectedVersion,
            $"Expected AWS SDK v{expectedVersion}, but got v{version?.Major ?? 0}");
    }
}