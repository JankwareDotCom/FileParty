using FileParty.Core.Models;

namespace FileParty.Core.RegistrationTests
{
    public class TestConfiguration : StorageProviderConfiguration<TestModule>
    {
        
    }

    public class TestConfiguration2 : StorageProviderConfiguration<TestModule2>
    {
        public override char DirectorySeparationCharacter => '@';
    }
    
    public class TestConfiguration3 : StorageProviderConfiguration<TestModule2>
    {
        public override char DirectorySeparationCharacter => '#';
    }
}