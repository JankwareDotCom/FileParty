using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using Xunit;

namespace FileParty.Providers.FileSystem.Tests
{
    public class FileSystemStorageProviderShould : IDisposable
    {
        private readonly string _baseDirectory =
            $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}{Guid.Empty}{Path.DirectorySeparatorChar}";

        private readonly IStorageProvider _storageProvider;

        public FileSystemStorageProviderShould()
        {
            _storageProvider = new FileSystemStorageProvider();
            if (Directory.Exists(_baseDirectory)) Directory.Delete(_baseDirectory, true);
            Directory.CreateDirectory(_baseDirectory);
        }

        public void Dispose()
        {
            _storageProvider?.Dispose();
            Directory.Delete(_baseDirectory, true);
        }

        [Fact]
        public async Task DetermineIfFileExists()
        {
            var filePath = $"{_baseDirectory}{Guid.NewGuid()}.txt";
            await using var fs = File.Create(filePath);
            Assert.True(await _storageProvider.ExistsAsync(filePath));
            Assert.False(await _storageProvider.ExistsAsync(filePath + "nope"));
        }

        [Fact]
        public async Task DetermineIfDirectoryExists()
        {
            var directoryPath = $"{_baseDirectory}{Guid.NewGuid()}";
            Assert.True(await _storageProvider.ExistsAsync(directoryPath));
            Assert.False(await _storageProvider.ExistsAsync(directoryPath + "nope"));
        }

        [Fact]
        public async Task DetermineStoredItemType()
        {
            var filePath = $"{_baseDirectory}{Guid.NewGuid()}.txt";
            var directoryPath = $"{_baseDirectory}{Guid.NewGuid()}";
            await using var fs = File.Create(filePath);

            Assert.True(_storageProvider.TryGetStoredItemType(filePath, out var fileType));
            Assert.True(_storageProvider.TryGetStoredItemType(directoryPath, out var dirType));
            Assert.False(_storageProvider.TryGetStoredItemType(Guid.NewGuid().ToString(), out var trashType));
            Assert.Equal(StoredItemType.File, fileType);
            Assert.Equal(StoredItemType.Directory, dirType);
            Assert.Null(trashType);
        }

        [Fact]
        public async Task CreateANewFile()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var filePath = $"{_baseDirectory}{Guid.NewGuid()}";

            if (File.Exists(filePath)) File.Delete(filePath);

            _storageProvider.WriteProgressEvent += (_, args) =>
            {
                Assert.NotNull(args);
                Debug.WriteLine(JsonSerializer.Serialize(args));
            };

            await _storageProvider.WriteAsync(
                filePath,
                inputStream,
                WriteMode.Create);

            Assert.True(await _storageProvider.ExistsAsync(filePath));
        }

        [Fact]
        public async Task FailAtReplacingAFileThatDoesNotExist()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var filePath = $"{_baseDirectory}{Guid.NewGuid()}";

            if (File.Exists(filePath)) File.Delete(filePath);

            _storageProvider.WriteProgressEvent += (_, args) =>
            {
                Assert.NotNull(args);
                Debug.WriteLine(JsonSerializer.Serialize(args));
            };

            var error = await Assert.ThrowsAsync<StorageException>(async () =>
            {
                await _storageProvider.WriteAsync(
                    filePath,
                    inputStream,
                    WriteMode.Replace);
            });

            Assert.False(await _storageProvider.ExistsAsync(filePath));
            Assert.Equal(Errors.FileNotFoundException.Message, error.Message);
        }

        [Fact]
        public async Task ReplaceAFile()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var filePath = $"{_baseDirectory}{Guid.NewGuid()}";

            if (File.Exists(filePath)) File.Delete(filePath);

            await _storageProvider.WriteAsync(
                filePath,
                inputStream,
                WriteMode.Create);

            await using var replacementStream = new MemoryStream();
            await using var replacementWriter = new StreamWriter(replacementStream);
            await replacementWriter.WriteAsync(new string('x', 12 * 1024)); // 12kb string
            await replacementWriter.FlushAsync();
            replacementStream.Position = 0;

            await _storageProvider.WriteAsync(
                filePath,
                replacementStream,
                WriteMode.Replace);

            var fileContents = await File.ReadAllTextAsync(filePath);

            Assert.True(fileContents.All(x => x == 'x'));
        }

        [Fact]
        public async Task FailAtFileReplacementWhenIncorrectWriteMode()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var filePath = $"{_baseDirectory}{Guid.NewGuid()}";

            if (File.Exists(filePath)) File.Delete(filePath);

            await _storageProvider.WriteAsync(
                filePath,
                inputStream,
                WriteMode.Create);

            await using var replacementStream = new MemoryStream();
            await using var replacementWriter = new StreamWriter(replacementStream);
            await replacementWriter.WriteAsync(new string('x', 12 * 1024)); // 12kb string
            await replacementWriter.FlushAsync();
            replacementStream.Position = 0;

            var error = await Assert.ThrowsAsync<StorageException>(async () =>
            {
                await _storageProvider.WriteAsync(
                    filePath,
                    replacementStream,
                    WriteMode.Create);
            });

            var fileContents = await File.ReadAllTextAsync(filePath);

            Assert.False(fileContents.All(x => x == 'x'));
            Assert.Equal(Errors.FileAlreadyExistsException.Message, error.Message);
        }

        [Fact]
        public async Task DeleteAFile()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var filePath = $"{_baseDirectory}{Guid.NewGuid()}";
            await _storageProvider.WriteAsync(filePath, inputStream, WriteMode.CreateOrReplace);

            Assert.True(await _storageProvider.ExistsAsync(filePath));

            await _storageProvider.DeleteAsync(filePath);

            Assert.False(await _storageProvider.ExistsAsync(filePath));
        }

        [Fact]
        public async Task DeleteADirectory()
        {
            var dirPath = $"{_baseDirectory}{Guid.NewGuid()}";
            Directory.CreateDirectory(dirPath);

            Assert.True(await _storageProvider.ExistsAsync(dirPath));

            await _storageProvider.DeleteAsync(dirPath);

            Assert.False(await _storageProvider.ExistsAsync(dirPath));
        }

        [Fact]
        public async Task RaiseErrorWhenTryingToDeleteAThingThatDoesNotExist()
        {
            var path = $"{_baseDirectory}{Guid.NewGuid()}";

            Assert.False(await _storageProvider.ExistsAsync(path));

            await Assert.ThrowsAsync<StorageException>(async () => { await _storageProvider.DeleteAsync(path); });
        }

        [Fact]
        public async Task ReturnInformationAboutAFile()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var name = Guid.NewGuid().ToString();
            var filePath = $"{_baseDirectory}{name}";

            if (File.Exists(filePath)) File.Delete(filePath);

            await _storageProvider.WriteAsync(
                filePath,
                inputStream,
                WriteMode.Create);

            var info1 = await _storageProvider.GetInformation(filePath);

            Thread.Sleep(1000); // for obvious timestamp differences

            await using var replacementStream = new MemoryStream();
            await using var replacementWriter = new StreamWriter(replacementStream);
            await replacementWriter.WriteAsync(new string('x', 12 * 1024)); // 12kb string
            await replacementWriter.FlushAsync();
            replacementStream.Position = 0;

            await _storageProvider.WriteAsync(
                filePath,
                replacementStream,
                WriteMode.Replace);

            var info2 = await _storageProvider.GetInformation(filePath);

            Assert.Equal(name, info1.Name);
            Assert.Equal(info1.Name, info2.Name);
            Assert.Equal(info1.DirectoryPath, _baseDirectory.TrimEnd(Path.DirectorySeparatorChar));
            Assert.Equal(info1.DirectoryPath, info2.DirectoryPath);
            Assert.NotNull(info1.CreatedTimestamp);
            Assert.NotNull(info2.CreatedTimestamp);
            Assert.Equal(info1.CreatedTimestamp, info2.CreatedTimestamp);
            Assert.True(info1.LastModifiedTimestamp < info2.LastModifiedTimestamp);
            Assert.Equal(StoredItemType.File, info1.StoredType);
            Assert.Equal(info1.StoredType, info2.StoredType);
        }
    }
}