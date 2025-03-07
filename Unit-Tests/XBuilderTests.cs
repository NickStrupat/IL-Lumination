using System.Reflection.Emit;
using Illumination;
using Xunit;

namespace Unit_Tests;

public class XBuilderTests
{
    [Fact]
    public void XAssemblyBuilder_Generate()
    {
        //var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new(name), RunAndCollect);
        XAssemblyBuilder xab = new("TestAssembly");
        var xtb = new XTypeBuilder { Name = "TestType" };
        xab.Types.Add(xtb);
        xtb.TypeParameters.Add(new XTypeParameterBuilder("T"));
        //xab.GenerateAssembly("TestAssembly.dll");
    }
}