using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;
using FileParty.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FileParty.Core.RegistrationTests
{
    public class FilePartyWriteSubscriptionsShould
    {
        [Fact]
        public async Task SubscribeToAllWriteEventsAcrossProviders()
        {
            var sc = this.AddFileParty(
            cfg => cfg.AddModule<TestModule2>(new TestConfiguration2()),
            cfg => cfg.AddModule<TestModule>(new TestConfiguration()));

            await using var sp = sc.BuildServiceProvider();
            
            var factory = sp.GetRequiredService<IFilePartyFactory>();
            var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
            var subManager = sp.GetRequiredService<IWriteProgressSubscriptionManager>();
            var storage = factory.GetStorageProvider();
            var asyncStorage = await asyncFactory.GetAsyncStorageProvider();
            var storage2 = factory.GetStorageProvider<TestModule2>();
            var asyncStorage2 = await asyncFactory.GetAsyncStorageProvider<TestModule2>();

            var infoList = new List<WriteProgressInfo>();
            
            void HandleWriteProgress(object sender, WriteProgressInfo info)
            {
                infoList.Add(info);
            }

            var subscriptionId = subManager.SubscribeToAll(HandleWriteProgress);
            
            storage.Write(new FilePartyWriteRequest("1", new MemoryStream()));
            storage2.Write(new FilePartyWriteRequest("2", new MemoryStream()));
            await asyncStorage.WriteAsync(new FilePartyWriteRequest("3", new MemoryStream()), CancellationToken.None);
            await asyncStorage2.WriteAsync(new FilePartyWriteRequest("4", new MemoryStream()), CancellationToken.None);

            Assert.NotEmpty(infoList);

            for (var i = 1; i <= 4; i++)
            {
                Assert.Equal(10, infoList.Count(c=>c.StoragePointer == i.ToString()));    
            }
        }

        [Fact] 
        public async Task AllowUnsubscribeToAll()
        {
            var sc = this.AddFileParty(
                cfg => cfg.AddModule<TestModule2>(new TestConfiguration2()),
                cfg => cfg.AddModule<TestModule>(new TestConfiguration()));

            await using var sp = sc.BuildServiceProvider();
            
            var factory = sp.GetRequiredService<IFilePartyFactory>();
            var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
            var subManager = sp.GetRequiredService<IWriteProgressSubscriptionManager>();
            var storage = factory.GetStorageProvider();
            var asyncStorage = await asyncFactory.GetAsyncStorageProvider();
            var storage2 = factory.GetStorageProvider<TestModule2>();
            var asyncStorage2 = await asyncFactory.GetAsyncStorageProvider<TestModule2>();

            var infoList = new List<WriteProgressInfo>();
            
            void HandleWriteProgress(object sender, WriteProgressInfo info)
            {
                infoList.Add(info);
            }

            var subscriptionId = subManager.SubscribeToAll(HandleWriteProgress);
            
            storage.Write(new FilePartyWriteRequest("1", new MemoryStream()));
            storage2.Write(new FilePartyWriteRequest("2", new MemoryStream()));
            
            subManager.UnsubscribeFromAll(subscriptionId);
            
            await asyncStorage.WriteAsync(new FilePartyWriteRequest("3", new MemoryStream()), CancellationToken.None);
            await asyncStorage2.WriteAsync(new FilePartyWriteRequest("4", new MemoryStream()), CancellationToken.None);

            Assert.NotEmpty(infoList);

            Assert.Equal(20, infoList.Count);
            for (var i = 1; i <= 2; i++)
            {
                Assert.Equal(10, infoList.Count(c=>c.StoragePointer == i.ToString()));    
            }
        }
        
        [Fact]
        public async Task AllowUnsubscribeToRequest()
        {
            var sc = this.AddFileParty(
                cfg => cfg.AddModule<TestModule2>(new TestConfiguration2()),
                cfg => cfg.AddModule<TestModule>(new TestConfiguration()));

            await using var sp = sc.BuildServiceProvider();
            
            var factory = sp.GetRequiredService<IFilePartyFactory>();
            var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
            var subManager = sp.GetRequiredService<IWriteProgressSubscriptionManager>();
            var storage = factory.GetStorageProvider();
            var asyncStorage = await asyncFactory.GetAsyncStorageProvider();
            var storage2 = factory.GetStorageProvider<TestModule2>();
            var asyncStorage2 = await asyncFactory.GetAsyncStorageProvider<TestModule2>();

            var infoList = new List<WriteProgressInfo>();
            
            void HandleWriteProgress(object sender, WriteProgressInfo info)
            {
                infoList.Add(info);
            }

            var req1 = new FilePartyWriteRequest("1", new MemoryStream());
            var req4 = new FilePartyWriteRequest("4", new MemoryStream());

            var handlerId1 = subManager.SubscribeToRequest(req1.Id, HandleWriteProgress);
            var handlerId4 = subManager.SubscribeToRequest(req4.Id, HandleWriteProgress);
            
            storage.Write(req1);
            storage2.Write(new FilePartyWriteRequest("2", new MemoryStream()));

            subManager.UnsubscribeFromRequest(req4.Id, handlerId4);
            
            await asyncStorage.WriteAsync(new FilePartyWriteRequest("3", new MemoryStream()), CancellationToken.None);
            await asyncStorage2.WriteAsync(req4, CancellationToken.None);

            Assert.NotEmpty(infoList);
            Assert.Equal(10, infoList.Count);
            Assert.Equal(10, infoList.Count(c=>c.StoragePointer == "1"));    
            Assert.Equal(0, infoList.Count(c=>c.StoragePointer == "4"));
            
            Assert.Empty(subManager.GetRequestHandlers(req1.Id));
            Assert.Empty(subManager.GetRequestHandlers(req4.Id));
        }
        
        [Fact]
        public async Task SubscribeToSpecificWriteRequestsAcrossProvidersWithAutoMagicUnsubscribe()
        {
            var sc = this.AddFileParty(
                cfg => cfg.AddModule<TestModule2>(new TestConfiguration2()),
                cfg => cfg.AddModule<TestModule>(new TestConfiguration()));

            await using var sp = sc.BuildServiceProvider();
            
            var factory = sp.GetRequiredService<IFilePartyFactory>();
            var asyncFactory = sp.GetRequiredService<IAsyncFilePartyFactory>();
            var subManager = sp.GetRequiredService<IWriteProgressSubscriptionManager>();
            var storage = factory.GetStorageProvider();
            var asyncStorage = await asyncFactory.GetAsyncStorageProvider();
            var storage2 = factory.GetStorageProvider<TestModule2>();
            var asyncStorage2 = await asyncFactory.GetAsyncStorageProvider<TestModule2>();

            var infoList = new List<WriteProgressInfo>();
            
            void HandleWriteProgress(object sender, WriteProgressInfo info)
            {
                infoList.Add(info);
            }

            var req1 = new FilePartyWriteRequest("1", new MemoryStream());
            var req4 = new FilePartyWriteRequest("4", new MemoryStream());

            subManager.SubscribeToRequest(req1.Id, HandleWriteProgress);
            subManager.SubscribeToRequest(req4.Id, HandleWriteProgress);
            
            storage.Write(req1);
            storage2.Write(new FilePartyWriteRequest("2", new MemoryStream()));
            await asyncStorage.WriteAsync(new FilePartyWriteRequest("3", new MemoryStream()), CancellationToken.None);
            await asyncStorage2.WriteAsync(req4, CancellationToken.None);

            Assert.NotEmpty(infoList);
            Assert.Equal(20, infoList.Count);
            Assert.Equal(10, infoList.Count(c=>c.StoragePointer == "1"));    
            Assert.Equal(10, infoList.Count(c=>c.StoragePointer == "4"));
            
            Assert.Empty(subManager.GetRequestHandlers(req1.Id));
            Assert.Empty(subManager.GetRequestHandlers(req4.Id));
        }
    }
}