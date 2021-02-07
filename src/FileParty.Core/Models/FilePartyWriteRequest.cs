using System;
using System.IO;
using FileParty.Core.Enums;

namespace FileParty.Core.Models
{
    public class FilePartyWriteRequest
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime RequestCreatedAt { get; } = DateTime.UtcNow;
        public string StoragePointer { get; set; }
        public WriteMode WriteMode { get; set; }
        public Stream Stream { get; set; }

        public FilePartyWriteRequest(string storagePointer, Stream stream, WriteMode writeMode = WriteMode.Create)
        {
            StoragePointer = storagePointer;
            WriteMode = writeMode;
            Stream = stream;
        }

        public static FilePartyWriteRequest Create(string storagePointer, Stream stream, out Guid requestId, WriteMode writeMode = WriteMode.Create)
        {
            var request = new FilePartyWriteRequest(storagePointer, stream, writeMode);
            requestId = request.Id;
            return request;
        }
    }
}