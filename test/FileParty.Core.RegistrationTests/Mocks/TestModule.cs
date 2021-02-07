using FileParty.Core.Registration;

namespace FileParty.Core.RegistrationTests
{
    public class TestModule : BaseFilePartyModule<TestStorageProvider, TestAsyncStorageProvider>
    {
        public TestModule()
        {
            this.RegisterModuleDependency<TestModule, IFakeService1, FakeService1>(x => new FakeService1());
            this.RegisterModuleDependency<TestModule, IFakeService2, FakeService2>();
            this.RegisterModuleDependency<TestModule, FakeService3>();
        }
    }

    public class TestModule2 : BaseFilePartyModule<TestStorageProvider2, TestAsyncStorageProvider2>
    {
        
    }
}