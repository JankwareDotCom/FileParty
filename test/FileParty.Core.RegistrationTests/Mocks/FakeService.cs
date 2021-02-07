using System;

namespace FileParty.Core.RegistrationTests
{
    public class FakeService1 : IFakeService1
    {
        public FakeService1()
        {
            
        }

        public Guid InstanceId { get; } = Guid.NewGuid();
        
        public Guid[] GetInstanceIds()
        {
            return new[] {InstanceId};
        }
    }
    
    public class FakeService2 : IFakeService2
    {
        private readonly IFakeService1 _fakeService1;
        
        public FakeService2(IFakeService1 fakeService1)
        {
            _fakeService1 = fakeService1;
        }

        public Guid InstanceId { get; } = Guid.NewGuid();
        
        public Guid[] GetInstanceIds()
        {
            return new[] {_fakeService1.InstanceId, InstanceId};
        }
    }
    
    public class FakeService3: IFakeService3
    {
        private readonly IFakeService1 _fakeService1;
        private readonly IFakeService2 _fakeService2;

        public FakeService3(IFakeService1 fakeService1, IFakeService2 fakeService2)
        {
            _fakeService1 = fakeService1;
            _fakeService2 = fakeService2;
        }

        public Guid InstanceId { get; } = Guid.NewGuid();
        
        public Guid[] GetInstanceIds()
        {
            return new[] {_fakeService1.InstanceId, _fakeService2.InstanceId, InstanceId};
        }
    }
}