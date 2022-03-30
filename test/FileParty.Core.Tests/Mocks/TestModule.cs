using FileParty.Core.Registration;

namespace FileParty.Core.Tests
{
    public class TestModule : BaseFilePartyModule<TestStorageProvider, TestAsyncStorageProvider>
    {
        public TestModule()
        {
            this.RegisterModuleDependency<TestModule, IFakeService1, FakeService1>((x,y) => new FakeService1());
            this.RegisterModuleDependency<TestModule, IFakeService2, FakeService2>();
            this.RegisterModuleDependency<TestModule, FakeService3>();
            this.RegisterModuleDependency<TestModule, FakeService4>((a, b) => new FakeService4());
        }
    }

    public class TestModule2 : BaseFilePartyModule<TestStorageProvider2, TestAsyncStorageProvider2>
    {
        
    }
}