using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination.Builders;

public abstract class TypeBuilder : IHasTypesAndMethods
{
	private protected readonly AssemblyBuilder assemblyBuilder; 
	private protected TypeBuilder(AssemblyBuilder assemblyBuilder, TypeAttributes visibility)
	{
		this.assemblyBuilder = assemblyBuilder; 
		this.visibility = visibility;
	}

	internal String? name { get; private protected set; }
	internal TypeAttributes visibility { get; private protected set; }
	internal TypeRef? baseTypeRef { get; private protected set; }
	
	internal readonly List<TypeRef> interfaces = new();
	internal readonly List<TypeParameterBuilder> typeParameters = new();
	internal readonly List<FieldBuilder> fields = new();
	internal readonly List<PropertyBuilder> properties = new();
	internal readonly List<NestedTypeBuilder> types = new();
	internal readonly List<NestedEnumBuilder> enums = new();
	internal readonly List<NestedMethodBuilder> methods = new();

	IEnumerable<TypeBuilder> IHasTypesAndMethods.Types => types;
	IEnumerable<MethodBuilder> IHasTypesAndMethods.Methods => methods;
}

public abstract class TypeBuilder<T> : TypeBuilder where T : TypeBuilder<T>
{
	private protected TypeBuilder(AssemblyBuilder assemblyBuilder, TypeAttributes visibility) : base(assemblyBuilder, visibility) {}
	
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
	
	public T NewField(out FieldBuilder fieldBuilder, Action<FieldBuilder> action) => (T)this.AddAction(fields, fieldBuilder = new(), action);
	public T NewField(out FieldBuilder fieldBuilder) => NewField(out fieldBuilder, _ => {});
	public T NewField(Action<FieldBuilder> action) => NewField(out _, action);
	
	public T NewProperty(out PropertyBuilder propertyBuilder, Action<PropertyBuilder> action) => (T)this.AddAction(properties, propertyBuilder = new(), action);
	public T NewProperty(out PropertyBuilder propertyBuilder) => NewProperty(out propertyBuilder, _ => {});
	public T NewProperty(Action<PropertyBuilder> action) => NewProperty(out _, action);
	
	public T NewMethod(out NestedMethodBuilder nestedMethodBuilder, Action<NestedMethodBuilder> action) => (T)this.AddAction(methods, nestedMethodBuilder = new(), action);
	public T NewMethod(out NestedMethodBuilder nestedMethodBuilder) => NewMethod(out nestedMethodBuilder, _ => {});
	public T NewMethod(Action<NestedMethodBuilder> action) => NewMethod(out _, action);
	
	public T NewEnum(out NestedEnumBuilder nestedEnumBuilder, Action<NestedEnumBuilder> action) => (T)this.AddAction(enums, nestedEnumBuilder = new(), action);
	public T NewEnum(out NestedEnumBuilder nestedEnumBuilder) => NewEnum(out nestedEnumBuilder, _ => {});
	public T NewEnum(Action<NestedEnumBuilder> action) => NewEnum(out _, action);
	
	public T NewType(out NestedTypeBuilder nestedTypeBuilder, Action<NestedTypeBuilder> builderAction) => (T)this.AddAction(types, nestedTypeBuilder = new(assemblyBuilder), builderAction);
	public T NewType(out NestedTypeBuilder nestedTypeBuilder) => NewType(out nestedTypeBuilder, _ => {});
	public T NewType(Action<NestedTypeBuilder> builderAction) => NewType(out _, builderAction);
}

public sealed class GlobalTypeBuilder : TypeBuilder<GlobalTypeBuilder>
{
	internal GlobalTypeBuilder(AssemblyBuilder assemblyBuilder) : base(assemblyBuilder, TypeAttributes.NotPublic) {}

	public GlobalTypeBuilder Assembly() { this.visibility = TypeAttributes.NotPublic; return this; }
	public GlobalTypeBuilder Public() { this.visibility = TypeAttributes.Public; return this; }
}

public sealed class NestedTypeBuilder : TypeBuilder<NestedTypeBuilder>
{
	internal NestedTypeBuilder(AssemblyBuilder assemblyBuilder) : base(assemblyBuilder, TypeAttributes.NestedPrivate) {}

	public NestedTypeBuilder Private() { this.visibility = TypeAttributes.NestedPrivate; return this; }
	public NestedTypeBuilder Family() { this.visibility = TypeAttributes.NestedFamily; return this; }
	public NestedTypeBuilder FamilyAndAssembly() { this.visibility = TypeAttributes.NestedFamANDAssem; return this; }
	public NestedTypeBuilder FamilyOrAssembly() { this.visibility = TypeAttributes.NestedFamORAssem; return this; }
	public NestedTypeBuilder Assembly() { this.visibility = TypeAttributes.NestedAssembly; return this; }
	public NestedTypeBuilder Public() { this.visibility = TypeAttributes.NestedPublic; return this; }
}

public sealed class FieldBuilder
{
	internal FieldBuilder() {}
	
	internal String? name { get; private set; }
	public FieldBuilder Name(String name) { this.name = name; return this; }
	
	internal TypeRef? typeRef { get; private set; }
	public FieldBuilder Type(Type type) { this.typeRef = type; return this; }
	public FieldBuilder Type(TypeBuilder typeBuilder) { this.typeRef = typeBuilder; return this; }
	
	internal TypeAttributes visibility { get; private set; }
	public FieldBuilder Private() { this.visibility = TypeAttributes.NestedPrivate; return this; }
	public FieldBuilder Family() { this.visibility = TypeAttributes.NestedFamily; return this; }
	public FieldBuilder FamilyAndAssembly() { this.visibility = TypeAttributes.NestedFamANDAssem; return this; }
	public FieldBuilder FamilyOrAssembly() { this.visibility = TypeAttributes.NestedFamORAssem; return this; }
	public FieldBuilder Assembly() { this.visibility = TypeAttributes.NestedAssembly; return this; }
	public FieldBuilder Public() { this.visibility = TypeAttributes.NestedPublic; return this; }
}

public sealed class PropertyBuilder
{
	internal PropertyBuilder() {}
}