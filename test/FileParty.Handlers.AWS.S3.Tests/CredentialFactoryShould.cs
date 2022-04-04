using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Core.Registration;
using FileParty.Providers.AWS.S3;
using FileParty.Providers.AWS.S3.Config;
using FileParty.Providers.AWS.S3.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FileParty.Handlers.AWS.S3.Tests;

public class CredentialFactoryShould
{
    private static IFilePartyAWSCredentialFactory _credFactory = new FilePartyAWSCredentialFactory();
    private static string _bucketName = Environment.GetEnvironmentVariable("fileparty_s3_bucket");
    private static string _regionName = Environment.GetEnvironmentVariable("fileparty_s3_region");
    private static string _accessKey = Environment.GetEnvironmentVariable("fileparty_s3_access_key");
    private static string _secretKey = Environment.GetEnvironmentVariable("fileparty_s3_secret_key");
    private static string _roleArn = Environment.GetEnvironmentVariable("fileparty_s3_role_arn");
    private static string _roleExternalId = Environment.GetEnvironmentVariable("fileparty_s3_role_external_id");

    private async Task EnsureFileCreationAndDeletion(StorageProviderConfiguration<AWS_S3Module> cfg)
    {
        await using var sp = this.AddFileParty(c => c.AddModule(cfg)).BuildServiceProvider();
        var storageProviderFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
        var storageProvider = await storageProviderFactory.GetAsyncStorageProvider();
        
        await using var inputStream = new MemoryStream();
        await using var inputWriter = new StreamWriter(inputStream);
        await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
        await inputWriter.FlushAsync();
        inputStream.Position = 0;

        var key = Guid.NewGuid().ToString();
        
        try
        {
            await storageProvider.WriteAsync(key, inputStream, WriteMode.Create, CancellationToken.None);
            Assert.True(await storageProvider.ExistsAsync(key, CancellationToken.None));
        }
        finally
        {
            await storageProvider.DeleteAsync(key, CancellationToken.None);
            Assert.False(await storageProvider.ExistsAsync(key, CancellationToken.None));    
        }
    }
    
    [Theory]
    [InlineData(null, null)]        // default profile, default place
    [InlineData("CredentialFactoryShould", null)] // profile at default place
    [InlineData("CredentialFactoryShould", "TempDirectory")] // profile at place
    [InlineData(null, "TempDirectory")] // default profile at place
    public async Task CreateCredentials_FromStoredProfile(string profileName, string profileLocation)
    {
        await using (var acc = new AWSConfigConfigurator(_accessKey, _secretKey, profileLocation))
        {
            profileLocation = acc.ProfileDirectory;
            var cfg = new AWSStoredProfileConfiguration(profileName, profileLocation);
            Assert.Equal(profileName, cfg.ProfileName);
            Assert.Equal(profileLocation, cfg.ProfileLocation);

            var awsCreds = _credFactory.GetAmazonCredentials(cfg);
            var creds = await awsCreds.GetCredentialsAsync();

            await EnsureFileCreationAndDeletion(new AWSAccessKeyConfiguration
            {
                AccessKey = creds.AccessKey,
                SecretKey = creds.SecretKey,
                Name = _bucketName,
                Region = _regionName
            });    
        }
    }

    [Fact]
    public async Task CreateCredentials_FromAccessKeys()
    {
        var cfg = new AWSAccessKeyConfiguration
        {
            AccessKey = _accessKey,
            Region = _regionName,
            Name = _bucketName,
            SecretKey = _secretKey
        };

        var awsCreds = _credFactory.GetAmazonCredentials(cfg);
        var creds = await awsCreds.GetCredentialsAsync();
        await EnsureFileCreationAndDeletion(new AWSAccessKeyConfiguration
        {
            AccessKey = creds.AccessKey,
            SecretKey = creds.SecretKey,
            Name = _bucketName,
            Region = _regionName
        });
    }
    
    [Fact]
    public async Task CreateCredentials_UsingSession()
    {
        var cfg = new AWSSessionCredentials(_accessKey, _secretKey)
        {
            Region = _regionName,
            Name = _bucketName,
            DurationSeconds = 15 * 60
        };
        
        await _credFactory.GetAmazonCredentials(cfg).GetCredentialsAsync();

        await EnsureFileCreationAndDeletion(cfg);
    }

    [Fact(Skip = "Long Running Test")]
    public async Task CreateCredentials_ButThrowDueToExpired_UsingSession()
    {
        var cfg = new AWSSessionCredentials(_accessKey, _secretKey)
        {
            Region = _regionName,
            Name = _bucketName,
            DurationSeconds = 15 * 60
        };

        await _credFactory.GetAmazonCredentials(cfg).GetCredentialsAsync();
        
        await Task.Delay(TimeSpan.FromSeconds(cfg.DurationSeconds), CancellationToken.None);
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await EnsureFileCreationAndDeletion(cfg);
        });
    }

    [Fact]
    public async Task CreateCredentials_UsingRole()
    {
        await using (_ = new AWSConfigConfigurator(_accessKey, _secretKey))
        {
            var cfg = new AWSRoleBasedConfiguration()
            {
                Name = _bucketName, 
                Region = _regionName, 
                RoleArn = _roleArn,
                ExternalId = _roleExternalId
            };
        
            await _credFactory.GetAmazonCredentials(cfg).GetCredentialsAsync();
            await EnsureFileCreationAndDeletion(cfg);    
        }
        
    }
}

public class AWSConfigConfigurator : IDisposable, IAsyncDisposable
{
    private static readonly AutoResetEvent AwsConfiguratorResetEvent = new AutoResetEvent(true);
    private static readonly string[] ProfileNames = { "default", nameof(CredentialFactoryShould) };
    private readonly bool _isTempDir;
    private readonly string _profileLocation;
    private readonly string[] _originalFiles;
    private readonly Guid _instanceId = Guid.NewGuid();
    
    public string ProfileDirectory { get; }
    
    /// <summary>
    /// Creates AWS Profile in given location, and if one already exists,
    /// preserves it during the test and restores after testing
    /// </summary>
    /// <param name="profileLocation">when "TempDirectory" it will use the Env Temp Dir,
    /// otherwise it will use the default location for AWS Configs</param>
    public AWSConfigConfigurator(string accessKey, string secretKey, string profileLocation = null)
    {
        _profileLocation = profileLocation;
        Debug.WriteLine($"{profileLocation ?? "DefaultProfileLocation"} {_instanceId} is awaiting the lock");
        AwsConfiguratorResetEvent.WaitOne();
        Debug.WriteLine($"{profileLocation ?? "DefaultProfileLocation"} {_instanceId} now owns lock");
        _isTempDir = profileLocation == "TempDirectory";
        ProfileDirectory = _isTempDir
            ? Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + 
              Path.DirectorySeparatorChar + _instanceId + Path.DirectorySeparatorChar
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).TrimEnd(Path.DirectorySeparatorChar) +
              Path.DirectorySeparatorChar + ".aws" + Path.DirectorySeparatorChar;

        // ensure dir exists
        Directory.CreateDirectory(ProfileDirectory);
        Directory.CreateDirectory(ProfileDirectory + _instanceId);

        // gets og files, and moves them
        _originalFiles = Directory.GetFiles(ProfileDirectory);
        foreach (var originalFile in _originalFiles)
        {
            File.Move(
                originalFile, 
                ProfileDirectory + _instanceId + Path.DirectorySeparatorChar + Path.GetFileName(originalFile)
            );
        }
        
        // creates new configs from env vars
        var configs = new List<string>();
        foreach (var pn in ProfileNames)
        {
            configs.Add($"[{pn}]");
            configs.Add($"aws_access_key_id={accessKey}");
            configs.Add($"aws_secret_access_key={secretKey}");
        }
        
        File.WriteAllLines(ProfileDirectory + "credentials", configs);
    }

    public void Dispose()
    {
        // deletes the files we made
        foreach (var testFile in Directory.GetFiles(ProfileDirectory))
        {
            File.Delete(testFile);
        }
        
        // moves original files back
        foreach (var originalFile in _originalFiles)
        {
            File.Move(originalFile, ProfileDirectory + Path.GetFileName(originalFile));
        }

        // cleans up mess
        Directory.Delete(ProfileDirectory + _instanceId, true);
        if (_isTempDir)
        {
            Directory.Delete(ProfileDirectory, true);
        }
        
        Debug.WriteLine($"{_profileLocation ?? "DefaultProfileLocation"} {_instanceId} is releasing the lock");
        AwsConfiguratorResetEvent.Set();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return new ValueTask();
    }
}