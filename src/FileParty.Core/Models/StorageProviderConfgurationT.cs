using System.IO;
using FileParty.Core.Interfaces;

namespace FileParty.Core.Models
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TFilePartyModule"></typeparam>
    public abstract class StorageProviderConfiguration<TFilePartyModule> : IStorageProviderConfiguration
        where TFilePartyModule : class, IFilePartyModule, new()
    {
        /// <inheritdoc />
        public virtual char DirectorySeparationCharacter => Path.DirectorySeparatorChar;
    }
}