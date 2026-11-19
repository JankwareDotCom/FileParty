using System;
using FileParty.Core.Interfaces;
using FileParty.Core.Registration;
using FileParty.Providers.AWS.S3;
using FileParty.Providers.AWS.S3.Config;
using FileParty.Providers.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace FileParty.Compat
{
    // Touches the public registration surface of every package so the build fails if a type the
    // consumer depends on is missing or cannot bind against the pinned dependency versions.
    internal static class Program
    {
        private static int Main()
        {
            var services = new ServiceCollection()
                .AddFileParty(c => c
                    .AddModule(new FileSystemConfiguration(AppDomain.CurrentDomain.BaseDirectory))
                    .AddModule(new AWSDefaultConfiguration { Region = "us-east-1", Name = "compat-bucket" })
#if FILEPARTY_AZURE
                    .AddModule(new FileParty.Providers.Azure.Storage.AzureBlobConfiguration())
#endif
                    .SetDefaultModule<FileSystemModule>());

            using (var provider = services.BuildServiceProvider())
            {
                var factory = provider.GetRequiredService<IFilePartyFactory>();
                Console.WriteLine($"FileParty compat OK: {factory.GetStorageProvider().GetType().FullName}");
            }

            return 0;
        }
    }
}
