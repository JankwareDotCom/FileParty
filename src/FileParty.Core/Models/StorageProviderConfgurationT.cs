using System.IO;
using FileParty.Core.Interfaces;

namespace FileParty.Core.Models
{
    /// <summary>
    /// Factory will use this configuration to determine which storage provider to produce and configure.
    /// Each storage provider should have a constructor that takes a configuration model created for that provider.
    /// </summary>
    /// <typeparam name="TStorageProvider"></typeparam>
    public abstract class StorageProviderConfiguration<TStorageProvider> 
        where TStorageProvider : class, IStorageProvider
    {
        public virtual char DirectorySeparationCharacter { get; } = Path.DirectorySeparatorChar;
    }
}