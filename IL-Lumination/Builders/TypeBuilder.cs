using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination.Builders;

public abstract class TypeBuilder : IHasTypesAndMethods, IBuilder<System.Reflection.Emit.TypeBuilder>
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
	internal readonly List<NestedMethodBuilder> methods = new();

	IEnumerable<TypeBuilder> IHasTypesAndMethods.Types => types;
	IEnumerable<MethodBuilder> IHasTypesAndMethods.Methods => methods;
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

	public T NewField(out FieldBuilder fieldBuilder, Action<FieldBuilder> action) => (T)this.AddAction(fields, fieldBuilder = new(), action);

	public T NewProperty(out PropertyBuilder propertyBuilder, Action<PropertyBuilder> action) => (T)this.AddAction(properties, propertyBuilder = new(), action);

	public T NewMethod(out NestedMethodBuilder nestedMethodBuilder, Action<NestedMethodBuilder> action) => (T)this.AddAction(methods, nestedMethodBuilder = new(this), action);

	public T NewEnum(out NestedEnumBuilder nestedEnumBuilder, Action<NestedEnumBuilder> action) => (T)this.AddAction(enums, nestedEnumBuilder = new(), action);

	public T NewType(out NestedTypeBuilder nestedTypeBuilder, Action<NestedTypeBuilder> builderAction) => (T)this.AddAction(types, nestedTypeBuilder = new(assemblyBuilder, this), builderAction);
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

	internal SetterBuilder? setterBuilder { get; private set; }
	public PropertyBuilder Set(out SetterBuilder setterBuilder, Action<SetterBuilder> builderAction)
	{
		setterBuilder = this.setterBuilder = new(this);
		builderAction(setterBuilder);
		return this;
	}
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

	public T NewLocal<TLocal>(out LocalBuilder<TLocal> localBuilder, Action<LocalBuilder<TLocal>> localBuilderAction) => (T)this.AddAction(locals.AsContravariant(), localBuilder = new((Int16)locals.Count), localBuilderAction);

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

public static class TypeBuilderExtensions
{
	public static T NewTypeParameter<T>(this TypeBuilder<T> tb, out TypeParameterBuilder typeParameterBuilder) where T : TypeBuilder<T> =>
		tb.NewTypeParameter(out typeParameterBuilder, _ => {});
	public static T NewTypeParameter<T>(this TypeBuilder<T> tb, Action<TypeParameterBuilder> builderAction) where T : TypeBuilder<T> =>
		tb.NewTypeParameter(out _, builderAction);

	public static T NewField<T>(this TypeBuilder<T> tb, out FieldBuilder fieldBuilder) where T : TypeBuilder<T> =>
		tb.NewField(out fieldBuilder, _ => {});
	public static T NewField<T>(this TypeBuilder<T> tb, Action<FieldBuilder> action) where T : TypeBuilder<T> =>
		tb.NewField(out _, action);

	public static T NewProperty<T>(this TypeBuilder<T> tb, out PropertyBuilder propertyBuilder) where T : TypeBuilder<T> =>
		tb.NewProperty(out propertyBuilder, _ => {});
	public static T NewProperty<T>(this TypeBuilder<T> tb, Action<PropertyBuilder> action) where T : TypeBuilder<T> =>
		tb.NewProperty(out _, action);

	public static T NewMethod<T>(this TypeBuilder<T> tb, out NestedMethodBuilder nestedMethodBuilder) where T : TypeBuilder<T> =>
		tb.NewMethod(out nestedMethodBuilder, _ => {});
	public static T NewMethod<T>(this TypeBuilder<T> tb, Action<NestedMethodBuilder> action) where T : TypeBuilder<T> =>
		tb.NewMethod(out _, action);

	public static T NewEnum<T>(this TypeBuilder<T> tb, out NestedEnumBuilder nestedEnumBuilder) where T : TypeBuilder<T> =>
		tb.NewEnum(out nestedEnumBuilder, _ => {});
	public static T NewEnum<T>(this TypeBuilder<T> tb, Action<NestedEnumBuilder> action) where T : TypeBuilder<T> =>
		tb.NewEnum(out _, action);

	public static T NewType<T>(this TypeBuilder<T> tb, out NestedTypeBuilder nestedTypeBuilder) where T : TypeBuilder<T> =>
		tb.NewType(out nestedTypeBuilder, _ => {});
	public static T NewType<T>(this TypeBuilder<T> tb, Action<NestedTypeBuilder> builderAction) where T : TypeBuilder<T> =>
		tb.NewType(out _, builderAction);
}

public static class PropertyBuilderExtensions
{
	public static PropertyBuilder Get(this PropertyBuilder pb, out GetterBuilder getterBuilder) =>
		pb.Get(out getterBuilder, _ => {});
	public static PropertyBuilder Get(this PropertyBuilder pb, Action<GetterBuilder> builderAction) =>
		pb.Get(out _, builderAction);

	public static PropertyBuilder Set(this PropertyBuilder pb, out SetterBuilder setterBuilder) =>
		pb.Set(out setterBuilder, _ => {});
	public static PropertyBuilder Set(this PropertyBuilder pb, Action<SetterBuilder> builderAction) =>
		pb.Set(out _, builderAction);
}

public static class AccessorBuilderExtensions
{
	public static T NewLocal<T>(this AccessorBuilder<T> ab, out LocalBuilder localBuilder) where T : AccessorBuilder<T> =>
		ab.NewLocal(out localBuilder, _ => {});
	public static T NewLocal<T>(this AccessorBuilder<T> ab, Action<LocalBuilder> localBuilderAction) where T : AccessorBuilder<T> =>
		ab.NewLocal(out _, localBuilderAction);

	public static T NewLocal<T, TLocal>(this AccessorBuilder<T> ab, out LocalBuilder<TLocal> localBuilder) where T : AccessorBuilder<T> =>
		ab.NewLocal(out localBuilder, _ => {});
	public static T NewLocal<T, TLocal>(this AccessorBuilder<T> ab, Action<LocalBuilder<TLocal>> localBuilderAction) where T : AccessorBuilder<T> =>
		ab.NewLocal(out _, localBuilderAction);
}
