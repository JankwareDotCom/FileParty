namespace FileParty.Core.Exceptions
{
    public static class Errors
    {
        public static readonly StorageException UnknownException = new("FP-000", "An unknown error has occured.");

        public static readonly StorageException FileNotFoundException =
            new("FP-001", "The requested file or directory does not exist.");

        public static readonly StorageException FileAlreadyExistsException =
            new("FP-002", "The file or directory already exists.");

        public static readonly StorageException MustBeFile = new("FP-003", "The request must be for a file.");

        public static readonly StorageException StoragePointerMustHaveValue =
            new("FP-004", "The storage pointer cannot be null or empty");

        public static readonly StorageException InvalidConfiguration =
            new("FP-005", "Storage Provider Configuration is Invalid.");

        public static StorageException DefaultConfigNotFound =
            new("FP-006", "A default configuration could not be found for a storage " +
                          "provider module.");

        public static StorageException SPNotFound =
            new("FP-007", "A storage provider could not be found.  Ensure the module to " +
                          "which it belongs has been registered");
    }
}