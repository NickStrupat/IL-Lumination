using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination.Builders;

public abstract class TypeBuilder : IBuilder<System.Reflection.Emit.TypeBuilder>
{
	internal readonly AssemblyBuilder assemblyBuilder; 
	private protected TypeBuilder(AssemblyBuilder assemblyBuilder, TypeAttributes visibility)
	{
		this.assemblyBuilder = assemblyBuilder; 
		this.visibility = visibility;
	}

	internal String? name { get; private protected set; }
	internal TypeAttributes visibility { get; private protected set; }
	internal TypeAttributes classOrNot { get; private protected set; }
	internal TypeAttributes abstractOrSealed { get; private protected set; }
	internal TypeRef? baseTypeRef { get; private protected set; }
	
	internal readonly List<TypeRef> interfaces = new();
	internal readonly List<TypeParameterBuilder> typeParameters = new();
	internal readonly List<FieldBuilder> fields = new();
	internal readonly List<PropertyBuilder> properties = new();
	internal readonly List<NestedTypeBuilder> types = new();
	internal readonly List<NestedEnumBuilder> enums = new();
	internal readonly List<ConstructorBuilder> constructors = new();
	internal readonly List<NestedMethodBuilder> methods = new();
}

public abstract class TypeBuilder<T> : TypeBuilder where T : TypeBuilder<T>
{
	private protected TypeBuilder(AssemblyBuilder assemblyBuilder, TypeAttributes visibility) : base(assemblyBuilder, visibility) {}
	
	public T Name(String name) { this.name = name; return (T)this; }
	public T Struct() { this.classOrNot = default; return BaseType<ValueType>(); }
	public T Class() { this.classOrNot = TypeAttributes.Class; return BaseType<Object>(); }
	public T Virtual() { this.abstractOrSealed = default; return (T)this; }
	public T Abstract() { this.abstractOrSealed = TypeAttributes.Abstract; return (T)this; }
	public T Sealed() { this.abstractOrSealed = TypeAttributes.Sealed; return (T)this; }
	
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
	
	public T NewMethod(out NestedMethodBuilder nestedMethodBuilder, Action<NestedMethodBuilder> action) => (T)this.AddAction(methods, nestedMethodBuilder = new(this), action);
	public T NewMethod(out NestedMethodBuilder nestedMethodBuilder) => NewMethod(out nestedMethodBuilder, _ => {});
	public T NewMethod(Action<NestedMethodBuilder> action) => NewMethod(out _, action);
	
	public T NewEnum(out NestedEnumBuilder nestedEnumBuilder, Action<NestedEnumBuilder> action) => (T)this.AddAction(enums, nestedEnumBuilder = new(), action);
	public T NewEnum(out NestedEnumBuilder nestedEnumBuilder) => NewEnum(out nestedEnumBuilder, _ => {});
	public T NewEnum(Action<NestedEnumBuilder> action) => NewEnum(out _, action);
	
	public T NewType(out NestedTypeBuilder nestedTypeBuilder, Action<NestedTypeBuilder> builderAction) => (T)this.AddAction(types, nestedTypeBuilder = new(assemblyBuilder, this), builderAction);
	public T NewType(out NestedTypeBuilder nestedTypeBuilder) => NewType(out nestedTypeBuilder, _ => {});
	public T NewType(Action<NestedTypeBuilder> builderAction) => NewType(out _, builderAction);
	
	public T NewConstructor(out ConstructorBuilder constructorBuilder, Action<ConstructorBuilder> builderAction) => (T)this.AddAction(constructors, constructorBuilder = new(this), builderAction);
	public T NewConstructor(out ConstructorBuilder constructorBuilder) => NewConstructor(out constructorBuilder, _ => {});
	public T NewConstructor(Action<ConstructorBuilder> builderAction) => NewConstructor(out _, builderAction);
}

public sealed class GlobalTypeBuilder : TypeBuilder<GlobalTypeBuilder>
{
	internal GlobalTypeBuilder(AssemblyBuilder assemblyBuilder) : base(assemblyBuilder, TypeAttributes.NotPublic) {}

	public GlobalTypeBuilder Assembly() { this.visibility = TypeAttributes.NotPublic; return this; }
	public GlobalTypeBuilder Public() { this.visibility = TypeAttributes.Public; return this; }
}

public sealed class NestedTypeBuilder : TypeBuilder<NestedTypeBuilder>
{
	internal NestedTypeBuilder(AssemblyBuilder assemblyBuilder, TypeBuilder containingType) : base(assemblyBuilder, TypeAttributes.NestedPrivate) => this.containingType = containingType;
	internal readonly TypeBuilder containingType;

	public NestedTypeBuilder Private() { this.visibility = TypeAttributes.NestedPrivate; return this; }
	public NestedTypeBuilder Family() { this.visibility = TypeAttributes.NestedFamily; return this; }
	public NestedTypeBuilder FamilyAndAssembly() { this.visibility = TypeAttributes.NestedFamANDAssem; return this; }
	public NestedTypeBuilder FamilyOrAssembly() { this.visibility = TypeAttributes.NestedFamORAssem; return this; }
	public NestedTypeBuilder Assembly() { this.visibility = TypeAttributes.NestedAssembly; return this; }
	public NestedTypeBuilder Public() { this.visibility = TypeAttributes.NestedPublic; return this; }
}

public sealed class FieldBuilder : IBuilder<System.Reflection.Emit.FieldBuilder>
{
	internal FieldBuilder() {}
	
	internal String? name { get; private set; }
	public FieldBuilder Name(String name) { this.name = name; return this; }
	
	internal TypeRef? typeRef { get; private set; }
	public FieldBuilder Type(Type type) { this.typeRef = type; return this; }
	public FieldBuilder Type<T>() => Type(typeof(T));
	public FieldBuilder Type(TypeBuilder typeBuilder) { this.typeRef = typeBuilder; return this; }
	
	internal FieldAttributes visibility { get; private set; }
	public FieldBuilder Private() { this.visibility = FieldAttributes.Private; return this; }
	public FieldBuilder Family() { this.visibility = FieldAttributes.Family; return this; }
	public FieldBuilder FamilyAndAssembly() { this.visibility = FieldAttributes.FamANDAssem; return this; }
	public FieldBuilder FamilyOrAssembly() { this.visibility = FieldAttributes.FamANDAssem; return this; }
	public FieldBuilder Assembly() { this.visibility = FieldAttributes.Assembly; return this; }
	public FieldBuilder Public() { this.visibility = FieldAttributes.Public; return this; }

	internal FieldAttributes storageType { get; private set; }
	public FieldBuilder Static() { this.storageType = FieldAttributes.Static; return this; }
	public FieldBuilder NonStatic() { this.storageType = default; return this; }
}

public sealed class PropertyBuilder : IBuilder<System.Reflection.Emit.PropertyBuilder>
{
	internal PropertyBuilder() {}
	
	internal String? name { get; private set; }
	public PropertyBuilder Name(String name) { this.name = name; return this; }
	
	internal MethodAttributes storageType { get; private set; }
	public PropertyBuilder Static() { this.storageType = MethodAttributes.Static; return this; }
	public PropertyBuilder NonStatic() { this.storageType = default; return this; }
	
	internal TypeRef? typeRef { get; private set; }
	public PropertyBuilder Type(Type type) { this.typeRef = type; return this; }
	public PropertyBuilder Type<T>() => Type(typeof(T));
	public PropertyBuilder Type(TypeBuilder typeBuilder) { this.typeRef = typeBuilder; return this; }
	
	internal GetterBuilder? getterBuilder { get; private set; }
	public PropertyBuilder Get(out GetterBuilder getterBuilder, Action<GetterBuilder> builderAction)
	{
		getterBuilder = this.getterBuilder = new(this);
		builderAction(getterBuilder);
		return this;
	}
	public PropertyBuilder Get(out GetterBuilder getterBuilder) => Get(out getterBuilder, _ => {});
	public PropertyBuilder Get(Action<GetterBuilder> builderAction) => Get(out _, builderAction);
	
	internal SetterBuilder? setterBuilder { get; private set; }
	public PropertyBuilder Set(out SetterBuilder setterBuilder, Action<SetterBuilder> builderAction)
	{
		setterBuilder = this.setterBuilder = new(this);
		builderAction(setterBuilder);
		return this;
	}
	public PropertyBuilder Set(out SetterBuilder setterBuilder) => Set(out setterBuilder, _ => {});
	public PropertyBuilder Set(Action<SetterBuilder> builderAction) => Set(out _, builderAction);
}

public abstract class AccessorBuilder<T> : IBuilder<System.Reflection.Emit.MethodBuilder> where T : AccessorBuilder<T>
{
	internal AccessorBuilder(PropertyBuilder propertyBuilder) => this.propertyBuilder = propertyBuilder;
	internal readonly PropertyBuilder propertyBuilder;
	
	internal MethodAttributes visibility { get; private set; }
	internal readonly List<Action<BodyBuilder>> bodyActions = new();
	internal readonly List<LocalBuilderBase> locals = new();

	public T Private() { this.visibility = MethodAttributes.Private; return (T)this; }
	public T Family() { this.visibility = MethodAttributes.Family; return (T)this; }
	public T FamilyAndAssembly() { this.visibility = MethodAttributes.FamANDAssem; return (T)this; }
	public T FamilyOrAssembly() { this.visibility = MethodAttributes.FamORAssem; return (T)this; }
	public T Assembly() { this.visibility = MethodAttributes.Assembly; return (T)this; }
	public T Public() { this.visibility = MethodAttributes.Public; return (T)this; }
	
	public T NewLocal(out LocalBuilder localBuilder, Action<LocalBuilder> localBuilderAction) => (T)this.AddAction(locals.AsContravariant(), localBuilder = new((Int16)locals.Count), localBuilderAction);
	public T NewLocal(out LocalBuilder localBuilder) => NewLocal(out localBuilder, _ => {});
	public T NewLocal(Action<LocalBuilder> localBuilderAction) => NewLocal(out _, localBuilderAction);
	
	public T NewLocal<TLocal>(out LocalBuilder<TLocal> localBuilder, Action<LocalBuilder<TLocal>> localBuilderAction) => (T)this.AddAction(locals.AsContravariant(), localBuilder = new((Int16)locals.Count), localBuilderAction);
	public T NewLocal<TLocal>(out LocalBuilder<TLocal> localBuilder) => NewLocal(out localBuilder, _ => {});
	public T NewLocal<TLocal>(Action<LocalBuilder<TLocal>> localBuilderAction) => NewLocal(out _, localBuilderAction);
	
	public T Body(Action<BodyBuilder> bodyAction) { this.bodyActions.Add(bodyAction); return (T)this; }
}

public sealed class GetterBuilder : AccessorBuilder<GetterBuilder>
{
	internal GetterBuilder(PropertyBuilder propertyBuilder) : base(propertyBuilder) {}
}

public sealed class SetterBuilder : AccessorBuilder<SetterBuilder>
{
	internal SetterBuilder(PropertyBuilder propertyBuilder) : base(propertyBuilder) {}
}

public sealed class ConstructorBuilder : IBuilder<System.Reflection.Emit.ConstructorBuilder>
{
	internal ConstructorBuilder(TypeBuilder containingType) => this.containingType = containingType;
	internal readonly TypeBuilder containingType;

	internal MethodAttributes visibility { get; private set; }
	public ConstructorBuilder Private() { this.visibility = MethodAttributes.Private; return this; }
	public ConstructorBuilder Family() { this.visibility = MethodAttributes.Family; return this; }
	public ConstructorBuilder FamilyAndAssembly() { this.visibility = MethodAttributes.FamANDAssem; return this; }
	public ConstructorBuilder FamilyOrAssembly() { this.visibility = MethodAttributes.FamORAssem; return this; }
	public ConstructorBuilder Assembly() { this.visibility = MethodAttributes.Assembly; return this; }
	public ConstructorBuilder Public() { this.visibility = MethodAttributes.Public; return this; }

	internal readonly List<ParameterBuilderBase> parameters = new();

	public ConstructorBuilder NewParameter(out ParameterBuilder parameterBuilder, Action<ParameterBuilder> parameterBuilderAction) =>
		this.AddAction(parameters.AsContravariant(), parameterBuilder = new((Int16)parameters.Count), parameterBuilderAction);
	public ConstructorBuilder NewParameter(out ParameterBuilder parameterBuilder) => NewParameter(out parameterBuilder, _ => {});
	public ConstructorBuilder NewParameter(Action<ParameterBuilder> parameterBuilderAction) => NewParameter(out _, parameterBuilderAction);

	public ConstructorBuilder NewParameter<TParam>(out ParameterBuilder<TParam> parameterBuilder, Action<ParameterBuilder<TParam>> parameterBuilderAction) =>
		this.AddAction(parameters.AsContravariant(), parameterBuilder = new((Int16)parameters.Count), parameterBuilderAction);
	public ConstructorBuilder NewParameter<TParam>(out ParameterBuilder<TParam> parameterBuilder) => NewParameter(out parameterBuilder, _ => {});
	public ConstructorBuilder NewParameter<TParam>(Action<ParameterBuilder<TParam>> parameterBuilderAction) => NewParameter(out _, parameterBuilderAction);

	internal readonly List<LocalBuilderBase> locals = new();

	public ConstructorBuilder NewLocal(out LocalBuilder localBuilder, Action<LocalBuilder> localBuilderAction) => this.AddAction(locals.AsContravariant(), localBuilder = new((Int16)locals.Count), localBuilderAction);
	public ConstructorBuilder NewLocal(out LocalBuilder localBuilder) => NewLocal(out localBuilder, _ => {});
	public ConstructorBuilder NewLocal(Action<LocalBuilder> localBuilderAction) => NewLocal(out _, localBuilderAction);

	public ConstructorBuilder NewLocal<TLocal>(out LocalBuilder<TLocal> localBuilder, Action<LocalBuilder<TLocal>> localBuilderAction) => this.AddAction(locals.AsContravariant(), localBuilder = new((Int16)locals.Count), localBuilderAction);
	public ConstructorBuilder NewLocal<TLocal>(out LocalBuilder<TLocal> localBuilder) => NewLocal(out localBuilder, _ => {});
	public ConstructorBuilder NewLocal<TLocal>(Action<LocalBuilder<TLocal>> localBuilderAction) => NewLocal(out _, localBuilderAction);

	internal readonly List<Action<BodyBuilder>> bodyActions = new();
	public ConstructorBuilder Body(Action<BodyBuilder> bodyAction) { this.bodyActions.Add(bodyAction); return this; }
}