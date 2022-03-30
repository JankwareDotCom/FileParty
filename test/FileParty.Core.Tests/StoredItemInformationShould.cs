using System;
using FileParty.Core.Enums;
using FileParty.Core.Models;
using Xunit;

namespace FileParty.Core.Tests;

public class StoredItemInformationShould
{

    [Fact]
    public void TryGetPropertySuccessfully()
    {
        var dt = DateTime.UtcNow.AddDays(-1);
        var info = new StoredItemInformation
        {
            Name = nameof(TryGetPropertySuccessfully),
            CreatedTimestamp = DateTime.UtcNow.AddMinutes(-1),
            StoragePointer = nameof(TryGetPropertySuccessfully),
            DirectoryPath = string.Empty,
            LastModifiedTimestamp = DateTime.UtcNow,
            StoredType = StoredItemType.File,
            Size = 0,
            Properties =
            {
                {"string", "string"},
                {"bool", true},
                {"null", (object) null},
                {"datetime", dt},
                {"integer", 42},
                {"float", 69.420f},
                {"stuff", (DateTime?) DateTime.UtcNow.Date}
            }
        };
        
        Assert.True(info.TryGetProperty("string", out string str, "failed"));
        Assert.Equal("string", str);
        
        Assert.True(info.TryGetProperty("bool", out bool b, false));
        Assert.True(b);
        
        Assert.True(info.TryGetProperty<object>("null", out var obj, new object()));
        Assert.Null(obj);
        
        Assert.True(info.TryGetProperty("datetime", out DateTime dtprop, DateTime.UtcNow));
        Assert.Equal(dt, dtprop);
        
        Assert.True(info.TryGetProperty("integer", out var integer, 8675309));
        Assert.Equal(42, integer);
        
        Assert.True(info.TryGetProperty("float", out float f, 3.14159f));
        Assert.Equal(69.420f, f);

        Assert.False(info.TryGetProperty("doesnotexist", out string dne, "does not exist"));
        Assert.Equal("does not exist", dne);
        
        Assert.False(info.TryGetProperty(null, out string propNameEmpty, "prop name empty"));
        Assert.Equal("prop name empty", propNameEmpty);
        
        Assert.True(info.TryGetProperty("integer", out var integer2, 4815162342f));
        Assert.Equal(4815162342f, integer2);
        
        Assert.True(info.TryGetProperty("stuff", out var stuff, (DateTime?) DateTime.UtcNow));
        Assert.Equal(DateTime.UtcNow.Date, stuff);
    }
}