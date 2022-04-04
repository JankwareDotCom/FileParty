using FileParty.Core.Interfaces;

namespace FileParty.Core.Tests;

public abstract class BaseStorageProviderTest<TModule, TStorageProvider, TAsyncStorageProvider>
    where TModule : class, IFilePartyModule
    where TStorageProvider : class, IStorageProvider
    where TAsyncStorageProvider : class, IAsyncStorageProvider
{
    
}