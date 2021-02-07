using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileParty.Core.Enums;

namespace FileParty.Core.Interfaces
{
    public interface IStorageReader : IUseDirectorySeparatorCharacter
    {
        /// <summary>
        /// Get a stream of a file from the storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <returns></returns>
        Stream Read(string storagePointer);
        
        /// <summary>
        /// Determines if the file exists in the storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <returns>True when the file exists</returns>
        bool Exists(string storagePointer);

        /// <summary>
        /// Determines if files exists in the storage pointer
        /// </summary>
        /// <param name="storagePointers">Generally the path where the file is stored, as an enumerable</param>
        /// <returns>Dictionary with Storage Pointer as key, and boolean indication existence as value</returns>
        IDictionary<string, bool> Exists(IEnumerable<string> storagePointers);

        /// <summary>
        /// Tries to get a stored item type (Directory / File)
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <param name="type">Output of stored item type, returns File or Directory on success, null on failure</param>
        /// <returns>True when the file exists</returns>
        bool TryGetStoredItemType(string storagePointer, out StoredItemType? type);

        /// <summary>
        /// Returns basic information about a stored file or directory
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <returns><see cref="IStoredItemInformation"/></returns>
        IStoredItemInformation GetInformation(string storagePointer);
    }
}