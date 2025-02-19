using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination;

public abstract class TypeBuilder
{
	private protected TypeBuilder() {}

	internal String? name;
	internal TypeAttributes visibility;
	internal readonly List<NestedTypeBuilder> types = new();
}

public abstract class TypeBuilder<T> : TypeBuilder where T : TypeBuilder<T>
{
	private protected TypeBuilder() {}
	
	public T Name(String name) { this.name = name; return (T)this; }
	
	public T NewType(out NestedTypeBuilder nestedTypeBuilder) => (T)this.AddTo(types, nestedTypeBuilder = new());
	public T NewType(Action<NestedTypeBuilder> action) => (T)this.AddAction(types, new(), action);
}

public sealed class GlobalTypeBuilder : TypeBuilder<GlobalTypeBuilder>
{
	internal GlobalTypeBuilder() => visibility = TypeAttributes.NotPublic;

	public GlobalTypeBuilder Internal() { this.visibility = TypeAttributes.NotPublic; return this; }
	public GlobalTypeBuilder Public() { this.visibility = TypeAttributes.Public; return this; }
}

public sealed class NestedTypeBuilder : TypeBuilder<NestedTypeBuilder>
{
	internal NestedTypeBuilder() => visibility = TypeAttributes.NestedPrivate;

	public NestedTypeBuilder Private() { this.visibility = TypeAttributes.NestedPrivate; return this; }
	public NestedTypeBuilder Public() { this.visibility = TypeAttributes.NestedPublic; return this; }
	public NestedTypeBuilder Family() { this.visibility = TypeAttributes.NestedFamily; return this; }
	public NestedTypeBuilder FamilyAndAssembly() { this.visibility = TypeAttributes.NestedFamANDAssem; return this; }
	public NestedTypeBuilder FamilyOrAssembly() { this.visibility = TypeAttributes.NestedFamORAssem; return this; }
}