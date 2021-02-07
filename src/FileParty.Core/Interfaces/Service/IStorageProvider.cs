using System;

namespace FileParty.Core.Interfaces
{
    /// <summary>
    /// A common interface for storage, agnostic to cloud or local hosting.
    /// </summary>
    public interface IStorageProvider :
        IStorageWriter, 
        IStorageReader
    {
        
    }
}