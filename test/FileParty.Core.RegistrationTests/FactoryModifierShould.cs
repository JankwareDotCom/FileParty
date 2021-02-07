using System.Threading.Tasks;
using FileParty.Core.Interfaces;
using FileParty.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FileParty.Core.RegistrationTests
{
    public class FactoryModifierShould
    {
        [Fact]
        public async Task BeMalleable()
        {
            var sc = this.AddFileParty(cfg =>
            {
                cfg.AddModule<TestModule>(new TestConfiguration());
                cfg.AddModule<TestModule2>(new TestConfiguration2());
                cfg.SetDefaultModule<TestModule>();
            });
        
            await using var sp = sc.BuildServiceProvider();
            var factory = sp.GetRequiredService<IFilePartyFactory>();
            var factoryModifier = sp.GetRequiredService<IFilePartyFactoryModifier>();
            var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
            var storage = factory.GetStorageProvider();
            var asyncStorage = await asyncFactory.GetAsyncStorageProvider();
        
            Assert.Equal(typeof(TestStorageProvider), storage.GetType());
            Assert.Equal(typeof(TestAsyncStorageProvider), asyncStorage.GetType());
            
            factoryModifier.ChangeDefaultModuleConfig<TestModule2>();
            
            storage = factory.GetStorageProvider();
            asyncStorage = await asyncFactory.GetAsyncStorageProvider();
            
            Assert.Equal(typeof(TestStorageProvider2), storage.GetType());
            Assert.Equal(typeof(TestAsyncStorageProvider2), asyncStorage.GetType());
        }
    }
}