using System;
using FileParty.Core.Models;

namespace FileParty.Core.EventArgs
{
    public class WriteProgressEventArgs
    {
        public Guid WriteRequestId { get; }
        
        public WriteProgressInfo WriteProgressInfo { get; }

        /// <summary>
        /// DateTime when the event was raised
        /// </summary>
        public DateTime RaiseDate { get; set; } = DateTime.UtcNow;

        public WriteProgressEventArgs(Guid id, string storagePointer, long totalBytesTransferred, long totalFileBytes)
        {
            WriteRequestId = id;
            WriteProgressInfo = new WriteProgressInfo
            {
                WriteProgressId = id,
                StoragePointer = storagePointer,
                TotalBytesTransferred = totalBytesTransferred,
                TotalBytesRemaining = totalFileBytes - totalBytesTransferred,
                TotalFileBytes = totalFileBytes,
                PercentComplete = (int) Math.Round((double) (totalBytesTransferred * 100) / totalFileBytes)
            };
        }
    }
}