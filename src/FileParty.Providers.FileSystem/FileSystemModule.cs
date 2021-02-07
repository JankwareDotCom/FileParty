using System;
using FileParty.Core;

namespace FileParty.Providers.FileSystem
{
    public class FileSystemModule : BaseFilePartyModule
    {
        public override Type GetStorageProviderType() => typeof(FileSystemStorageProvider);

        public override Type GetAsyncStorageProviderType() => typeof(FileSystemStorageProvider);
    }
}