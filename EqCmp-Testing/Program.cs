using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Illumination;

Expression<Func<Bar, Object?>> expr = x => x.Baz;
var prop = expr.GetPropOrFieldGetter();

var dm = new DynamicMethod<Woohoo>();
dm.Body().Ret();
int x = 3;
dm.CreateDelegate()(ref x);
return;
var asdfasdf = new[] { 1, 2, 3 }.Select(x => x + 1);
var b = asdfasdf.TryGetNonEnumeratedCount(out var count);
asdfasdf.Count();
What<Bar>.Okay(x => new { x.Baz, x.Bong, x.BaseProp });

var barbar = new Bar { Baz = "adsf" };
What<Bar>.Zoo(x => x.Baz, x => barbar.Bong);
var func = What<Bar>.Foo(x => x.Baz);
var bar = new Bar { Baz = "adsf" };
bar.BazP = "asasdfdf";
ref readonly var asdf = ref Woo(ref bar);
ref String baz = ref func(ref bar);
;

static ref readonly string Woo(ref Bar bar) => ref bar.Baz;
static ref readonly string Woo2(ref Bar bar) => ref bar.BazP;

delegate void Woohoo(ref int x);

class Bar : BarBase
{
    public required string Baz;// { get; set; }
    public ref string BazP => ref Baz;
    public int Bong { get; set; }
}

class BarBase
{
    public string BaseProp { get; set; }
}

static class What<T>
{
    public static void Zoo(params Expression<Func<T, Object?>>[] exprs)
    {
        foreach (var expr in exprs)
        {
            var memberInfo = expr.Body switch
            {
                UnaryExpression { Operand: MemberExpression { Member: {} mi }} => mi,
                MemberExpression { Member: {} mi } => mi,
                _ => throw new("Expected a member expression.")
            };
            if (memberInfo.DeclaringType != typeof(T))
                throw new("Expected a member of the specified type.");
            
            DynamicMethod dm = new(String.Empty, typeof(T).MakeByRefType(), [typeof(T).MakeByRefType()]);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
        }
    }
    
    public static RefFuncRef<TMember> Foo<TMember>(Expression<Func<T, TMember>> memberExpression)
    {
        if (memberExpression.Body is not MemberExpression body)
            throw new("Expected a member expression.");
        
        // create delegate that returns the member by reference
        DynamicMethod dm = new(String.Empty, typeof(TMember).MakeByRefType(), [typeof(T).MakeByRefType()]);
        var il = dm.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        if (body.Member is FieldInfo field)
        {
            il.Emit(OpCodes.Ldind_Ref);
            il.Emit(OpCodes.Ldflda, field);
        }
        else if (body.Member is PropertyInfo property)
            il.Emit(OpCodes.Call, property.GetGetMethod()!);
        else
            throw new("Expected a field or property.");
        il.Emit(OpCodes.Ret);
        var func = dm.CreateDelegate<RefFuncRef<TMember>>();
        return func;
    }
    
    public delegate ref TMember RefFuncRef<TMember>(ref T member);



    public static RefFuncRef<T> Okay(Expression<Func<T, Object>> membersExpression)
    {
        if (membersExpression.Body is not NewExpression body)
            throw new("Expected a new expression.");
        var memberInfos = new List<MemberInfo>();
        foreach (var arg in body.Arguments)
        {
            if (arg is not MemberExpression { Member: {} mi })
                throw new("Expected a member expression.");
            if (!typeof(T).IsAssignableTo(mi.DeclaringType))
                throw new("Expected a member of the specified type: " + typeof(T).FullName);
            memberInfos.Add(mi);
        }
        DynamicMethod dm = new(String.Empty, typeof(T).MakeByRefType(), [typeof(T).MakeByRefType()]);
        var il = dm.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        foreach (var memberInfo in memberInfos)
            switch (memberInfo)
            {
                case FieldInfo field:
                    il.Emit(OpCodes.Ldind_Ref);
                    il.Emit(OpCodes.Ldflda, field);
                    break;
                case PropertyInfo property:
                    il.Emit(OpCodes.Call, property.GetMethod);
                    break;
                default:
                    throw new("Expected a field or property.");
            }

        il.Emit(OpCodes.Ret);
        var func = dm.CreateDelegate<RefFuncRef<T>>();
        return func;
    }
}