using System;

namespace Illumination.Builders;

public sealed class LocalBuilder : LocalBuilderBase<LocalBuilder>
{
	internal LocalBuilder(Int16 index) : base(index) {}

	public LocalBuilder Type(Type type) { this.typeRef = type; return this; }
	public LocalBuilder Type(TypeBuilder typeBuilder) { this.typeRef = typeBuilder; return this; }
}

public sealed class LocalBuilder<T> : LocalBuilderBase<LocalBuilder<T>>
{
	internal LocalBuilder(Int16 index) : base(index) => typeRef = typeof(T);
}

public abstract class LocalBuilderBase<T> : LocalBuilderBase where T : LocalBuilderBase<T>
{
	private protected LocalBuilderBase(Int16 index) : base(index) {}
	
	public T Name(String name) { this.name = name; return (T)this; }
}

public abstract class LocalBuilderBase : IBuilder<System.Reflection.Emit.LocalBuilder>
{
	internal readonly Int16 index;
	private protected LocalBuilderBase(Int16 index) => this.index = index;
	
	internal String? name { get; private protected set; }
	internal TypeRef? typeRef { get; private protected set; }
}