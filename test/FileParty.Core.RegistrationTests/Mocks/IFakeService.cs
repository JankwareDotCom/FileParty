using System;

namespace FileParty.Core.RegistrationTests
{
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
}