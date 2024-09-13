using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileParty.Core.Exceptions;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FileParty.Core.Tests;

public class FilePartyRegistrationShould
{
    [Fact]
    public async Task RegisterAStorageProviderWithTheMainServiceCollection()
    {
        var sc = this.AddFileParty(cfg =>
        {
            cfg.AddModule<TestModule2>(new TestConfiguration2());
            cfg.AddModule<TestModule>(new TestConfiguration());
        });

        await using var sp = sc.BuildServiceProvider();
        var storageProvider = sp.GetRequiredService<IStorageProvider>();

        var cfg = new TestConfiguration() as StorageProviderConfiguration<TestModule>;
        Assert.Equal(Path.DirectorySeparatorChar, cfg.DirectorySeparationCharacter);

        Assert.True(storageProvider is TestStorageProvider);
    }

    [Fact]
    public async Task ChangeDefaultStorageProvider()
    {
        var sc = this.AddFileParty(cfg =>
        {
            cfg.AddModule<TestModule2>(new TestConfiguration2());
            cfg.AddModule<TestModule>(new TestConfiguration());
        });

        await using var sp = sc.BuildServiceProvider();
        var storageProvider = sp.GetRequiredService<IStorageProvider>();
        var asyncStorageProvider = sp.GetRequiredService<IAsyncStorageProvider>();

        Assert.True(storageProvider is TestStorageProvider);
        Assert.True(asyncStorageProvider is TestAsyncStorageProvider);

        var factoryReConfig = sp.GetRequiredService<IFilePartyFactoryModifier>();
        factoryReConfig.ChangeDefaultModuleConfig(new TestConfiguration2());
        storageProvider = sp.GetRequiredService<IStorageProvider>();
        asyncStorageProvider = sp.GetRequiredService<IAsyncStorageProvider>();

        var newDirChar = new TestConfiguration2().DirectorySeparationCharacter;
        Assert.Equal(newDirChar, storageProvider.DirectorySeparatorCharacter);
        Assert.Equal(newDirChar, asyncStorageProvider.DirectorySeparatorCharacter);

        Assert.True(storageProvider is TestStorageProvider2);
        Assert.True(asyncStorageProvider is TestAsyncStorageProvider2);
    }

    [Fact]
    private async Task ThrowIfChangingAnUnregisteredStorageProvider()
    {
        var sc = this.AddFileParty(cfg => { cfg.AddModule<TestModule2>(new TestConfiguration2()); });
        await using var sp = sc.BuildServiceProvider();
        var factoryReConfig = sp.GetRequiredService<IFilePartyFactoryModifier>();

        var err = Assert.Throws<StorageException>(
            () => factoryReConfig.ChangeDefaultModuleConfig(new TestConfiguration()));

        Assert.Equal(Errors.SPNotFound.Message, err.Message);
    }

    [Fact]
    public Task RegisterAFilePartyConfigAndConfigForEachModuleRegistered()
    {
        var sc = this.AddFileParty(cfg =>
        {
            cfg.AddModule<TestModule2>(new TestConfiguration2());
            cfg.AddModule<TestModule>(new TestConfiguration());
        });

        Assert.NotNull(sc);
        var descriptors = sc.ToList();
        Assert.NotNull(descriptors.FirstOrDefault(f =>
            f.ServiceType == typeof(IConfiguredFileParty) &&
            f.Lifetime == ServiceLifetime.Singleton));

        Assert.Equal(2, descriptors.Count(f =>
            f.ServiceType == typeof(IFilePartyModuleConfiguration) &&
            f.Lifetime == ServiceLifetime.Singleton &&
            f.ImplementationInstance != null &&
            f.ImplementationInstance.GetType().IsGenericType));

        Assert.Single(descriptors.Where(w => w.ServiceType == typeof(IFilePartyFactory)));
        Assert.Single(descriptors.Where(w => w.ServiceType == typeof(IAsyncFilePartyFactory)));

        return Task.CompletedTask;
    }

    [Fact]
    public async Task HaveTheFactoriesReturnLastAsDefaultWhenNoDefaultIsSet()
    {
        var sc = this.AddFileParty(cfg =>
        {
            cfg.AddModule<TestModule2>(new TestConfiguration2());
            cfg.AddModule<TestModule>(new TestConfiguration());
        });

        await using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<IFilePartyFactory>();
        var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
        var storage = factory.GetStorageProvider();
        var asyncStorage = await asyncFactory.GetAsyncStorageProvider();

        Assert.Equal(typeof(TestStorageProvider), storage.GetType());
        Assert.Equal(typeof(TestAsyncStorageProvider), asyncStorage.GetType());
    }

    [Fact]
    public async Task HaveTheFactoriesReturnTheDefaultWhenDefaultIsSet()
    {
        var sc = this.AddFileParty(cfg =>
        {
            cfg.AddModule<TestModule>(new TestConfiguration());
            cfg.AddModule<TestModule2>(new TestConfiguration2());
            cfg.SetDefaultModule<TestModule>();
        });

        await using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<IFilePartyFactory>();
        var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
        var storage = factory.GetStorageProvider();
        var asyncStorage = await asyncFactory.GetAsyncStorageProvider();

        Assert.Equal(typeof(TestStorageProvider), storage.GetType());
        Assert.Equal(typeof(TestAsyncStorageProvider), asyncStorage.GetType());
    }

    [Fact]
    public async Task AllowTheFactoryToReturnADifferentTypeStorageProviderWhenRequested()
    {
        var sc = this.AddFileParty(cfg =>
        {
            cfg.AddModule<TestModule2>(new TestConfiguration2());
            cfg.AddModule<TestModule>(new TestConfiguration());
        });

        await using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<IFilePartyFactory>();
        var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
        var storage = factory.GetStorageProvider<TestModule2>();
        var asyncStorage = await asyncFactory.GetAsyncStorageProvider<TestModule2>();

        Assert.Equal(typeof(TestStorageProvider2), storage.GetType());
        Assert.Equal(typeof(TestAsyncStorageProvider2), asyncStorage.GetType());
    }

    [Fact]
    public async Task AllowTheFactoryToReturnADifferentTypeStorageProviderWhenRequestedWithConfiguration()
    {
        var defaultConfig = new TestConfiguration2();
        var sc = this.AddFileParty(cfg =>
        {
            cfg.AddModule<TestModule2>(defaultConfig);
            cfg.AddModule<TestModule>(new TestConfiguration());
        });

        var newConfig = new TestConfiguration3();
        await using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<IFilePartyFactory>();
        var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
        var storage = factory.GetStorageProvider<TestModule2>(newConfig);
        var asyncStorage = await asyncFactory.GetAsyncStorageProvider<TestModule2>(newConfig);

        Assert.Equal(typeof(TestStorageProvider2), storage.GetType());
        Assert.Equal(typeof(TestAsyncStorageProvider2), asyncStorage.GetType());

        Assert.NotEqual(defaultConfig.DirectorySeparationCharacter, newConfig.DirectorySeparationCharacter);
        Assert.Equal(newConfig.DirectorySeparationCharacter, storage.DirectorySeparatorCharacter);
    }

    [Fact]
    public async Task AllowNullConfigOnRegistration()
    {
        var sc = this.AddFileParty(cfg =>
        {
            cfg.AddModule<TestModule>(null);
            cfg.AddModule<TestModule2>(null);
        });

        await using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<IFilePartyFactory>();

        var tmSP = factory.GetStorageProvider<TestModule>(new TestConfiguration());

        Assert.NotNull(tmSP);
    }

    [Fact]
    public async Task ThrowIfModuleNotRegistered()
    {
        var sc = this.AddFileParty(cfg => { cfg.AddModule<TestModule2>(null); });

        await using var sp = sc.BuildServiceProvider();
        var factory = sp.GetRequiredService<IFilePartyFactory>();

        var err = Assert.Throws<StorageException>(() => factory.GetStorageProvider<TestModule>());
        Assert.Equal(Errors.SPNotFound.Message, err.Message);
        Assert.Equal(Errors.SPNotFound.ErrorCode, err.ErrorCode);

        err = Assert.Throws<StorageException>(() => factory.GetStorageProvider<TestModule>(new TestConfiguration()));
        Assert.Equal(Errors.SPNotFound.Message, err.Message);
        Assert.Equal(Errors.SPNotFound.ErrorCode, err.ErrorCode);

        var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();

        err = await Assert.ThrowsAsync<StorageException>(
            async () => await asyncFactory.GetAsyncStorageProvider<TestModule>());
        Assert.Equal(Errors.SPNotFound.Message, err.Message);
        Assert.Equal(Errors.SPNotFound.ErrorCode, err.ErrorCode);

        err = await Assert.ThrowsAsync<StorageException>(
            async () => await asyncFactory.GetAsyncStorageProvider<TestModule>(new TestConfiguration()));
        Assert.Equal(Errors.SPNotFound.Message, err.Message);
        Assert.Equal(Errors.SPNotFound.ErrorCode, err.ErrorCode);
    }

    [Theory]
    [InlineData(typeof(IFilePartyFactory), typeof(IStorageReader), null)]
    [InlineData(typeof(IFilePartyFactory), typeof(IStorageWriter), null)]
    [InlineData(typeof(IAsyncFilePartyFactory), typeof(IAsyncStorageReader), null)]
    [InlineData(typeof(IAsyncFilePartyFactory), typeof(IAsyncStorageWriter), null)]
    [InlineData(typeof(IFilePartyFactory), typeof(IStorageReader), false)]
    [InlineData(typeof(IFilePartyFactory), typeof(IStorageWriter), false)]
    [InlineData(typeof(IAsyncFilePartyFactory), typeof(IAsyncStorageReader), false)]
    [InlineData(typeof(IAsyncFilePartyFactory), typeof(IAsyncStorageWriter), false)]
    [InlineData(typeof(IFilePartyFactory), typeof(IStorageReader), true)]
    [InlineData(typeof(IFilePartyFactory), typeof(IStorageWriter), true)]
    [InlineData(typeof(IAsyncFilePartyFactory), typeof(IAsyncStorageReader), true)]
    [InlineData(typeof(IAsyncFilePartyFactory), typeof(IAsyncStorageWriter), true)]
    public async Task GetSpecializedStorageServices(Type factory, Type serviceInterface, bool? useConfig)
    {
        var sc = this.AddFileParty(cfg => { cfg.AddModule<TestModule>(new TestConfiguration()); });

        var sp = sc.BuildServiceProvider();
        var factoryService = sp.GetRequiredService(factory);

        switch (factoryService)
        {
            case IFilePartyFactory f:
            {
                var storageProvider =
                    useConfig == null ? serviceInterface == typeof(IStorageReader) ? (object) f.GetStorageReader() :
                    serviceInterface == typeof(IStorageWriter) ? f.GetStorageWriter() : null :
                    useConfig.GetValueOrDefault(true) ? serviceInterface == typeof(IStorageReader)
                        ?
                        (object) f.GetStorageReader(new TestConfiguration())
                        :
                        serviceInterface == typeof(IStorageWriter)
                            ? f.GetStorageWriter(new TestConfiguration())
                            : null :
                    serviceInterface == typeof(IStorageReader) ? (object) f.GetStorageReader<TestModule>() :
                    serviceInterface == typeof(IStorageWriter) ? f.GetStorageWriter<TestModule>() : null;

                Assert.NotNull(storageProvider);
                Assert.True(serviceInterface.IsInstanceOfType(storageProvider));
                break;
            }
            case IAsyncFilePartyFactory af:
            {
                var storageProvider =
                    useConfig == null
                        ? serviceInterface == typeof(IAsyncStorageReader)
                            ?
                            (object) await af.GetAsyncStorageReader()
                            :
                            serviceInterface == typeof(IAsyncStorageWriter)
                                ? await af.GetAsyncStorageWriter()
                                : null
                        :
                        useConfig.GetValueOrDefault(true)
                            ? serviceInterface == typeof(IAsyncStorageReader)
                                ?
                                (object) await af.GetAsyncStorageReader<TestModule>(new TestConfiguration())
                                :
                                serviceInterface == typeof(IAsyncStorageWriter)
                                    ? await af.GetAsyncStorageWriter<TestModule>(new TestConfiguration())
                                    : null
                            :
                            serviceInterface == typeof(IAsyncStorageReader)
                                ?
                                (object) await af.GetAsyncStorageReader<TestModule>()
                                :
                                serviceInterface == typeof(IAsyncStorageWriter)
                                    ? await af.GetAsyncStorageWriter<TestModule>()
                                    : null;

                Assert.NotNull(storageProvider);
                Assert.True(serviceInterface.IsInstanceOfType(storageProvider));
                break;
            }
            // let's hope it never gets here
            default:
                Assert.True(false);
                break;
        }
    }
}