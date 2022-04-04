using System;
using System.Threading.Tasks;
using Amazon.S3;
using FileParty.Core.Models;
using FileParty.Providers.AWS.S3.Interfaces;

namespace FileParty.Providers.AWS.S3
{
    internal class FilePartyS3ClientWrapper : IDisposable, IAsyncDisposable
    {
        private readonly IFilePartyS3ClientFactory _clientFactory;
        private readonly object _clientLock = new object();
        private readonly StorageProviderConfiguration<AWS_S3Module> _config;
        private AmazonS3Client _client;

        public FilePartyS3ClientWrapper(IFilePartyS3ClientFactory clientFactory,
            StorageProviderConfiguration<AWS_S3Module> config = null)
        {
            _clientFactory = clientFactory;
            _config = config;
            CreateNewClient();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        private void CreateNewClient()
        {
            lock (_clientLock)
            {
                _client?.Dispose();
                _client = _config == null
                    ? _clientFactory.GetClient()
                    : _clientFactory.GetClient(_config);
            }
        }

        public void Execute(Action<AmazonS3Client> action)
        {
            try
            {
                action(_client);
            }
            catch (ObjectDisposedException e)
            {
                if (e.ObjectName != "Amazon.S3.AmazonS3Client") throw;

                CreateNewClient();
                action(_client);
            }
        }

        public Task ExecuteAsync(Func<AmazonS3Client, Task> action)
        {
            try
            {
                return action(_client);
            }
            catch (ObjectDisposedException e)
            {
                if (e.ObjectName != "Amazon.S3.AmazonS3Client") throw;

                CreateNewClient();
                return action(_client);
            }
        }

        public T Execute<T>(Func<AmazonS3Client, T> action)
        {
            try
            {
                return action(_client);
            }
            catch (ObjectDisposedException e)
            {
                if (e.ObjectName != "Amazon.S3.AmazonS3Client") throw;

                CreateNewClient();
                return action(_client);
            }
        }

        public Task<T> ExecuteAsync<T>(Func<AmazonS3Client, Task<T>> action)
        {
            try
            {
                return action(_client);
            }
            catch (ObjectDisposedException e)
            {
                if (e.ObjectName != "Amazon.S3.AmazonS3Client") throw;

                CreateNewClient();
                return action(_client);
            }
        }
    }
}