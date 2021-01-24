using System;
using System.Collections.Generic;
using FileParty.Core.Enums;

namespace FileParty.Core.Interfaces
{
    public interface IStoredItemInformation
    {
        /// <summary>
        ///     Type of Item
        /// </summary>
        StoredItemType StoredType { get; }

        /// <summary>
        ///     Gets the path to the directory containing this stored item
        /// </summary>
        string DirectoryPath { get; }

        /// <summary>
        ///     The name of the stored item
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The full path or reference id for accessing the stored item
        /// </summary>
        string StoragePointer { get; }

        /// <summary>
        ///     Size in Bytes
        /// </summary>
        long? Size { get; }

        /// <summary>
        ///     Creation time
        /// </summary>
        DateTime? CreatedTimestamp { get; }

        /// <summary>
        ///     Last modified time
        /// </summary>
        DateTime? LastModifiedTimestamp { get; }

        /// <summary>
        ///     Misc. Properties of this StoredItem
        /// </summary>
        public Dictionary<string, object> Properties { get; }
    }
}