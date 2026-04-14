using System;
using System.Reflection;

namespace Illumination.Builders;

public sealed class ParameterBuilder : ParameterBuilderBase<ParameterBuilder>
{
	internal ParameterBuilder(Int16 index) : base(index) {}

	public ParameterBuilder Type(Type type) { this.typeRef = type; return this; }
	public ParameterBuilder Type(TypeBuilder typeBuilder) { this.typeRef = typeBuilder; return this; }
	public new ParameterBuilder HasDefault(Object? value) => base.HasDefault(value);
}

public sealed class ParameterBuilder<T> : ParameterBuilderBase<ParameterBuilder<T>>
{
	internal ParameterBuilder(Int16 index) : base(index) => typeRef = typeof(T);

	public ParameterBuilder<T> HasDefault(T value) => base.HasDefault(value);
}

public abstract class ParameterBuilderBase<T> : ParameterBuilderBase where T : ParameterBuilderBase<T>
{
	private protected ParameterBuilderBase(Int16 index) : base(index) {}

	public T Name(String name) { this.name = name; return (T)this; }
	public T In() { this.attributes |= ParameterAttributes.In; return (T)this; }
	public T Out() { this.attributes |= ParameterAttributes.Out; return (T)this; }
	public T Optional() { this.attributes |= ParameterAttributes.Optional; return (T)this; }
	protected T HasDefault(Object? value) { this.attributes |= ParameterAttributes.HasDefault | ParameterAttributes.Optional; this.defaultValue = value; this.hasDefaultValue = true; return (T)this; }
}

public abstract class ParameterBuilderBase : IBuilder<System.Reflection.Emit.ParameterBuilder>
{
	internal readonly Int16 index;
	private protected ParameterBuilderBase(Int16 index) => this.index = index;

	internal String? name { get; private protected set; }
	internal TypeRef? typeRef { get; private protected set; }
	internal ParameterAttributes attributes { get; private protected set; }
	internal Object? defaultValue { get; private protected set; }
	internal Boolean hasDefaultValue { get; private protected set; }
}