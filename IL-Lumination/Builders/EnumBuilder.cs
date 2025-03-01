using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Illumination;

public abstract class EnumBuilder
{
	private protected EnumBuilder(TypeAttributes visibility) => this.visibility = visibility;

	internal String? name { get; private protected set; }
	internal TypeAttributes visibility { get; private protected set; }
	internal TypeRef underlyingTypeRef { get; private protected set; } = typeof(Int32);
	internal readonly List<EnumLiteralBuilderBase> literals = new();
	
	private protected static readonly Type[] validUnderlyingTypes = [typeof(Byte), typeof(SByte), typeof(UInt16), typeof(Int16), typeof(UInt32), typeof(Int32), typeof(UInt64), typeof(Int64)];
}

public abstract class EnumBuilder<T> : EnumBuilder where T : EnumBuilder<T>
{
	private protected EnumBuilder(TypeAttributes visibility) : base(visibility) {}
	
	public T Name(String name) { this.name = name; return (T)this; }

	public T UnderlyingType(Type underlyingType)
	{
		if (Array.IndexOf(validUnderlyingTypes, underlyingType) < 0)
			throw new ArgumentException($"Invalid underlying type `{underlyingType.Name}`. Must be one of `{String.Join("`, `", validUnderlyingTypes.Select(x => x.Name))}`.", nameof(underlyingType));
		this.underlyingTypeRef = underlyingType; return (T)this;
	}
	public T UnderlyingType<TUnderlying>() where TUnderlying : unmanaged, IBinaryInteger<TUnderlying> => this.UnderlyingType(typeof(TUnderlying));
	public T UnderlyingType(TypeBuilder underlyingTypeBuilder) { this.underlyingTypeRef = underlyingTypeBuilder; return (T)this; }

	public T NewLiteral(out EnumLiteralBuilder enumLiteralBuilder, Action<EnumLiteralBuilder> action) =>
		(T)this.AddAction(literals.AsContravariant(), enumLiteralBuilder = new(), action);
	public T NewLiteral(out EnumLiteralBuilder enumLiteralBuilder) => NewLiteral(out enumLiteralBuilder, x => {});
	public T NewLiteral(Action<EnumLiteralBuilder> action) => NewLiteral(out _, action);
}

public abstract class EnumBuilder<T, TUnderlying> : EnumBuilder where T : EnumBuilder<T, TUnderlying> where TUnderlying : struct, IBinaryInteger<TUnderlying>
{
	private protected EnumBuilder(TypeAttributes visibility) : base(visibility) {}
	
	public T Name(String name) { this.name = name; return (T)this; }

	public T NewLiteral(out EnumLiteralBuilder<TUnderlying> enumLiteralBuilder, Action<EnumLiteralBuilder<TUnderlying>> action) =>
		(T)this.AddAction(literals.AsContravariant(), enumLiteralBuilder = new EnumLiteralBuilder<TUnderlying>(), action);
	public T NewLiteral(out EnumLiteralBuilder<TUnderlying> enumLiteralBuilder) => NewLiteral(out enumLiteralBuilder, x => {});
	public T NewLiteral(Action<EnumLiteralBuilder<TUnderlying>> action) => NewLiteral(out _, action);
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