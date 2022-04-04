using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core;
using FileParty.Core.Enums;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FileParty.Providers.FileSystem.Tests
{
    public class FileSystemStorageProviderShould : IDisposable
    {
        private readonly string _baseDirectory =
            $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}{Guid.Empty}{Path.DirectorySeparatorChar}";

        private readonly IAsyncStorageProvider _asyncStorageProvider;
        private readonly IStorageProvider _storageProvider;

        public FileSystemStorageProviderShould()
        {
            var sc = this.AddFileParty(x => x.AddModule(new FileSystemConfiguration(_baseDirectory)));
            using var sp = sc.BuildServiceProvider();
            var factory = sp.GetRequiredService<IFilePartyFactory>();
            var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();

            _asyncStorageProvider = asyncFactory.GetAsyncStorageProvider().Result;
            _storageProvider = factory.GetStorageProvider();
            
            if (Directory.Exists(_baseDirectory)) Directory.Delete(_baseDirectory, true);
            Directory.CreateDirectory(_baseDirectory);
        }

        public void Dispose()
        {
            Directory.Delete(_baseDirectory, true);
        }

        [Fact]
        public void HaveADefaultConfig()
        {
            var cfg = new FileSystemConfiguration();
            Assert.Equal(Path.GetTempPath(), cfg.BasePath);
            Assert.Equal(Path.DirectorySeparatorChar, cfg.DirectorySeparationCharacter);
        }
        
        [Fact]
         public void EnsureConfigIsSupported()
         {
             var cfg = new FakeConfig();
             var sp = new FileSystemStorageProvider(cfg);
             var err = Assert.Throws<StorageException>(() => sp.Exists(Guid.NewGuid().ToString()));
             Assert.Equal(Errors.InvalidConfiguration.Message, err.Message);
         }
        class FakeConfig : StorageProviderConfiguration<FileSystemModule> { }
        
        [Fact]
        public void HaveConfigMethodsThatWork()
        {
            var cfg = new FileSystemConfiguration();
            cfg.UseBasePath(null);
            cfg.UseDirectorySeparationCharacter('!');
            Assert.Null(cfg.BasePath);
            Assert.Equal('!', cfg.DirectorySeparationCharacter);
        }

        [Fact]
        public async Task ThrowIfWriteStreamIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _asyncStorageProvider
                    .WriteAsync(nameof(ThrowIfWriteStreamIsNull), null, WriteMode.CreateOrReplace));
        }

        [Fact]
        public async Task DetermineIfFileExists()
        {
            var filePath = $"{Guid.NewGuid()}.txt";
            await using var fs = File.Create(_baseDirectory + filePath);
            Assert.True(await _asyncStorageProvider.ExistsAsync(filePath));
            Assert.False(await _asyncStorageProvider.ExistsAsync(filePath + "nope"));
            
            Assert.True(_storageProvider.Exists(filePath));
            Assert.False(_storageProvider.Exists(filePath + "nope"));
        }

        [Fact]
        public async Task DetermineIfDirectoryExists()
        {
            var directoryPath = $"{Guid.NewGuid()}";
            Directory.CreateDirectory(_baseDirectory + directoryPath);
            Assert.True(await _asyncStorageProvider.ExistsAsync(directoryPath));
            Assert.False(await _asyncStorageProvider.ExistsAsync(directoryPath + "nope"));
            
            Assert.True(_storageProvider.Exists(directoryPath));
            Assert.False(_storageProvider.Exists(directoryPath + "nope"));
        }

        [Fact]
        public async Task DetermineStoredItemType()
        {
            var filePath = $"{Guid.NewGuid()}.txt";
            var directoryPath = $"{Guid.NewGuid()}";
            await using var fs = File.Create(_baseDirectory + filePath);
            Directory.CreateDirectory(_baseDirectory + directoryPath);

            Assert.NotNull(await _asyncStorageProvider.TryGetStoredItemTypeAsync(filePath));
            Assert.NotNull(await _asyncStorageProvider.TryGetStoredItemTypeAsync(directoryPath));
            Assert.Null(await _asyncStorageProvider.TryGetStoredItemTypeAsync(Guid.NewGuid().ToString()));
            Assert.Equal(StoredItemType.File, await _asyncStorageProvider.TryGetStoredItemTypeAsync(filePath));
            Assert.Equal(StoredItemType.Directory, await _asyncStorageProvider.TryGetStoredItemTypeAsync(directoryPath));
        }

        [Fact]
        public async Task CreateANewFileAndReadIt()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var filePath = $"{Guid.NewGuid()}";

            if (File.Exists(filePath)) File.Delete(filePath);

            _storageProvider.WriteProgressEvent += (_, args) =>
            {
                Assert.NotNull(args);
                Debug.WriteLine(JsonSerializer.Serialize(args));
            };

            _storageProvider.Write(
                filePath,
                inputStream,
                WriteMode.Create);

            Assert.True(await _asyncStorageProvider.ExistsAsync(filePath));

            await using var stream = _storageProvider.Read(filePath);
            using var reader = new StreamReader(stream);
            Assert.Matches(@"\*+", await reader.ReadToEndAsync());
        }

        [Fact]
        public async Task ThrowIfTryingToReadDirectoryInsteadOfFile()
        {
            var err = await Assert.ThrowsAsync<StorageException>(
                async () => await _asyncStorageProvider
                    .ReadAsync(Path.DirectorySeparatorChar.ToString()));
            
            Assert.Equal(Errors.MustBeFile.Message, err.Message);
        }
        
        [Fact]
        public async Task ThrowIfTryingToReadNotFoundFile()
        {
            var err = await Assert.ThrowsAsync<StorageException>(
                async () => await _asyncStorageProvider
                    .ReadAsync(Guid.NewGuid().ToString()));
            
            Assert.Equal(Errors.FileNotFoundException.Message, err.Message);
        }

        [Fact]
        public async Task FailAtReplacingAFileThatDoesNotExist()
        {
            await using var inputStream = new MemoryStream();
            await using var inputWriter = new StreamWriter(inputStream);
            await inputWriter.WriteAsync(new string('*', 12 * 1024)); // 12kb string
            await inputWriter.FlushAsync();
            inputStream.Position = 0;

            var filePath = $"{Guid.NewGuid()}";

            if (File.Exists(filePath)) File.Delete(filePath);

            _asyncStorageProvider.WriteProgressEvent += (_, args) =>
            {
                Assert.NotNull(args);
                Debug.WriteLine(JsonSerializer.Serialize(args));
            };

            var error = await Assert.ThrowsAsync<StorageException>(async () =>
            {
                await _asyncStorageProvider.WriteAsync(
                    filePath,
                    inputStream,
                    WriteMode.Replace);
            });

            Assert.False(await _asyncStorageProvider.ExistsAsync(filePath));
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

            var filePath = $"{Guid.NewGuid()}";

            if (File.Exists(filePath)) File.Delete(filePath);

            await _asyncStorageProvider.WriteAsync(
                filePath,
                inputStream,
                WriteMode.Create);

            await using var replacementStream = new MemoryStream();
            await using var replacementWriter = new StreamWriter(replacementStream);
            await replacementWriter.WriteAsync(new string('x', 12 * 1024)); // 12kb string
            await replacementWriter.FlushAsync();
            replacementStream.Position = 0;

            await _asyncStorageProvider.WriteAsync(
                filePath,
                replacementStream,
                WriteMode.Replace);

            var fileContents = await File.ReadAllTextAsync(_baseDirectory + filePath);

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

            var filePath = $"{Guid.NewGuid()}";

            if (File.Exists(_baseDirectory + filePath)) File.Delete(_baseDirectory + filePath);

            await _asyncStorageProvider.WriteAsync(
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
                await _asyncStorageProvider.WriteAsync(
                    filePath,
                    replacementStream,
                    WriteMode.Create);
            });

            var fileContents = await File.ReadAllTextAsync(_baseDirectory + filePath);

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

            var filePath = $"{Guid.NewGuid()}";
            await _asyncStorageProvider.WriteAsync(filePath, inputStream, WriteMode.CreateOrReplace);

            Assert.True(await _asyncStorageProvider.ExistsAsync(filePath));

            _storageProvider.Delete(new [] {filePath, "doesnotexist"});

            Assert.False(await _asyncStorageProvider.ExistsAsync(filePath));
        }

        [Fact]
        public async Task DeleteADirectory()
        {
            var dirPath = $"{Guid.NewGuid()}";
            Directory.CreateDirectory(_baseDirectory + dirPath);

            Assert.True(await _asyncStorageProvider.ExistsAsync(dirPath));

            _storageProvider.Delete(new [] {dirPath, "doesnotexist"});

            Assert.False(await _asyncStorageProvider.ExistsAsync(dirPath));
        }

        [Fact]
        public async Task RaiseErrorWhenTryingToDeleteAThingThatDoesNotExist()
        {
            var path = $"{Guid.NewGuid()}";

            Assert.False(await _asyncStorageProvider.ExistsAsync(path));

            await Assert.ThrowsAsync<StorageException>(async () => { await _asyncStorageProvider.DeleteAsync(path); });
        }

        [Fact]
        public async Task ReturnNullInformationIfFileNotFound()
        {
            var info = await _asyncStorageProvider.GetInformationAsync(Guid.NewGuid().ToString());
            Assert.Null(info);
        }

        [Fact]
        public void ThrowIfNullOrEmptyStoragePointerPassed()
        {
            var exc = Assert.Throws<StorageException>(() => _storageProvider.TryGetStoredItemType(null, out _));
            Assert.Equal(Errors.StoragePointerMustHaveValue.Message, exc.Message);

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
            var filePath = $"{name}";

            if (File.Exists(filePath)) File.Delete(filePath);

            await _asyncStorageProvider.WriteAsync(
                filePath,
                inputStream,
                WriteMode.Create);

            var info1 = await _asyncStorageProvider.GetInformationAsync(filePath);

            Thread.Sleep(1000); // for obvious timestamp differences

            await using var replacementStream = new MemoryStream();
            await using var replacementWriter = new StreamWriter(replacementStream);
            await replacementWriter.WriteAsync(new string('x', 12 * 1024)); // 12kb string
            await replacementWriter.FlushAsync();
            replacementStream.Position = 0;

            await _asyncStorageProvider.WriteAsync(
                filePath,
                replacementStream,
                WriteMode.Replace);

            var info2 = await _asyncStorageProvider.GetInformationAsync(filePath);

            Assert.Equal(name, info1.Name);
            Assert.Equal(info1.Name, info2.Name);
            Assert.Equal(info1.DirectoryPath, _baseDirectory.TrimEnd(Path.DirectorySeparatorChar));
            Assert.Equal(info1.DirectoryPath, info2.DirectoryPath);
            Assert.NotNull(info1.CreatedTimestamp);
            Assert.NotNull(info2.CreatedTimestamp);
            Assert.True(info1.LastModifiedTimestamp < info2.LastModifiedTimestamp);
            Assert.Equal(StoredItemType.File, info1.StoredType);
            Assert.Equal(info1.StoredType, info2.StoredType);
            Assert.Equal(info1.StoragePointer, info2.StoragePointer);
            Assert.Equal(info1.Size, info2.Size);
        }
    }
}