﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileParty.Core.Enums;
using FileParty.Core.EventArgs;
using FileParty.Core.Interfaces;
using FileParty.Core.Models;

namespace FileParty.Core.Tests;

public class TestStorageProvider : BaseStorageProvider<TestModule>
{
    public TestStorageProvider(
        StorageProviderConfiguration<TestModule> configuration,
        IFakeService1 fs1,
        IFakeService2 fs2,
        FakeService3 fs3,
        FakeService4 fs4)
        : base(configuration)
    {
    }

    public override void Write(FilePartyWriteRequest request)
    {
        for (var i = 1; i <= 10; i++)
        {
            WriteProgressEvent?.Invoke(this,
                new WriteProgressEventArgs(request.Id, request.StoragePointer, 10 * i, 100));
            Thread.Sleep(i * 10);
        }
    }

    public override void Delete(string storagePointer)
    {
        throw new NotImplementedException();
    }

    public override void Delete(IEnumerable<string> storagePointers)
    {
        throw new NotImplementedException();
    }

    public override event EventHandler<WriteProgressEventArgs> WriteProgressEvent;

    public override Stream Read(string storagePointer)
    {
        throw new NotImplementedException();
    }

    public override bool Exists(string storagePointer)
    {
        throw new NotImplementedException();
    }

    public override IDictionary<string, bool> Exists(IEnumerable<string> storagePointers)
    {
        throw new NotImplementedException();
    }

    public override bool TryGetStoredItemType(string storagePointer, out StoredItemType? type)
    {
        throw new NotImplementedException();
    }

    public override IStoredItemInformation GetInformation(string storagePointer)
    {
        throw new NotImplementedException();
    }
}

public class TestAsyncStorageProvider : BaseAsyncStorageProvider<TestModule>
{
    public TestAsyncStorageProvider(StorageProviderConfiguration<TestModule> configuration) : base(configuration)
    {
    }

    public override Task<Stream> ReadAsync(string storagePointer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> ExistsAsync(string storagePointer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<IDictionary<string, bool>> ExistsAsync(IEnumerable<string> storagePointers,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<StoredItemType?> TryGetStoredItemTypeAsync(string storagePointer,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<IStoredItemInformation> GetInformationAsync(string storagePointer,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task WriteAsync(FilePartyWriteRequest request, CancellationToken cancellationToken)
    {
        for (var i = 1; i <= 10; i++)
        {
            WriteProgressEvent?.Invoke(this,
                new WriteProgressEventArgs(request.Id, request.StoragePointer, 10 * i, 100));
            Thread.Sleep(i * 10);
        }

        return Task.CompletedTask;
    }

    public override Task DeleteAsync(string storagePointer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync(IEnumerable<string> storagePointers, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override event EventHandler<WriteProgressEventArgs> WriteProgressEvent;
}

public class TestStorageProvider2 : BaseStorageProvider<TestModule2>
{
    public TestStorageProvider2(StorageProviderConfiguration<TestModule2> configuration) : base(configuration)
    {
    }

    protected override void Dispose()
    {
        Debug.WriteLine("MEMORY: " + GC.GetTotalMemory(false));
        base.Dispose();
    }

    public override event EventHandler<WriteProgressEventArgs> WriteProgressEvent;

    public override void Write(FilePartyWriteRequest request)
    {
        for (var i = 1; i <= 10; i++)
        {
            WriteProgressEvent?.Invoke(this,
                new WriteProgressEventArgs(request.Id, request.StoragePointer, 10 * i, 100));
            Thread.Sleep(i * 10);
        }
    }

    public override void Delete(string storagePointer)
    {
        throw new NotImplementedException();
    }

    public override void Delete(IEnumerable<string> storagePointers)
    {
        throw new NotImplementedException();
    }

    public override Stream Read(string storagePointer)
    {
        throw new NotImplementedException();
    }

    public override bool Exists(string storagePointer)
    {
        throw new NotImplementedException();
    }

    public override IDictionary<string, bool> Exists(IEnumerable<string> storagePointers)
    {
        throw new NotImplementedException();
    }

    public override bool TryGetStoredItemType(string storagePointer, out StoredItemType? type)
    {
        throw new NotImplementedException();
    }

    public override IStoredItemInformation GetInformation(string storagePointer)
    {
        throw new NotImplementedException();
    }
}

public class TestAsyncStorageProvider2 : BaseAsyncStorageProvider<TestModule2>
{
    public TestAsyncStorageProvider2(StorageProviderConfiguration<TestModule2> configuration) : base(configuration)
    {
    }

    public override Task<Stream> ReadAsync(string storagePointer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> ExistsAsync(string storagePointer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<IDictionary<string, bool>> ExistsAsync(IEnumerable<string> storagePointers,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<StoredItemType?> TryGetStoredItemTypeAsync(string storagePointer,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<IStoredItemInformation> GetInformationAsync(string storagePointer,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task WriteAsync(FilePartyWriteRequest request, CancellationToken cancellationToken)
    {
        for (var i = 1; i <= 10; i++)
        {
            WriteProgressEvent?.Invoke(this,
                new WriteProgressEventArgs(request.Id, request.StoragePointer, 10 * i, 100));
            Thread.Sleep(i * 10);
        }

        return Task.CompletedTask;
    }

    public override Task DeleteAsync(string storagePointer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync(IEnumerable<string> storagePointers, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override event EventHandler<WriteProgressEventArgs> WriteProgressEvent;
}