using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination;

public abstract class TypeBuilder
{
	private protected TypeBuilder(TypeAttributes visibility) => this.visibility = visibility;

	internal String? name { get; private protected set; }
	internal TypeAttributes visibility { get; private protected set; }
	internal TypeRef? baseTypeRef { get; private protected set; }
	internal readonly List<TypeRef> interfaces = new();
	internal readonly List<TypeParameterBuilder> typeParameters = new();
	internal readonly List<NestedTypeBuilder> types = new();
}

public abstract class TypeBuilder<T> : TypeBuilder where T : TypeBuilder<T>
{
	private protected TypeBuilder(TypeAttributes visibility) : base(visibility) {}
	
	public T Name(String name) { this.name = name; return (T)this; }
	
	public T BaseType(Type type) { this.baseTypeRef = type; return (T)this; }
	public T BaseType<TBase>() => this.BaseType(typeof(TBase));
	public T BaseType(TypeBuilder typeBuilder) { this.baseTypeRef = typeBuilder; return (T)this; }
	
	public T AddInterface(Type type) { this.interfaces.Add(type); return (T)this; }
	public T AddInterface<TInterface>() => this.AddInterface(typeof(TInterface));
	public T AddInterface(TypeBuilder typeBuilder) { this.interfaces.Add(typeBuilder); return (T)this; }
	
	public T NewTypeParameter(out TypeParameterBuilder typeParameterBuilder, Action<TypeParameterBuilder> builderAction) => (T)this.AddAction(typeParameters, typeParameterBuilder = new(), builderAction);
	public T NewTypeParameter(out TypeParameterBuilder typeParameterBuilder) => NewTypeParameter(out typeParameterBuilder, _ => {});
	public T NewTypeParameter(Action<TypeParameterBuilder> builderAction) => NewTypeParameter(out _, builderAction);
	
	public T NewType(out NestedTypeBuilder nestedTypeBuilder) => (T)this.AddTo(types, nestedTypeBuilder = new());
	public T NewType(Action<NestedTypeBuilder> builderAction) => (T)this.AddAction(types, new(), builderAction);
}

public sealed class GlobalTypeBuilder : TypeBuilder<GlobalTypeBuilder>
{
	internal GlobalTypeBuilder() : base(TypeAttributes.NotPublic) {}

	public GlobalTypeBuilder Assembly() { this.visibility = TypeAttributes.NotPublic; return this; }
	public GlobalTypeBuilder Public() { this.visibility = TypeAttributes.Public; return this; }
}

public sealed class NestedTypeBuilder : TypeBuilder<NestedTypeBuilder>
{
	internal NestedTypeBuilder() : base(TypeAttributes.NestedPrivate) {}

	public NestedTypeBuilder Private() { this.visibility = TypeAttributes.NestedPrivate; return this; }
	public NestedTypeBuilder Family() { this.visibility = TypeAttributes.NestedFamily; return this; }
	public NestedTypeBuilder FamilyAndAssembly() { this.visibility = TypeAttributes.NestedFamANDAssem; return this; }
	public NestedTypeBuilder FamilyOrAssembly() { this.visibility = TypeAttributes.NestedFamORAssem; return this; }
	public NestedTypeBuilder Assembly() { this.visibility = TypeAttributes.NestedAssembly; return this; }
	public NestedTypeBuilder Public() { this.visibility = TypeAttributes.NestedPublic; return this; }
}