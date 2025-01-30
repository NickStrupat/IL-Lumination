using System;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace Unit_Tests;

public class TryCatchTest
{
    [Fact]
    public void Test()
    {
        var methodBuilder = new DynamicMethod("", typeof(string), null);
        var il = methodBuilder.GetILGenerator();

        LocalBuilder l1 = il.DeclareLocal(typeof(string));
        LocalBuilder l2 = il.DeclareLocal(typeof(Exception));
        LocalBuilder l3 = il.DeclareLocal(typeof(bool));
        LocalBuilder l4 = il.DeclareLocal(typeof(string));

        Label ll1 = il.DefineLabel();
        Label ll2 = il.DefineLabel();
        Label ll3 = il.DefineLabel();
        Label ll4 = il.DefineLabel();

        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ldstr, "1");
        il.Emit(OpCodes.Stloc_0);

        il.BeginExceptionBlock();
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ldstr, "2");
        il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Leave_S, ll1);

        il.BeginExceptFilterBlock();
        il.Emit(OpCodes.Isinst, typeof(Exception));
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Brtrue, ll2);

        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Br, ll3);

        il.MarkLabel(ll2);
        il.Emit(OpCodes.Stloc_1);
        il.Emit(OpCodes.Ldloc_1);
        il.Emit(OpCodes.Callvirt, typeof(Exception).GetMethod("get_Message"));
        il.Emit(OpCodes.Ldstr, "123");
        il.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
        il.Emit(OpCodes.Stloc_2);
        il.Emit(OpCodes.Ldloc_2);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Cgt_Un);

        il.MarkLabel(ll3);
        // il.Emit(OpCodes.Endfilter);

        //il.BeginFaultBlock();
        il.BeginCatchBlock(null);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ldstr, "3");
        il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Stloc_3);
        il.Emit(OpCodes.Leave_S, ll4);

        il.EndExceptionBlock();

        il.MarkLabel(ll1);
        il.Emit(OpCodes.Ldstr, "4");
        il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Stloc_3);
        il.Emit(OpCodes.Br_S, ll4);

        il.MarkLabel(ll4);
        il.Emit(OpCodes.Ldloc_3);
        il.Emit(OpCodes.Ret);
        
        var del = methodBuilder.CreateDelegate<Func<string>>();
        var s = del();
    }
}