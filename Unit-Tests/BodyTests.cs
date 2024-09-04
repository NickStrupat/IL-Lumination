using System;
using System.Reflection.Emit;
using System.Threading;
using IL_Lumination;
using Xunit;

namespace Unit_Tests;

public class BodyTests
{
    [Theory]
    [InlineData(3, 1, 2)]
    [InlineData(5, 2, 3)]
    [InlineData(7, 3, 4)]
    [InlineData(-3, -1, -2)]
    public void Add(Int32 result, Int32 a, Int32 b)
    {
        var int32 = typeof(Int32);
        var dm = new DynamicMethod("", int32, [int32, int32]);
        var body = dm.GetILGenerator().Body();
        body.Ldarg_S(0).Ldarg(1).Add().Ret();
        var del = dm.CreateDelegate<Func<Int32, Int32, Int32>>();
        Assert.Equal(result, del(a, b));
    }
    
    [Theory]
    [InlineData(3, 1, 2)]
    [InlineData(5, 2, 3)]
    [InlineData(7, 3, 4)]
    [InlineData(-3, -1, -2)]
    public void Add_Ovf(Int32 result, Int32 a, Int32 b)
    {
        var int32 = typeof(Int32);
        var dm = new DynamicMethod("", int32, [int32, int32]);
        var body = dm.GetILGenerator().Body();
        body.Ldarg_S(0).Ldarg(1).Add_Ovf().Ret();
        var del = dm.CreateDelegate<Func<Int32, Int32, Int32>>();
        Assert.Equal(result, del(a, b));
    }

    [Fact]
    public void DivideByZeroThrows()
    {
        var dm = new DynamicFunc<Int32>();
        dm.Body().Ldc_I4_1().Ldc_I4_0().Div().Ret();
        var del = dm.CreateDelegate();
        Assert.Throws<DivideByZeroException>(() => del());
    }

    [Fact]
    public void CallConstructor()
    {
        var dm = new DynamicFunc<String>();
        dm.Body().Newobj<HasCtor>().Ret();
        var del = dm.CreateDelegate();
        var actual = del();
        Assert.NotNull(actual);
    }
    class HasCtor;

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void CatchThrownException(Boolean @throw, Int32 expected)
    {
        var dm = new DynamicFunc<Boolean, Int32>();
        dm.Body()
            .DeclareLocal<Int32>(out var result)
            .DefineLabel(out var noThrow)
            .DefineLabel(out var done)
            .BeginExceptionBlock()
            .Ldarg(0)
            .Brfalse_S(noThrow)
            .Newobj(typeof(Exception).GetConstructor([])!)
            .Throw()
            .MarkLabel(noThrow)
            .Ldc_I4_0()
            .Stloc(result)
            .Leave_S(done)
            .BeginCatchBlock(typeof(Object))
            .Pop()
            .Ldc_I4_1()
            .Stloc(result)
            .Leave_S(done)
            .EndExceptionBlock()
            .MarkLabel(done)
            .Ldloc(result)
            .Ret();
        var del = dm.CreateDelegate();
        var actual = del(@throw);
        Assert.Equal(expected, actual);
    }
    
    public static int Try_Catch4(bool b)
    {
        try
        {
            if (b)
                throw new();
            return 0;
        }
        catch
        {
            return 1;
        }
    }

    public static Int32 B(short a, short b) => a & b;
}