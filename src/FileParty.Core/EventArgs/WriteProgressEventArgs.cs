using System;
using FileParty.Core.Models;

namespace FileParty.Core.EventArgs
{
    public class WriteProgressEventArgs
    {
        public WriteProgressEventArgs(Guid id, string storagePointer, long totalBytesTransferred, long totalFileBytes)
            : this(id, storagePointer, totalBytesTransferred, totalFileBytes, DateTime.MinValue)
        {
        }


        public WriteProgressEventArgs(Guid id, string storagePointer, long totalBytesTransferred, long totalFileBytes,
            DateTime requestCreatedAt)
        {
            RaiseDate = DateTime.UtcNow;
            WriteRequestId = id;
            WriteProgressInfo = new WriteProgressInfo
            {
                WriteProgressId = id,
                StoragePointer = storagePointer,
                TotalBytesTransferred = totalBytesTransferred,
                TotalBytesRemaining = totalFileBytes - totalBytesTransferred,
                TotalFileBytes = totalFileBytes,
                PercentComplete = (int) Math.Round((double) (totalBytesTransferred * 100) / totalFileBytes),
                RequestCreatedAt = requestCreatedAt
            };
        }

        public Guid WriteRequestId { get; }

        public WriteProgressInfo WriteProgressInfo { get; }

        /// <summary>
        ///     DateTime when the event was raised
        /// </summary>
        public DateTime RaiseDate { get; set; }
    }
}