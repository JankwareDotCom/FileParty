using System;

namespace FileParty.Core.Tests;

public interface IFakeService1
{
    Guid InstanceId { get; }
    Guid[] GetInstanceIds();
}

public interface IFakeService2 : IFakeService1
{
}

public interface IFakeService3 : IFakeService2
{
}