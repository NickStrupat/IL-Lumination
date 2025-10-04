using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using NickStrupat;

namespace Illumination.Builders;

public abstract class EnumBuilder : IBuilder<System.Reflection.Emit.TypeBuilder>
{
	private protected EnumBuilder(TypeAttributes visibility, Type underlyingType)
	{
		this.visibility = visibility;
		this.underlyingType = underlyingType;
	}

	internal String? name { get; private protected set; }
	internal TypeAttributes visibility { get; private protected set; }
	internal Type underlyingType { get; private protected set; }
	internal readonly HashSet<EnumLiteralBuilderBase> literals = new(EqCmp<EnumLiteralBuilderBase>.Create(x => x.name));
	
	private protected static class UnderlyingTypes
	{
		private static readonly FrozenSet<Type> validUnderlyingTypes =
		[
			typeof(Byte),
			typeof(SByte),
			typeof(UInt16),
			typeof(Int16),
			typeof(UInt32),
			typeof(Int32),
			typeof(UInt64),
			typeof(Int64)
		];
		
		public static Type Validate(Type type)
		{
			if (validUnderlyingTypes.Contains(type))
				return type;
			var typeNamesCsv = String.Join("`, `", validUnderlyingTypes.Select(x => x.Name));
			var message = $"Invalid underlying type `{type.Name}`. Must be one of `{typeNamesCsv}`.";
			throw new ArgumentException(message, nameof(underlyingType));
		}
	}
}

public abstract class EnumBuilder<T> : EnumBuilder where T : EnumBuilder<T>
{
	private protected EnumBuilder(TypeAttributes visibility) : base(visibility, typeof(Int32)) {}
	
	public T Name(String name) { this.name = name; return (T)this; }

	public T UnderlyingType(Type underlyingType) { this.underlyingType = UnderlyingTypes.Validate(underlyingType); return (T)this; }
	public T UnderlyingType<TUnderlying>() where TUnderlying : unmanaged, IBinaryInteger<TUnderlying> => this.UnderlyingType(typeof(TUnderlying));

	public T NewLiteral(out EnumLiteralBuilder enumLiteralBuilder, Action<EnumLiteralBuilder> action) =>
		(T)this.AddAction(literals.AsContravariant(), enumLiteralBuilder = new(), action);
}

public abstract class EnumBuilder<T, TUnderlying> : EnumBuilder where T : EnumBuilder<T, TUnderlying> where TUnderlying : struct, IBinaryInteger<TUnderlying>
{
	private protected EnumBuilder(TypeAttributes visibility) : base(visibility, typeof(TUnderlying)) {}
	
	public T Name(String name) { this.name = name; return (T)this; }

	public T NewLiteral(out EnumLiteralBuilder<TUnderlying> enumLiteralBuilder, Action<EnumLiteralBuilder<TUnderlying>> action) =>
		(T)this.AddAction(literals.AsContravariant(), enumLiteralBuilder = new EnumLiteralBuilder<TUnderlying>(), action);
}

public sealed class GlobalEnumBuilder : EnumBuilder<GlobalEnumBuilder>
{
	internal GlobalEnumBuilder() : base(TypeAttributes.NotPublic) {}

	public GlobalEnumBuilder Assembly() { this.visibility = TypeAttributes.NotPublic; return this; }
	public GlobalEnumBuilder Public() { this.visibility = TypeAttributes.Public; return this; }
}

public sealed class GlobalEnumBuilder<TUnderlying> : EnumBuilder<GlobalEnumBuilder<TUnderlying>, TUnderlying>
	where TUnderlying : struct, IBinaryInteger<TUnderlying>
{
	internal GlobalEnumBuilder() : base(TypeAttributes.NotPublic) {}

	public GlobalEnumBuilder<TUnderlying> Assembly() { this.visibility = TypeAttributes.NotPublic; return this; }
	public GlobalEnumBuilder<TUnderlying> Public() { this.visibility = TypeAttributes.Public; return this; }
}

public sealed class NestedEnumBuilder : EnumBuilder<NestedEnumBuilder>
{
	internal NestedEnumBuilder() : base(TypeAttributes.NestedPrivate) {}

	public NestedEnumBuilder Private() { this.visibility = TypeAttributes.NestedPrivate; return this; }
	public NestedEnumBuilder Family() { this.visibility = TypeAttributes.NestedFamily; return this; }
	public NestedEnumBuilder FamilyAndAssembly() { this.visibility = TypeAttributes.NestedFamANDAssem; return this; }
	public NestedEnumBuilder FamilyOrAssembly() { this.visibility = TypeAttributes.NestedFamORAssem; return this; }
	public NestedEnumBuilder Assembly() { this.visibility = TypeAttributes.NestedAssembly; return this; }
	public NestedEnumBuilder Public() { this.visibility = TypeAttributes.NestedPublic; return this; }
}

public static class EnumBuilderExtensions
{
	public static T NewLiteral<T>(this EnumBuilder<T> eb, out EnumLiteralBuilder enumLiteralBuilder) where T : EnumBuilder<T> =>
		eb.NewLiteral(out enumLiteralBuilder, _ => {});
	public static T NewLiteral<T>(this EnumBuilder<T> eb, Action<EnumLiteralBuilder> action) where T : EnumBuilder<T> =>
		eb.NewLiteral(out _, action);

	public static T NewLiteral<T, TUnderlying>(this EnumBuilder<T, TUnderlying> eb, out EnumLiteralBuilder<TUnderlying> enumLiteralBuilder)
		where T : EnumBuilder<T, TUnderlying>
		where TUnderlying : struct, IBinaryInteger<TUnderlying> =>
		eb.NewLiteral(out enumLiteralBuilder, _ => {});
	public static T NewLiteral<T, TUnderlying>(this EnumBuilder<T, TUnderlying> eb, Action<EnumLiteralBuilder<TUnderlying>> action)
		where T : EnumBuilder<T, TUnderlying>
		where TUnderlying : struct, IBinaryInteger<TUnderlying> =>
		eb.NewLiteral(out _, action);
	public static T NewLiteral<T, TUnderlying>(this EnumBuilder<T, TUnderlying> eb, String name, TUnderlying value)
		where T : EnumBuilder<T, TUnderlying>
		where TUnderlying : struct, IBinaryInteger<TUnderlying> =>
		eb.NewLiteral(out _, x => x.Name(name).Value(value));
}