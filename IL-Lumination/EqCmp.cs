using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Illumination;

// public delegate ref TR FuncRef<T, TR>(T arg) where T : struct where TR : class;
// public delegate ref TR RefFuncRef<T, TR>(ref T arg) where T : struct where TR : struct;
// public delegate TR RefFunc<T, TR>(ref T arg) where T : struct where TR : class;

class BarBar : IEquatable<BarBar>
{
	public required String Bar { get; set; }
	public Int32 Baz { get; set; }
	public required String bar;
	public Int32 baz;

	public Boolean Equals(BarBar? other)
	{
		if (other is null)
			return false;
		if (ReferenceEquals(this, other))
			return true;
		
		//return Bar == other.Bar && Baz == other.Baz && bar == other.bar && baz == other.baz;

		return EqualityComparer<String>.Default.Equals(Bar, other.Bar)
		       && EqualityComparer<Int32>.Default.Equals(Baz, other.Baz)
		       && EqualityComparer<String>.Default.Equals(bar, other.bar)
		       && EqualityComparer<Int32>.Default.Equals(baz, other.baz)
			;
	}

	public override Int32 GetHashCode()
	{
		var hc = new HashCode();
		hc.Add(Bar);
		hc.Add(Baz);
		hc.Add(bar);
		hc.Add(baz);
		return hc.ToHashCode();
	}
	
	
}

class Foo : Equatable<Foo>, IEq<Foo>
{
	//protected override IEnumerable<Object> EqualityMembers => [Bar, Baz];

	public required String Bar { get; set; }
	public Int32 Baz { get; set; }

	//protected override EqMembers EqualityMembers(EqM x) => x([Bar, Baz]);
	static Expression<Members<Foo>> IEq<Foo>.EqualityMembers { get; } = x => new { x.Bar, x.Baz };
}

public abstract class Equatable<T> : IEquatable<T> where T : Equatable<T>, IEq<T>
{
	//protected abstract EqMembers EqualityMembers(EqM x);

	// Boolean IEqualityComparer<T>.Equals(T? x, T? y) => (x, y) switch
	// {
	// 	(null, null) => true,
	// 	(null, _) => false,
	// 	(_, null) => false,
	// 	(_, _) => EqualityMembers.All(member => member(x).Equals(member(y)))
	// };
	
	private static readonly Func<T, Int32> GetHashCodeFunc;
	private static readonly Func<T, T, Boolean> EqualsFunc;
	static Equatable() => (GetHashCodeFunc, EqualsFunc) = CreateEqualityDelegates();

	private readonly T derivedThis;
	protected Equatable() => derivedThis = Unsafe.As<T>(this);

	public sealed override Int32 GetHashCode() => GetHashCodeFunc(derivedThis);
	public sealed override Boolean Equals(Object? obj) => obj is T other && Equals(other);

	public Boolean Equals(T? other) => other is not null && EqualsFunc(derivedThis, other);
	// {
	// 	if (ReferenceEquals(this, other))
	// 		return true;
	// 	if (other is null)
	// 		return false;
	// 	var members = EqualityMembers((T)this);
	// 	var otherMembers = EqualityMembers(other);
	// 	if (members.Length != otherMembers.Length)
	// 		return false;
	// 	for (Int32 i = 0; i < members.Length; i++)
	// 		if (!members[i].Equals(otherMembers[i]))
	// 			return false;
	// 	return true;
	// }
	
	private static (Func<T, Int32> getHashCode, Func<T, T, Boolean> equals) CreateEqualityDelegates()
	{
		if (T.EqualityMembers.Body is not NewExpression ne)
			throw new("Expected a new expression.");
		var memberInfos = new List<MemberInfo>();
		foreach (var arg in ne.Arguments)
		{
			if (arg is not MemberExpression { Member: {} mi })
				throw new("Expected a member expression.");
			if (!typeof(T).IsAssignableTo(mi.DeclaringType))
				throw new("Expected a member of the specified type: " + typeof(T).FullName);
			memberInfos.Add(mi);
		}
		
		DynamicMethod<Func<T, Int32>> ghc = new();
		var ghcBody = ghc.Body();
		ghcBody
			.DeclareLocal<HashCode>(out var hc)
			.Ldloca_S(hc)
			.Initobj<HashCode>();
		var hcAdd = typeof(HashCode).GetMethod(nameof(HashCode.Add), [Type.MakeGenericMethodParameter(0)])!;
		foreach (var memberInfo in memberInfos)
		{
			ghcBody.Ldloca_S(hc).Ldarg_0();
			_ = memberInfo switch
			{
				FieldInfo fi => ghcBody
					.Ldfld(fi)
					.Call(hcAdd.MakeGenericMethod(fi.FieldType)),
				PropertyInfo { GetMethod: {} gm } pi => ghcBody
					.Call(gm)
					.Call(hcAdd.MakeGenericMethod(pi.PropertyType)),
				_ => throw new("Expected a field or property.")
			};
		}

		ghcBody
			.Ldloca_S(hc)
			.Call(typeof(HashCode).GetMethod(nameof(HashCode.ToHashCode))!)
			.Ret();
		
		DynamicFunc<T, T, Boolean> eq = new();
		var eqBody = eq.Body();
		eqBody.DefineLabel(out var otherNotNull).Ldarg_1().Brtrue_S(otherNotNull);
		eqBody.Ldc_I4_0().Ret();
		eqBody.MarkLabel(otherNotNull).Ldarg_0().Ldarg_1().DefineLabel(out var notRefEq).Bne_Un_S(notRefEq);
		eqBody.Ldc_I4_1().Ret();
		eqBody.MarkLabel(notRefEq).DefineLabel(out var notEq);
		foreach (var memberInfo in memberInfos)
		{
			_ = memberInfo switch
			{
				FieldInfo fi => Field(eqBody, fi),
				PropertyInfo pi => Property(eqBody, pi),
				_ => throw new("Expected a field or property.")
			};
			eqBody.Brfalse_S(notEq);
		}

		eqBody.Ldc_I4_1().Ret().MarkLabel(notEq).Ldc_I4_0().Ret();
		
		return (ghc.CreateDelegate(), eq.CreateDelegate());

		static Body Field(Body body, FieldInfo fieldInfo)
		{
			var (defaultInstanceGetter, equals) = GetEqualityComparerDefault(fieldInfo.FieldType);
			return body.Call(defaultInstanceGetter).Ldarg_0().Ldfld(fieldInfo).Ldarg_1().Ldfld(fieldInfo).Callvirt(equals);
		}
		
		static Body Property(Body body, PropertyInfo propertyInfo)
		{
			var (defaultInstanceGetter, equals) = GetEqualityComparerDefault(propertyInfo.PropertyType);
			var mi = propertyInfo.GetMethod!;
			return body.Call(defaultInstanceGetter).Ldarg_0().Call(mi).Ldarg_1().Callvirt(mi).Callvirt(equals);
		}
		
		static (MethodInfo defaultInstanceGetter, MethodInfo equals) GetEqualityComparerDefault(Type type)
		{
			var ec = typeof(EqualityComparer<>).MakeGenericType(type);
			return (ec.GetProperty("Default")!.GetMethod!, ec.GetMethod("Equals", [type, type])!);
		}
		// DynamicMethod dm = new(String.Empty, typeof(T).MakeByRefType(), [typeof(T).MakeByRefType()]);
		// var il = dm.GetILGenerator();
		// il.Emit(OpCodes.Ldarg_0);
		// foreach (var memberInfo in memberInfos)
		// 	switch (memberInfo)
		// 	{
		// 		case FieldInfo field:
		// 			il.Emit(OpCodes.Ldind_Ref);
		// 			il.Emit(OpCodes.Ldflda, field);
		// 			break;
		// 		case PropertyInfo property:
		// 			il.Emit(OpCodes.Call, property.GetMethod);
		// 			break;
		// 		default:
		// 			throw new("Expected a field or property.");
		// 	}
		//
		// il.Emit(OpCodes.Ret);
		// var func = dm.CreateDelegate<RefFuncRef<T>>();
		// return func;
	}
}

public interface IEq<T>
{
	//static abstract void EqualityMembers(EzEquatableAction x);
	//static abstract EqMembers EqualityMembers(EqM eqm);
	static abstract Expression<Members<T>> EqualityMembers { get; }
}

public delegate Object Members<T>(T obj);

public sealed class EqMembers
{
	internal EqMembers() {}
}
public delegate EqMembers EqM(ReadOnlySpan<Object> members);

// public sealed class EqCmp<T>(Func<T, T, Boolean> equals, Func<T, Int32> getHashCode) : IEqualityComparer<T> where T : notnull
// {
// 	public Boolean Equals(T? x, T? y)
// 	{
// 		if (typeof(T).IsValueType)
// 			return equals(x!, y!);
// 		if (ReferenceEquals(x, y))
// 			return true;
// 		if (x is null || y is null)
// 			return false;
// 		return equals(x, y);
// 	}
//
// 	public Int32 GetHashCode(T obj) => getHashCode(obj);
// 	
// 	// public static EqCmp<T> Create<TP>(RefFuncRef<T, TP> prop) where TP : notnull => new(
// 	// 	(x, y) => prop.Invoke(ref x).Equals(prop(ref y)),
// 	// 	x => HashCode.Combine(prop(ref x))
// 	// );
// 	
// 	// public static EqCmp<T> Create<TP>(RefFunc<T, TP> prop) where TP : notnull => new(
// 	// 	(x, y) => prop.Invoke(ref x).Equals(prop(ref y)),
// 	// 	x => HashCode.Combine(prop(ref x))
// 	// );
//
// 	public static EqCmp<T> Create<TP>(Func<T, TP> prop) => new(
// 		(x, y) => Eq(prop, x, y),
// 		x => HashCode.Combine(prop(x))
// 	);
//
// 	public static EqCmp<T> Create<TP1, TP2>(Func<T, TP1> prop1, Func<T, TP2> prop2) => new(
// 		(x, y) => Eq(prop1, x, y) && Eq(prop2, x, y),
// 		x => HashCode.Combine(prop1(x), prop2(x))
// 	);
// 	
// 	public static EqCmp<T> Create<TP1, TP2, TP3>(Func<T, TP1> prop1, Func<T, TP2> prop2, Func<T, TP3> prop3) => new(
// 		(x, y) => Eq(prop1, x, y) && Eq(prop2, x, y) && Eq(prop3, x, y),
// 		x => HashCode.Combine(prop1(x), prop2(x), prop3(x))
// 	);
// 	
// 	public static EqCmp<T> Create<TP1, TP2, TP3, TP4>(Func<T, TP1> prop1, Func<T, TP2> prop2, Func<T, TP3> prop3, Func<T, TP4> prop4) => new(
// 		(x, y) => Eq(prop1, x, y) && Eq(prop2, x, y) && Eq(prop3, x, y) && Eq(prop4, x, y),
// 		x => HashCode.Combine(prop1(x), prop2(x), prop3(x), prop4(x))
// 	);
// 	
// 	private static Boolean Eq<TP>(Func<T, TP> prop, T x, T y) => (prop(x), prop(y)) switch
// 	{
// 		(null, null) => true,
// 		(null, _) or (_, null) => false,
// 		(IEquatable<TP> eq, var other) => eq.Equals(other),
// 		(var other, IEquatable<TP> eq) => eq.Equals(other),
// 		_ => x.Equals(y)
// 	};
// 	
// 	private static Boolean Equals<TP>(TP? x, TP? y) => (x, y) switch
// 	{
// 		(null, null) => true,
// 		(null, _) or (_, null) => false,
// 		(IEquatable<TP> eq, var _y) => eq.Equals(_y),
// 		(var _x, IEquatable<TP> eq) => eq.Equals(_x),
// 		_ => x.Equals(y)
// 	};
// }

// public static class EqCmp
// {
// 	public static EqCmpBuilder<T> For<T>() => new();
//
// 	public static EqCmp<T> Create<T, TP>(this EqCmpBuilder<T> builder, Func<T, TP> prop) where T : notnull => EqCmp<T>.Create(prop);
// 	public static EqCmp<T> Create<T, TP>(this EqCmpBuilder<T> builder, RefFunc<T, TP> prop) where T : struct where TP : class => new(
// 		(x, y) => prop.Invoke(ref x).Equals(prop(ref y)),
// 		x => HashCode.Combine(prop(ref x))
// 	);
// }
//
// public ref struct EqCmpBuilder<T>;