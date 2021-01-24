using System;

namespace FileParty.Core.Exceptions
{
    public class StorageException : Exception
    {
        public StorageException()
        {
        }

        public StorageException(string message)
        {
            Message = message;
        }

        public StorageException(string errorCode, string message) : this(message)
        {
            ErrorCode = errorCode;
        }
        
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string ErrorCode { get; } = string.Empty;
        public override string Message { get; }
    }
}