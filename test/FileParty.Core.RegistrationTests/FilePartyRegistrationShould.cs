using System.Linq;
using System.Threading.Tasks;
using FileParty.Core.Interfaces;
using FileParty.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FileParty.Core.RegistrationTests
{
    public class FilePartyRegistrationShould
    {
        [Fact]
        public async Task RegisterAFilePartyConfigAndConfigForEachModuleRegistered()
        {
            var sc = this.AddFileParty(cfg =>
            {
                cfg.AddModule<TestModule2>(new TestConfiguration2());
                cfg.AddModule<TestModule>(new TestConfiguration());
            });

            Assert.NotNull(sc);
            var descriptors = sc.ToList();
            Assert.NotNull(descriptors.FirstOrDefault(f=>
                f.ServiceType == typeof(IConfiguredFileParty) && 
                f.Lifetime == ServiceLifetime.Singleton));

            Assert.Equal(2, descriptors.Count(f =>
                f.ServiceType == typeof(IFilePartyModuleConfiguration) &&
                f.Lifetime == ServiceLifetime.Singleton &&
                f.ImplementationInstance != null &&
                f.ImplementationInstance.GetType().IsGenericType));

            Assert.Single(descriptors.Where(w => w.ServiceType == typeof(IFilePartyFactory)));
            Assert.Single(descriptors.Where(w => w.ServiceType == typeof(IAsyncFilePartyFactory)));
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
    }
}