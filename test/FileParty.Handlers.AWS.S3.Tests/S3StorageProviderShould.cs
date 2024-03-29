﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.Interfaces;
using FileParty.Core.Registration;
using FileParty.Providers.AWS.S3;
using FileParty.Providers.AWS.S3.Config;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FileParty.Handlers.AWS.S3.Tests;

public class S3StorageProviderShould
{
    private readonly IAsyncStorageProvider _asyncStorageProvider;

    private readonly AWSAccessKeyConfiguration _config = new()
    {
        Region = Environment.GetEnvironmentVariable("fileparty_s3_region"), // e.g. "us-east-1"
        Name = Environment.GetEnvironmentVariable("fileparty_s3_bucket"),
        AccessKey = Environment.GetEnvironmentVariable("fileparty_s3_access_key"),
        SecretKey = Environment.GetEnvironmentVariable("fileparty_s3_secret_key")
    };

    private readonly ITestOutputHelper _testOutputHelper;

    public S3StorageProviderShould(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var sc = this.AddFileParty(x => x.AddModule(_config));
        using var sp = sc.BuildServiceProvider();
        var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
        _asyncStorageProvider = asyncFactory.GetAsyncStorageProvider().Result;
    }

    [Fact]
    public async Task CreateAFile_CheckIfFileExists_GetFileInfo_DeleteExistingFile()
    {
        if (await _asyncStorageProvider.ExistsAsync("dir"))
        {
            try
            {
                await _asyncStorageProvider.DeleteAsync("dir");
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
            }
        }

        await using var inputStream = new MemoryStream();
        await using var inputWriter = new StreamWriter(inputStream);
        await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
        await inputWriter.FlushAsync();
        inputStream.Position = 0;

        var storagePointer =
            "dir" +
            _asyncStorageProvider.DirectorySeparatorCharacter +
            nameof(CreateAFile_CheckIfFileExists_GetFileInfo_DeleteExistingFile);

        _asyncStorageProvider.WriteProgressEvent += (_, args) =>
        {
            Assert.NotNull(args);
            Debug.WriteLine(JsonSerializer.Serialize(args));
        };

        await _asyncStorageProvider.WriteAsync(
            storagePointer,
            inputStream,
            WriteMode.Create);

        Assert.True(await _asyncStorageProvider.ExistsAsync(storagePointer));

        var info = await _asyncStorageProvider.GetInformationAsync(storagePointer);
        var info2 = await _asyncStorageProvider.GetInformationAsync("dir");

        Assert.Equal(nameof(CreateAFile_CheckIfFileExists_GetFileInfo_DeleteExistingFile), info.Name);
        Assert.Equal(12 * 1024, info.Size);
        Assert.Equal(StoredItemType.File, info.StoredType);
        Assert.Equal(StoredItemType.Directory, info2.StoredType);

        await using var fileStream = await _asyncStorageProvider.ReadAsync(storagePointer, CancellationToken.None);
        Assert.Equal(12 * 1024, fileStream.Length);

        await using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        _testOutputHelper.WriteLine(bytes.ToString());
        var base64 = Convert.ToBase64String(bytes);

        Assert.NotNull(base64);

        var utfEight = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

        Assert.True(utfEight.All(a => a.Equals('*')));

        await _asyncStorageProvider.DeleteAsync(storagePointer);

        Assert.False(await _asyncStorageProvider.ExistsAsync(storagePointer));
    }

    [Fact]
    public async Task DeleteEntireDirectory()
    {
        await using var inputStream = new MemoryStream();
        await using var inputWriter = new StreamWriter(inputStream);
        await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
        await inputWriter.FlushAsync();
        inputStream.Position = 0;

        var storagePointerPrefix =
            "dir2" +
            _asyncStorageProvider.DirectorySeparatorCharacter +
            "file_";

        for (var i = 0; i < 10; i++)
        {
            var storagePointer = storagePointerPrefix + i;
            var stream = new MemoryStream();
            await inputStream.CopyToAsync(stream);

            await _asyncStorageProvider.WriteAsync(
                storagePointer,
                stream,
                WriteMode.CreateOrReplace);

            inputStream.Position = 0;

            Assert.True(await _asyncStorageProvider.ExistsAsync(storagePointer));
        }

        var info = await _asyncStorageProvider.GetInformationAsync("dir2");
        Assert.Equal(StoredItemType.Directory, info.StoredType);
        Assert.Equal(StoredItemType.Directory, await _asyncStorageProvider.TryGetStoredItemTypeAsync("dir2"));

        await _asyncStorageProvider.DeleteAsync("dir2");

        Assert.False(await _asyncStorageProvider.ExistsAsync("dir2"));
    }

    [Fact]
    public async Task AllowNullConfigOnRegistration()
    {
        var sc = this.AddFileParty(cfg => { cfg.AddModule<AWS_S3Module>(null); });

        await using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<IFilePartyFactory>();

        Assert.NotNull(factory.GetStorageProvider());
        Assert.NotNull(factory.GetStorageProvider(new AWSDefaultConfiguration()));
        Assert.NotNull(factory.GetStorageProvider(new AWSAccessKeyConfiguration
        {
            AccessKey = "fake",
            Name = "obviously fake",
            Region = "us-west-2",
            SecretKey = "fake"
        }));
    }
}