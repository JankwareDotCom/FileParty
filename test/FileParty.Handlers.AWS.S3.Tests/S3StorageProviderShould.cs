using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.Interfaces;
using FileParty.Providers.AWS.S3;
using FileParty.Providers.AWS.S3.Config;
using Xunit;

namespace FileParty.Handlers.AWS.S3.Tests
{
    public class S3StorageProviderShould
    {
        private readonly IStorageProvider _storageProvider;

        private readonly AWSAccessKeyConfiguration _config = new()
        {
            Region = Environment.GetEnvironmentVariable("fileparty_s3_region"), // e.g. "us-east-1"
            Name = Environment.GetEnvironmentVariable("fileparty_s3_bucket"),
            AccessKey = Environment.GetEnvironmentVariable("fileparty_s3_access_key"),
            SecretKey =
                Environment.GetEnvironmentVariable("fileparty_s3_secret_key")
        };

        public S3StorageProviderShould()
        {
            _storageProvider = new S3StorageProvider(_config);
        }

        [Fact]
        public async Task CreateAFile_CheckIfFileExists_GetFileInfo_DeleteExistingFile()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var storagePointer =
                "dir" +
                S3StorageProvider.DirectorySeparator +
                nameof(CreateAFile_CheckIfFileExists_GetFileInfo_DeleteExistingFile);

            _storageProvider.WriteProgressEvent += (_, args) =>
            {
                Assert.NotNull(args);
                Debug.WriteLine(JsonSerializer.Serialize(args));
            };

            await _storageProvider.WriteAsync(
                storagePointer,
                inputStream,
                WriteMode.Create);

            Assert.True(await _storageProvider.ExistsAsync(storagePointer));

            var info = await _storageProvider.GetInformation(storagePointer);
            var info2 = await _storageProvider.GetInformation("dir");

            Assert.Equal(nameof(CreateAFile_CheckIfFileExists_GetFileInfo_DeleteExistingFile), info.Name);
            Assert.Equal(12 * 1024, info.Size);
            Assert.Equal(StoredItemType.File, info.StoredType);
            Assert.Equal(StoredItemType.Directory, info2.StoredType);

            await _storageProvider.DeleteAsync(storagePointer);

            Assert.False(await _storageProvider.ExistsAsync(storagePointer));
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
                S3StorageProvider.DirectorySeparator +
                "file_";

            for (var i = 0; i < 10; i++)
            {
                var storagePointer = storagePointerPrefix + i;
                var stream = new MemoryStream();
                await inputStream.CopyToAsync(stream);
                
                await _storageProvider.WriteAsync(
                    storagePointer,
                    stream,
                    WriteMode.Create);

                inputStream.Position = 0;
                
                Assert.True(await _storageProvider.ExistsAsync(storagePointer));
            }

            Assert.True(_storageProvider.TryGetStoredItemType("dir2", out var type));
            Assert.Equal(StoredItemType.Directory, type);
            
            await _storageProvider.DeleteAsync("dir2");

            Assert.False(await _storageProvider.ExistsAsync("dir2"));
        }
    }
}