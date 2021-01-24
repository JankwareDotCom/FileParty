using System;
using System.Collections.Generic;
using FileParty.Core.Enums;
using FileParty.Core.Interfaces;

namespace FileParty.Core.Models
{
    public class StoredItemInformation : IStoredItemInformation
    {
        public StoredItemType StoredType { get; set; }

        /// <inheritdoc />
        public string DirectoryPath { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string StoragePointer { get; set; }

        /// <inheritdoc />
        public long? Size { get; set; }

        /// <inheritdoc />
        public DateTime? CreatedTimestamp { get; set; }

        /// <inheritdoc />
        public DateTime? LastModifiedTimestamp { get; set; }

        /// <inheritdoc />
        public Dictionary<string, object> Properties { get; } =
            new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);


        public bool TryGetProperty<T>(string propertyName, out T value, T defaultValue = default)
        {
            if (!string.IsNullOrWhiteSpace(propertyName) &&
                Properties.TryGetValue(propertyName, out var propertyValue) &&
                propertyValue is T tValue)
            {
                value = tValue;
                return true;
            }

            value = defaultValue;
            return false;
        }
    }
}