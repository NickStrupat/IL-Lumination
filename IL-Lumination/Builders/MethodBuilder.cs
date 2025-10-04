using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination.Builders;

public abstract class MethodBuilder : IBuilder<System.Reflection.Emit.MethodBuilder>
{
	private protected MethodBuilder(MethodAttributes visibility, MethodAttributes storageType)
	{
		this.visibility = visibility;
		this.storageType = storageType;
	}

	internal String? name { get; private protected set; }
	internal MethodAttributes visibility { get; private protected set; }
	internal MethodAttributes storageType { get; private protected set; }
	internal TypeRef returnTypeRef { get; private protected set; } = typeof(void);
	internal readonly List<ParameterBuilderBase> parameters = new();
	internal readonly List<TypeParameterBuilder> typeParameters = new();
	internal readonly List<Action<BodyBuilder>> bodyActions = new();
	internal readonly List<LocalBuilderBase> locals = new();
}

public abstract class MethodBuilder<T> : MethodBuilder where T : MethodBuilder<T>
{
	private protected MethodBuilder(MethodAttributes visibility, MethodAttributes storageType) : base(visibility, storageType) {}
	
	public T Name(String name) { this.name = name; return (T)this; }

	public T ReturnType(Type returnType) { this.returnTypeRef = returnType; return (T)this; }
	public T ReturnType<TReturn>() => ReturnType(typeof(TReturn));
	public T ReturnType(TypeBuilder returnTypeBuilder) { this.returnTypeRef = returnTypeBuilder; return (T)this; }

	public T NewParameter(out ParameterBuilder parameterBuilder, Action<ParameterBuilder> parameterBuilderAction) =>
		(T)this.AddAction(parameters.AsContravariant(), parameterBuilder = new((Int16)parameters.Count), parameterBuilderAction);

	public T NewParameter<TParam>(out ParameterBuilder<TParam> parameterBuilder, Action<ParameterBuilder<TParam>> parameterBuilderAction) =>
		(T)this.AddAction(parameters.AsContravariant(), parameterBuilder = new((Int16)parameters.Count), parameterBuilderAction);

	public T NewTypeParameter(out TypeParameterBuilder typeParameterBuilder, Action<TypeParameterBuilder> typeParameterBuilderAction) =>
		(T)this.AddAction(typeParameters, typeParameterBuilder = new(), typeParameterBuilderAction);

	public T NewLocal(out LocalBuilder localBuilder, Action<LocalBuilder> localBuilderAction) => (T)this.AddAction(locals.AsContravariant(), localBuilder = new((Int16)locals.Count), localBuilderAction);

	public T NewLocal<TLocal>(out LocalBuilder<TLocal> localBuilder, Action<LocalBuilder<TLocal>> localBuilderAction) => (T)this.AddAction(locals.AsContravariant(), localBuilder = new((Int16)locals.Count), localBuilderAction);

	public T Body(Action<BodyBuilder> bodyAction) { this.bodyActions.Add(bodyAction); return (T)this; }
}

public sealed class GlobalMethodBuilder : MethodBuilder<GlobalMethodBuilder>
{
	internal GlobalMethodBuilder(AssemblyBuilder assemblyBuilder) : base(MethodAttributes.Private, MethodAttributes.Static) => this.assemblyBuilder = assemblyBuilder;
	internal readonly AssemblyBuilder assemblyBuilder;

	public GlobalMethodBuilder Assembly() { this.visibility = MethodAttributes.Private; return this; }
	public GlobalMethodBuilder Public() { this.visibility = MethodAttributes.Public; return this; }
}

public sealed class NestedMethodBuilder : MethodBuilder<NestedMethodBuilder>
{
	internal NestedMethodBuilder(TypeBuilder containingType) : base(MethodAttributes.Private, default) => this.containingType = containingType;
	private readonly TypeBuilder containingType;

	public NestedMethodBuilder Private() { this.visibility = MethodAttributes.Private; return this; }
	public NestedMethodBuilder Family() { this.visibility = MethodAttributes.Family; return this; }
	public NestedMethodBuilder FamilyAndAssembly() { this.visibility = MethodAttributes.FamANDAssem; return this; }
	public NestedMethodBuilder FamilyOrAssembly() { this.visibility = MethodAttributes.FamORAssem; return this; }
	public NestedMethodBuilder Assembly() { this.visibility = MethodAttributes.Assembly; return this; }
	public NestedMethodBuilder Public() { this.visibility = MethodAttributes.Public; return this; }

	public NestedMethodBuilder Static() { this.storageType = MethodAttributes.Static; return this; }
	public NestedMethodBuilder NonStatic() { this.storageType = default; return this; }

	public NestedMethodBuilder EntryPoint() { this.containingType.assemblyBuilder.entryPoint = this; return this; }
}

public static class MethodBuilderExtensions
{
	public static T NewParameter<T>(this MethodBuilder<T> mb, out ParameterBuilder parameterBuilder) where T : MethodBuilder<T> =>
		mb.NewParameter(out parameterBuilder, _ => {});
	public static T NewParameter<T>(this MethodBuilder<T> mb, Action<ParameterBuilder> parameterBuilderAction) where T : MethodBuilder<T> =>
		mb.NewParameter(out _, parameterBuilderAction);

	public static T NewParameter<T, TParam>(this MethodBuilder<T> mb, out ParameterBuilder<TParam> parameterBuilder) where T : MethodBuilder<T> =>
		mb.NewParameter(out parameterBuilder, _ => {});
	public static T NewParameter<T, TParam>(this MethodBuilder<T> mb, Action<ParameterBuilder<TParam>> parameterBuilderAction) where T : MethodBuilder<T> =>
		mb.NewParameter(out _, parameterBuilderAction);

	public static T NewTypeParameter<T>(this MethodBuilder<T> mb, out TypeParameterBuilder typeParameterBuilder) where T : MethodBuilder<T> =>
		mb.NewTypeParameter(out typeParameterBuilder, _ => {});
	public static T NewTypeParameter<T>(this MethodBuilder<T> mb, Action<TypeParameterBuilder> typeParameterBuilderAction) where T : MethodBuilder<T> =>
		mb.NewTypeParameter(out _, typeParameterBuilderAction);

	public static T NewLocal<T>(this MethodBuilder<T> mb, out LocalBuilder localBuilder) where T : MethodBuilder<T> =>
		mb.NewLocal(out localBuilder, _ => {});
	public static T NewLocal<T>(this MethodBuilder<T> mb, Action<LocalBuilder> localBuilderAction) where T : MethodBuilder<T> =>
		mb.NewLocal(out _, localBuilderAction);

	public static T NewLocal<T, TLocal>(this MethodBuilder<T> mb, out LocalBuilder<TLocal> localBuilder) where T : MethodBuilder<T> =>
		mb.NewLocal(out localBuilder, _ => {});
	public static T NewLocal<T, TLocal>(this MethodBuilder<T> mb, Action<LocalBuilder<TLocal>> localBuilderAction) where T : MethodBuilder<T> =>
		mb.NewLocal(out _, localBuilderAction);
}