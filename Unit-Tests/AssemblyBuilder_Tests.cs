using System;
using System.Reflection.Emit;
using Xunit;
using static System.Reflection.TypeAttributes;

namespace Unit_Tests;

public class AssemblyBuilder_Tests
{
    [Fact]
    public void Test()
    {
        var ab = AssemblyBuilder.DefineDynamicAssembly(new("test"), AssemblyBuilderAccess.RunAndCollect);
        var mb = ab.DefineDynamicModule("test");
        var tb = mb.DefineType("test", Public);
        var tb2 = mb.DefineType("test2", Public);
        var tb3 = tb2.DefineNestedType("test3", NestedPublic);
        
        // build assembly types
        var t = tb.CreateType();
        var ti = Activator.CreateInstance(t);
        var t2 = tb2.CreateType();
        var asd = tb2.CreateType();
        var ti2 = Activator.CreateInstance(tb2);
    }
}