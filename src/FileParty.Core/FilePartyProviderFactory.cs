using FileParty.Core.Interfaces;
using FileParty.Core.Models;

namespace FileParty.Core
{
    public static class FilePartyProviderFactory
    {
        public static IStorageProvider CreateStorageProvider<TStorageProvider>(
            StorageProviderConfiguration<TStorageProvider> configuration) 
            where TStorageProvider : class, IStorageProvider
        {
            var spType = typeof(TStorageProvider);
            var configType = configuration.GetType();
            var spConstructor = spType.GetConstructor(new[] {configType});

            if (spConstructor == null)
                return null;

            var storageProvider = spConstructor.Invoke(new object[] {configuration});
            return storageProvider as IStorageProvider;
        }
    }
}