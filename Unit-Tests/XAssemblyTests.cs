using Illumination;
using Xunit;

namespace Unit_Tests;

public class XAssemblyTests
{
    [Fact]
    public void XAssembly()
    {
        var assembly = new XAssembly("TestAssembly");
        Assert.Equal("TestAssembly", assembly.Name);
        Assert.Empty(assembly.Modules);
        var module = assembly.Module("TestModule");
        // Assert.Single(assembly.Modules);
        // Assert.Equal("TestModule", module.Name);
        // Assert.Empty(module.Types);
        // Assert.Empty(module.Methods);
    }
}