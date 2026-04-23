using System;
using System.Collections.Generic;
using System.Linq;
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

	internal readonly List<CustomAttributeBuilder> customAttributes = new();
	internal readonly List<TypeRef> interfaces = new();
	internal readonly List<TypeParameterBuilder> typeParameters = new();
	internal readonly List<FieldBuilder> fields = new();
	internal readonly List<PropertyBuilder> properties = new();
	internal readonly List<NestedTypeBuilder> types = new();
	internal readonly List<NestedEnumBuilder> enums = new();
	internal readonly List<ConstructorBuilder> constructors = new();
	internal readonly List<NestedMethodBuilder> methods = new();
	internal readonly List<EventBuilder> events = new();
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

	public T AddCustomAttribute(Type attributeType) => AddCustomAttribute(attributeType.GetConstructor(Type.EmptyTypes)!, []);
	public T AddCustomAttribute(ConstructorInfo constructor, params Object[] constructorArgs) { customAttributes.Add(new(constructor, constructorArgs, null, null, null, null)); return (T)this; }
	public T AddCustomAttribute(CustomAttributeBuilder customAttributeBuilder) { customAttributes.Add(customAttributeBuilder); return (T)this; }

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

	public T NewEvent(out EventBuilder eventBuilder, Action<EventBuilder> action) => (T)this.AddAction(events, eventBuilder = new(this), action);
	public T NewEvent(out EventBuilder eventBuilder) => NewEvent(out eventBuilder, _ => {});
	public T NewEvent(Action<EventBuilder> action) => NewEvent(out _, action);

	public T NewStaticConstructor(out ConstructorBuilder constructorBuilder, Action<ConstructorBuilder> builderAction) { constructorBuilder = new(this) { storageType = MethodAttributes.Static }; constructorBuilder.Private(); constructors.Add(constructorBuilder); builderAction(constructorBuilder); return (T)this; }
	public T NewStaticConstructor(out ConstructorBuilder constructorBuilder) => NewStaticConstructor(out constructorBuilder, _ => {});
	public T NewStaticConstructor(Action<ConstructorBuilder> builderAction) => NewStaticConstructor(out _, builderAction);

	public T NewAutoProperty(Action<AutoPropertyBuilder> action) => NewAutoProperty(out _, action);
	public T NewAutoProperty(out PropertyBuilder propertyBuilder) => NewAutoProperty(out propertyBuilder, _ => {});
	public T NewAutoProperty(out PropertyBuilder propertyBuilder, Action<AutoPropertyBuilder> action)
	{
		var config = new AutoPropertyBuilder();
		action(config);
		var propertyName = config.name ?? throw new InvalidOperationException("Auto properties require a name.");
		var propertyTypeRef = config.typeRef ?? throw new InvalidOperationException("Auto properties require a type.");

		NewField(out var backingField, f =>
		{
			f.Private().Name("<" + propertyName + ">k__BackingField");
			switch (propertyTypeRef)
			{
				case TypeRef.Declared clr: f.Type(clr.Type); break;
				case TypeRef.Builder b: f.Type(b.TypeBuilder); break;
			}
			if (config.storageType == MethodAttributes.Static)
				f.Static();
		});

		NewProperty(out propertyBuilder, p =>
		{
			switch (propertyTypeRef)
			{
				case TypeRef.Declared clr: p.Type(clr.Type); break;
				case TypeRef.Builder b: p.Type(b.TypeBuilder); break;
			}
			p.Name(propertyName);
			if (config.storageType == MethodAttributes.Static)
				p.Static();

			if (config.hasGetter)
			{
				p.Get(g =>
				{
					ApplyVisibility(g, config.getterVisibility ?? config.visibility);
					if (config.storageType == MethodAttributes.Static)
						g.Body(b => b.Ldsfld(backingField).Ret());
					else
						g.Body(b => b.Ldarg_0().Ldfld(backingField).Ret());
				});
			}

			if (config.hasSetter)
			{
				p.Set(s =>
				{
					ApplyVisibility(s, config.setterVisibility ?? config.visibility);
					if (config.storageType == MethodAttributes.Static)
						s.Body(b => b.Ldarg_0().Stsfld(backingField).Ret());
					else
						s.Body(b => b.Ldarg_0().Ldarg_1().Stfld(backingField).Ret());
				});
			}
		});

		return (T)this;
	}

	public T NewAutoEvent(Action<AutoEventBuilder> action) => NewAutoEvent(out _, action);
	public T NewAutoEvent(out EventBuilder eventBuilder) => NewAutoEvent(out eventBuilder, _ => {});
	public T NewAutoEvent(out EventBuilder eventBuilder, Action<AutoEventBuilder> action)
	{
		var config = new AutoEventBuilder();
		action(config);
		var handlerType = config.handlerType ?? throw new InvalidOperationException("Auto events require a handler type.");
		var eventName = config.name ?? throw new InvalidOperationException("Auto events require a name.");

		var combine = typeof(Delegate).GetMethod(nameof(Delegate.Combine), [typeof(Delegate), typeof(Delegate)])!;
		var remove = typeof(Delegate).GetMethod(nameof(Delegate.Remove), [typeof(Delegate), typeof(Delegate)])!;
		var compareExchange = typeof(System.Threading.Interlocked)
			.GetMethods().First(m => m.Name == nameof(System.Threading.Interlocked.CompareExchange) && m.IsGenericMethod)
			.MakeGenericMethod(handlerType);

		NewField(out var backingField, f => f.Private().Type(handlerType).Name(eventName));
		NewEvent(out eventBuilder, e => e.Name(eventName).HandlerType(handlerType)
			.AddMethod(m =>
			{
				ApplyVisibility(m, config.visibility);
				m.Name("add_" + eventName)
					.NewParameter(p => p.Type(handlerType).Name("value"))
					.Body(b => b
						.DeclareLocal(handlerType, out var current)
						.DeclareLocal(handlerType, out var previous)
						.Ldarg_0().Ldfld(backingField).Stloc(current)
						.DefineLabel(out var loop).MarkLabel(loop)
						.Ldloc(current).Stloc(previous)
						.Ldarg_0().Ldflda(backingField)
						.Ldloc(previous).Ldarg_1()
						.Call(combine).Castclass(handlerType)
						.Ldloc(previous)
						.Call(compareExchange)
						.Stloc(current)
						.Ldloc(current).Ldloc(previous).Bne_Un(loop)
						.Ret()
					);
			})
			.RemoveMethod(m =>
			{
				ApplyVisibility(m, config.visibility);
				m.Name("remove_" + eventName)
					.NewParameter(p => p.Type(handlerType).Name("value"))
					.Body(b => b
						.DeclareLocal(handlerType, out var current)
						.DeclareLocal(handlerType, out var previous)
						.Ldarg_0().Ldfld(backingField).Stloc(current)
						.DefineLabel(out var loop).MarkLabel(loop)
						.Ldloc(current).Stloc(previous)
						.Ldarg_0().Ldflda(backingField)
						.Ldloc(previous).Ldarg_1()
						.Call(remove).Castclass(handlerType)
						.Ldloc(previous)
						.Call(compareExchange)
						.Stloc(current)
						.Ldloc(current).Ldloc(previous).Bne_Un(loop)
						.Ret()
					);
			})
		);
		return (T)this;
	}

	private static void ApplyVisibility(NestedMethodBuilder m, MethodAttributes visibility)
	{
		_ = (visibility & MethodAttributes.MemberAccessMask) switch
		{
			MethodAttributes.Public => m.Public(),
			MethodAttributes.Family => m.Family(),
			MethodAttributes.Assembly => m.Assembly(),
			MethodAttributes.FamANDAssem => m.FamilyAndAssembly(),
			MethodAttributes.FamORAssem => m.FamilyOrAssembly(),
			_ => m.Private()
		};
	}

	private static void ApplyVisibility<TAccessor>(AccessorBuilder<TAccessor> a, MethodAttributes visibility) where TAccessor : AccessorBuilder<TAccessor>
	{
		_ = (visibility & MethodAttributes.MemberAccessMask) switch
		{
			MethodAttributes.Public => a.Public(),
			MethodAttributes.Family => a.Family(),
			MethodAttributes.Assembly => a.Assembly(),
			MethodAttributes.FamANDAssem => a.FamilyAndAssembly(),
			MethodAttributes.FamORAssem => a.FamilyOrAssembly(),
			_ => a.Private()
		};
	}
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
	public FieldBuilder FamilyOrAssembly() { this.visibility = FieldAttributes.FamORAssem; return this; }
	public FieldBuilder Assembly() { this.visibility = FieldAttributes.Assembly; return this; }
	public FieldBuilder Public() { this.visibility = FieldAttributes.Public; return this; }

	internal FieldAttributes storageType { get; private set; }
	public FieldBuilder Static() { this.storageType = FieldAttributes.Static; return this; }
	public FieldBuilder NonStatic() { this.storageType = default; return this; }

	internal FieldAttributes mutability { get; private set; }
	public FieldBuilder Readonly() { this.mutability = FieldAttributes.InitOnly; return this; }

	internal Object? constantValue { get; private set; }
	internal Boolean hasConstantValue { get; private set; }
	public FieldBuilder Literal(Object value) { this.mutability = FieldAttributes.Literal | FieldAttributes.HasDefault; this.constantValue = value; this.hasConstantValue = true; return this; }

	internal readonly List<CustomAttributeBuilder> customAttributes = new();
	public FieldBuilder AddCustomAttribute(System.Type attributeType) { customAttributes.Add(new(attributeType.GetConstructor(System.Type.EmptyTypes)!, [], null, null, null, null)); return this; }
	public FieldBuilder AddCustomAttribute(ConstructorInfo constructor, params Object[] constructorArgs) { customAttributes.Add(new(constructor, constructorArgs, null, null, null, null)); return this; }
	public FieldBuilder AddCustomAttribute(CustomAttributeBuilder customAttributeBuilder) { customAttributes.Add(customAttributeBuilder); return this; }
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
	public PropertyBuilder Type<T>(NullabilityState nullability) where T : class? => Type(typeof(T));
	public PropertyBuilder Type<T>() where T : struct => Type(typeof(T));
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

	public T Private() { this.visibility = MethodAttributes.Private; return (T)this; }
	public T Family() { this.visibility = MethodAttributes.Family; return (T)this; }
	public T FamilyAndAssembly() { this.visibility = MethodAttributes.FamANDAssem; return (T)this; }
	public T FamilyOrAssembly() { this.visibility = MethodAttributes.FamORAssem; return (T)this; }
	public T Assembly() { this.visibility = MethodAttributes.Assembly; return (T)this; }
	public T Public() { this.visibility = MethodAttributes.Public; return (T)this; }

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
	internal MethodAttributes storageType { get; set; }
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

	internal readonly List<Action<BodyBuilder>> bodyActions = new();
	public ConstructorBuilder Body(Action<BodyBuilder> bodyAction) { this.bodyActions.Add(bodyAction); return this; }

	internal readonly List<CustomAttributeBuilder> customAttributes = new();
	public ConstructorBuilder AddCustomAttribute(Type attributeType) => AddCustomAttribute(attributeType.GetConstructor(Type.EmptyTypes)!, []);
	public ConstructorBuilder AddCustomAttribute(ConstructorInfo constructor, params Object[] constructorArgs) { customAttributes.Add(new(constructor, constructorArgs, null, null, null, null)); return this; }
	public ConstructorBuilder AddCustomAttribute(CustomAttributeBuilder customAttributeBuilder) { customAttributes.Add(customAttributeBuilder); return this; }
}