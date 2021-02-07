using System;
using System.Collections.Generic;
using System.IO;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;
using FileParty.Core.Models;

namespace FileParty.Core.Interfaces
{
    public interface IStorageWriter : IUseDirectorySeparatorCharacter
    {

        /// <summary>
        /// Write a stream to a storage provider
        /// </summary>
        void Write(FilePartyWriteRequest request);
        
        /// <summary>
        /// Write a stream to a storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path to store the file</param>
        /// <param name="stream">Stream to store, assumes you manage disposal</param>
        /// <param name="writeMode">Determine the write type.  Create, Replace, or CreateOrReplace</param>
        void Write(
            string storagePointer,
            Stream stream,
            WriteMode writeMode);
        
        /// <summary>
        /// Delete a file from the storage provider
        /// </summary>
        /// <param name="storagePointer">Generally the path where the file is stored</param>
        /// <returns></returns>
        void Delete(string storagePointer);

        /// <summary>
        /// Delete many files from the storage provider
        /// </summary>
        /// <param name="storagePointers">Generally the path where the file is stored, as an enumerable</param>
        /// <returns></returns>
        void Delete(IEnumerable<string> storagePointers);
        
        /// <summary>
        /// EventHandler for Write Progress, uses <see cref="WriteProgressEventArgs"/>
        /// </summary>
        event EventHandler<WriteProgressEventArgs> WriteProgressEvent;
    }
}