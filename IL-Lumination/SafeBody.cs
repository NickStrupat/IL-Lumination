using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Illumination;

public sealed class SafeBody(DynamicMethod dm)
{
    private readonly Body body = new(dm.GetILGenerator());
    private readonly TypeStack stack = new();
    
    private Type Type<T>() => typeof(T);
    private Type RefType<T>() => Type<T>().MakeByRefType();
    private Type PtrType<T>() => Type<T>().MakePointerType();

    public SafeBody Add()
    {
        var type1 = stack.Pop();
        var type2 = stack.Pop();
        var map = new Dictionary<(Type, Type), Type>
        {
            [(typeof(Int32), typeof(Int32))] = typeof(Int32),
            [(typeof(Int32), typeof(IntPtr))] = typeof(IntPtr),
            [(typeof(Int32), typeof(Int32).MakeByRefType())] = typeof(Int32).MakeByRefType(),
            [(typeof(Int32), typeof(Int32).MakePointerType())] = typeof(Int32).MakePointerType(),
            
            [(typeof(Int64), typeof(Int64))] = typeof(Int64),
            
            [(typeof(IntPtr), typeof(Int32))] = typeof(Int32),
            [(typeof(IntPtr), typeof(IntPtr))] = typeof(IntPtr),
            [(typeof(IntPtr), typeof(Int32).MakeByRefType())] = typeof(Int32).MakeByRefType(),
            [(typeof(IntPtr), typeof(Int32).MakePointerType())] = typeof(Int32).MakePointerType(),
            
            [(typeof(Single), typeof(Single))] = typeof(Single),
            [(typeof(Double), typeof(Double))] = typeof(Double),
            
            [(typeof(Int32).MakeByRefType(), typeof(Int32))] = typeof(Int32).MakeByRefType(),
            [(typeof(Int32).MakeByRefType(), typeof(IntPtr))] = typeof(Int32).MakeByRefType(),
            [(typeof(Int32).MakePointerType(), typeof(Int32))] = typeof(Int32).MakePointerType(),
            [(typeof(Int32).MakePointerType(), typeof(IntPtr))] = typeof(Int32).MakePointerType(),
        };
        if (!map.TryGetValue((type1, type2), out var type))
            throw new($"Types on the stack are incompatible with the {nameof(OpCodes.Add)} operation: `{type1}`, `{type2}`");
        stack.Push(type);
        body.Add();
        return this;
    }

    private sealed class TypeStack : Stack<Type>
    {
        public void Push<T>() => Push(typeof(T));
    }
}

public interface IBody
{
    IBody<T> LoadArg<T>(in Arg<T> arg);
}

public interface IBody<T> : IBody
{
    
}

public readonly struct Arg<T>
{
    private readonly Int32 index;
    internal Arg(Int32 index) => this.index = index + 1;
    internal Int32 Index => index == 0 ? throw new("Arg instance was not initialized.") : index - 1;
}