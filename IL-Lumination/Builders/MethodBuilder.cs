using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination;

public abstract class MethodBuilder
{
	private protected MethodBuilder(MethodAttributes visibility) => this.visibility = visibility;
	
	internal String? name { get; private protected set; }
	internal MethodAttributes visibility { get; private protected set; }
	internal TypeRef? returnTypeRef { get; private protected set; }
	internal readonly List<ParameterBuilderBase> parameters = new();
	internal readonly List<TypeParameterBuilder> typeParameters = new();
	internal readonly List<Action<BodyBuilder>> bodyActions = new();
}

public abstract class MethodBuilder<T> : MethodBuilder where T : MethodBuilder<T>
{
	private protected MethodBuilder(MethodAttributes visibility) : base(visibility) {}
	
	public T Name(String name) { this.name = name; return (T)this; }
	
	public T ReturnType(Type returnType) { this.returnTypeRef = returnType; return (T)this; }
	public T ReturnType<TReturn>() => ReturnType(typeof(TReturn));
	public T ReturnType(TypeBuilder returnTypeBuilder) { this.returnTypeRef = returnTypeBuilder; return (T)this; }

	public T NewParameter(out ParameterBuilder parameterBuilder, Action<ParameterBuilder> parameterBuilderAction)
	{
		parameterBuilder = new((Int16)parameters.Count);
		parameters.Add(parameterBuilder);
		parameterBuilderAction(parameterBuilder);
		return (T)this;
	}
	public T NewParameter(out ParameterBuilder parameterBuilder) => NewParameter(out parameterBuilder, _ => {});
	public T NewParameter(Action<ParameterBuilder> parameterBuilderAction) => NewParameter(out _, parameterBuilderAction);
	
	public T NewParameter<TParam>(out ParameterBuilder<T> parameterBuilder, Action<ParameterBuilder<T>> parameterBuilderAction)
	{
		parameterBuilder = new((Int16)parameters.Count);
		parameters.Add(parameterBuilder);
		parameterBuilderAction(parameterBuilder);
		return (T)this;
	}
	public T NewParameter<TParam>(out ParameterBuilder<T> parameterBuilder) => NewParameter<TParam>(out parameterBuilder, _ => {});
	public T NewParameter<TParam>(Action<ParameterBuilder<T>> parameterBuilderAction) => NewParameter<TParam>(out _, parameterBuilderAction);
	
	public T NewTypeParameter(out TypeParameterBuilder typeParameterBuilder, Action<TypeParameterBuilder> typeParameterBuilderAction) => (T)this.AddAction(typeParameters, typeParameterBuilder = new(), typeParameterBuilderAction);
	public T NewTypeParameter(out TypeParameterBuilder typeParameterBuilder) => NewTypeParameter(out typeParameterBuilder, _ => {});
	public T NewTypeParameter(Action<TypeParameterBuilder> typeParameterBuilderAction) => NewTypeParameter(out _, typeParameterBuilderAction);
	
	public T Body(Action<BodyBuilder> bodyAction) { this.bodyActions.Add(bodyAction); return (T)this; }
}

public sealed class GlobalMethodBuilder : MethodBuilder<GlobalMethodBuilder>
{
	internal GlobalMethodBuilder() : base(MethodAttributes.Private) {}

	public GlobalMethodBuilder Assembly() { this.visibility = MethodAttributes.Private; return this; }
	public GlobalMethodBuilder Public() { this.visibility = MethodAttributes.Public; return this; }
}

public sealed class NestedMethodBuilder : MethodBuilder<NestedMethodBuilder>
{
	internal NestedMethodBuilder() : base(MethodAttributes.Private) {}
	
	public NestedMethodBuilder Private() { this.visibility = MethodAttributes.Private; return this; }
	public NestedMethodBuilder Family() { this.visibility = MethodAttributes.Family; return this; }
	public NestedMethodBuilder FamilyAndAssembly() { this.visibility = MethodAttributes.FamANDAssem; return this; }
	public NestedMethodBuilder FamilyOrAssembly() { this.visibility = MethodAttributes.FamORAssem; return this; }
	public NestedMethodBuilder Assembly() { this.visibility = MethodAttributes.Assembly; return this; }
	public NestedMethodBuilder Public() { this.visibility = MethodAttributes.Public; return this; }
	
	internal MethodAttributes instantiation;
	public NestedMethodBuilder Static() { this.instantiation = MethodAttributes.Static; return this; }
	public NestedMethodBuilder NonStatic() { this.instantiation = default; return this; }
}