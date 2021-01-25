namespace FileParty.Core.Exceptions
{
    public static class Errors
    {
        public static readonly StorageException UnknownException =
            new StorageException("FP-000", "An unknown error has occured.");

        public static readonly StorageException FileNotFoundException =
            new StorageException("FP-001", "The requested file or directory does not exist.");

        public static readonly StorageException FileAlreadyExistsException =
            new StorageException("FP-002", "The file or directory already exists.");

        public static readonly StorageException MustBeFile =
            new StorageException("FP-003", "The request must be for a file.");

        public static readonly StorageException StoragePointerMustHaveValue =
            new StorageException("FP-004", "The storage pointer cannot be null or empty");
    }
}