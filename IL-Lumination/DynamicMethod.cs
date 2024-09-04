using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace IL_Lumination;

public class DynamicMethod<TDel> where TDel : Delegate
{
    public DynamicMethod Method { get; } = new(String.Empty, ReturnType, ParameterTypes);
    
    /// <inheritdoc cref="DynamicMethod.GetILGenerator()"/>
    public ILGenerator GetILGenerator() => Method.GetILGenerator();

    /// <summary><inheritdoc cref="DynamicMethod.CreateDelegate(Type)"/></summary>
    public TDel CreateDelegate() => Method.CreateDelegate<TDel>();
    
    private static readonly MethodInfo InvokeMethod = typeof(TDel).GetMethod("Invoke")!;
    private static readonly Type ReturnType = InvokeMethod.ReturnType;
    private static readonly Type[] ParameterTypes = InvokeMethod.GetParameters().Select(x => x.ParameterType).ToArray();
}

public sealed class DynamicAction : DynamicMethod<Action>;
public sealed class DynamicAction<T0> : DynamicMethod<Action<T0>>;
public sealed class DynamicAction<T0, T1> : DynamicMethod<Action<T0, T1>>;
public sealed class DynamicAction<T0, T1, T2> : DynamicMethod<Action<T0, T1, T2>>;
public sealed class DynamicAction<T0, T1, T2, T3> : DynamicMethod<Action<T0, T1, T2, T3>>;
public sealed class DynamicAction<T0, T1, T2, T3, T4> : DynamicMethod<Action<T0, T1, T2, T3, T4>>;
public sealed class DynamicAction<T0, T1, T2, T3, T4, T5> : DynamicMethod<Action<T0, T1, T2, T3, T4, T5>>;

public sealed class DynamicFunc<R> : DynamicMethod<Func<R>>;
public sealed class DynamicFunc<T0, R> : DynamicMethod<Func<T0, R>>;
public sealed class DynamicFunc<T0, T1, R> : DynamicMethod<Func<T0, T1, R>>;
public sealed class DynamicFunc<T0, T1, T2, R> : DynamicMethod<Func<T0, T1, T2, R>>;
public sealed class DynamicFunc<T0, T1, T2, T3, R> : DynamicMethod<Func<T0, T1, T2, T3, R>>;
public sealed class DynamicFunc<T0, T1, T2, T3, T4, R> : DynamicMethod<Func<T0, T1, T2, T3, T4, R>>;
public sealed class DynamicFunc<T0, T1, T2, T3, T4, T5, R> : DynamicMethod<Func<T0, T1, T2, T3, T4, T5, R>>;

// public abstract class DynamicMethodBase<TDel> where TDel : Delegate
// {
//     private protected DynamicMethodBase() {}
//     
//     private readonly DynamicMethod dm = new(String.Empty, ReturnType, ParameterTypes);
//     
//     /// <inheritdoc cref="DynamicMethod.GetILGenerator()"/>
//     public ILGenerator GetILGenerator() => dm.GetILGenerator();
//     
//     /// <inheritdoc cref="DynamicMethod.GetILGenerator(System.Int32)"/>
//     public ILGenerator GetILGenerator(Int32 streamSize) => dm.GetILGenerator(streamSize);
//
//     /// <summary><inheritdoc cref="DynamicMethod.CreateDelegate(Type)"/></summary>
//     public TDel CreateDelegate() => dm.CreateDelegate<TDel>();
//     
//     private static readonly MethodInfo InvokeMethod = typeof(TDel).GetMethod("Invoke")!;
//     private static readonly Type ReturnType = InvokeMethod.ReturnType;
//     private static readonly Type[] ParameterTypes = InvokeMethod.GetParameters().Select(x => x.ParameterType).ToArray();
// }