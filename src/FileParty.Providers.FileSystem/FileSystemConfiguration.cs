using System.IO;
using FileParty.Core.Models;

namespace FileParty.Providers.FileSystem
{
    public class FileSystemConfiguration : StorageProviderConfiguration<FileSystemModule>
    {
        private char _directorySeparator = Path.DirectorySeparatorChar;
        public override char DirectorySeparationCharacter => _directorySeparator;
        public string BasePath { get; set; }

        public FileSystemConfiguration()
        {
            
        }
        
        public FileSystemConfiguration(string basePath)
        {
            UseBasePath(basePath);
        }

        public FileSystemConfiguration UseDirectorySeparationCharacter(char directorySeparator)
        {
            _directorySeparator = directorySeparator;
            return this;
        }

        public FileSystemConfiguration UseBasePath(string baseDirectory)
        {
            BasePath = baseDirectory;
            return this;
        }
    }
}