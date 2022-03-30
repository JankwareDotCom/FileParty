using System;
using System.Threading.Tasks;
using Amazon.S3;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3
{
    public static class FilePartyS3ClientWrapper
    {
        public static void Execute(this IFilePartyS3ClientFactory filePartyS3ClientFactory, 
            Action<AmazonS3Client> action, StorageProviderConfiguration<AWS_S3Module> config = null)
        {
            var client = config == null
                ? filePartyS3ClientFactory.GetClient()
                : filePartyS3ClientFactory.GetClient(config);
            
            try
            {
                action(client);
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        public static Task ExecuteAsync(this IFilePartyS3ClientFactory filePartyS3ClientFactory,
            Func<AmazonS3Client, Task> action, StorageProviderConfiguration<AWS_S3Module> config = null)
        {
            var client = config == null
                ? filePartyS3ClientFactory.GetClient()
                : filePartyS3ClientFactory.GetClient(config);
            
            try
            {
                return action(client);
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        public static T Execute<T>(this IFilePartyS3ClientFactory filePartyS3ClientFactory,
            Func<AmazonS3Client, T> action, StorageProviderConfiguration<AWS_S3Module> config = null)
        {
            var client = config == null
                ? filePartyS3ClientFactory.GetClient()
                : filePartyS3ClientFactory.GetClient(config);
            
            try
            {
                return action(client);
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        public static Task<T> ExecuteAsync<T>(this IFilePartyS3ClientFactory filePartyS3ClientFactory,
            Func<AmazonS3Client, Task<T>> action, StorageProviderConfiguration<AWS_S3Module> config = null)
        {
            var client = config == null
                ? filePartyS3ClientFactory.GetClient()
                : filePartyS3ClientFactory.GetClient(config);
            
            try
            {
                return action(client);
            }
            finally
            {
                client?.Dispose();
            }
        }
    }
}