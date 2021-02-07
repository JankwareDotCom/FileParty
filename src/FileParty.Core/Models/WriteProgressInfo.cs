using System;

namespace FileParty.Core.Models
{
    public class WriteProgressInfo
    {
        /// <summary>
        /// Identifier for write progress request
        /// </summary>
        public Guid WriteProgressId { get; set; } 
        
        /// <summary>
        /// Generally the path where the file is stored
        /// </summary>
        public string StoragePointer { get; set; }

        /// <summary>
        /// Total bytes transferred
        /// </summary>
        public long TotalBytesTransferred { get; set; }

        /// <summary>
        /// Remaining bytes to be transferred
        /// </summary>
        public long TotalBytesRemaining { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long TotalFileBytes { get; set; }

        /// <summary>
        /// Percent complete
        /// </summary>
        public int PercentComplete { get; set; }
    }
}