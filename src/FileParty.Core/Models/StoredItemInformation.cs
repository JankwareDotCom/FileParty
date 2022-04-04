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
            new(StringComparer.InvariantCultureIgnoreCase);


        /// <summary>
        ///     Trys to get a property value as type
        /// </summary>
        /// <param name="propertyName">Property Name in collection</param>
        /// <param name="value">Out as Value Variable</param>
        /// <param name="defaultValue">Default value for when property is not found, or does not cast</param>
        /// <typeparam name="T">Type of property</typeparam>
        /// <returns>True if property is found in collection</returns>
        public bool TryGetProperty<T>(string propertyName, out T value, T defaultValue = default)
        {
            value = defaultValue;

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return false;
            }

            if (!Properties.TryGetValue(propertyName, out var propertyValue))
            {
                return false;
            }

            var isNullable = IsNullable<T>(propertyValue);
            if (propertyValue is T tValue)
            {
                value = tValue;
            }
            else if (isNullable && propertyValue is null)
            {
                value = default;
            }

            return true;
        }

        private static bool IsNullable<T>(object obj)
        {
            if (obj == null) return true;
            var type = typeof(T);
            if (!type.IsValueType) return true;
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}