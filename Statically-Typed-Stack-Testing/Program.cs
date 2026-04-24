using InlineIL;
using static InlineIL.IL.Emit;

ref int r = ref Unsafe2.AsRef(out int i);
Console.WriteLine(r);
r = 43;
Console.WriteLine(i);

Body b = default;

;

struct Body;
struct Body<T> where T : I;

struct Loc<T> : I;

record struct Pair<T1, T2>(T1 _1, T2 _2) : I
    where T1 : struct, I
    where T2 : struct, I;

interface I;

static class Extensions
{
    public static Pair<T1, T2> Push<T1, T2>(this T1 x, T2 value)
        where T1 : struct, I
        where T2 : struct, I
    {
        return new(x, value);
    }
    
    public static T1 Pop<T1, T2>(this Pair<T1, T2> pair)
        where T1 : struct, I
        where T2 : struct, I
    {
        return pair._1;
    }
    
    public static Body<T> DeclareLocal<T>(this Body body, out Local<T> local)
        where T : I
    {
        //body.DeclareLocal(typeof(T), out var lb);
        local = new Local<T>();
        return new Body<T>();
    }
}

internal class Local<T>
{
}

public static class Unsafe2
{
    public static ref T AsRef<T>(out T value)
    {
        Ldarg(nameof(value));
        Ret();
        throw IL.Unreachable();
    }
}

// static class Extensions
// {
//     public static Pair<T1, T2> Pop<T1, T2, T3>(ref this Pair<T1, Pair<T2, T3>> locals)
//         where T1 : I
//         where T2 : I
//         where T3 : I
//     {
//         return locals.Pop();
//     }
//     
//     public static Loc<T1> Pop<T1, T2>(ref this Pair<T1, T2> locals)
//         where T1 : I
//         where T2 : I
//     {
//         return default;
//     }
// }