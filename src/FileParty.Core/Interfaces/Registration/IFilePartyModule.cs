using System;

namespace FileParty.Core.Interfaces
{ 
    public interface IFilePartyModule
    {
        /// <summary>
        /// Returns Storage Provider Type
        /// </summary>
        Type GetStorageProviderType();
        
        /// <summary>
        /// Returns Async Storage Provider Type
        /// </summary>
        Type GetAsyncStorageProviderType();
    }
}