using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination;

public abstract class EnumBuilder
{
	private protected EnumBuilder(TypeAttributes visibility) => this.visibility = visibility;

	internal String? name { get; private protected set; }
	internal TypeAttributes visibility { get; private protected set; }
	internal TypeRef underlyingTypeRef { get; private protected set; } = typeof(Int32);
	internal readonly List<EnumLiteralBuilder> literals = new();
}

public abstract class EnumBuilder<T> : EnumBuilder where T : EnumBuilder<T>
{
	private protected EnumBuilder(TypeAttributes visibility) : base(visibility) {}
	
	public T Name(String name) { this.name = name; return (T)this; }
	
	public T UnderlyingType(Type underlyingType) { this.underlyingTypeRef = underlyingType; return (T)this; }
	public T UnderlyingType<TUnderlying>() => this.UnderlyingType(typeof(TUnderlying));
	public T UnderlyingType(TypeBuilder underlyingTypeBuilder) { this.underlyingTypeRef = underlyingTypeBuilder; return (T)this; }

	public T NewLiteral(out EnumLiteralBuilder enumLiteralBuilder, Action<EnumLiteralBuilder> action) => (T)this.AddAction(literals, enumLiteralBuilder = new(), action);
	public T NewLiteral(out EnumLiteralBuilder enumLiteralBuilder) => NewLiteral(out enumLiteralBuilder, x => {});
	public T NewLiteral(Action<EnumLiteralBuilder> action) => NewLiteral(out _, action);
}

public sealed class GlobalEnumBuilder : EnumBuilder<GlobalEnumBuilder>
{
	internal GlobalEnumBuilder() : base(TypeAttributes.NotPublic) {}

	public GlobalEnumBuilder Assembly() { this.visibility = TypeAttributes.NotPublic; return this; }
	public GlobalEnumBuilder Public() { this.visibility = TypeAttributes.Public; return this; }
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

public sealed class EnumLiteralBuilder
{
	internal EnumLiteralBuilder() {}
	
	internal String? name { get; private set; }
	public EnumLiteralBuilder Name(String name) { this.name = name; return this; }

	internal Object? value { get; private set; }
	public EnumLiteralBuilder Value(Object value) { this.value = value; return this; }
}