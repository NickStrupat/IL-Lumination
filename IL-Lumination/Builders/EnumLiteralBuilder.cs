using System;
using System.Numerics;

namespace Illumination;

public sealed class EnumLiteralBuilder : EnumLiteralBuilderBase<EnumLiteralBuilder>
{
	internal EnumLiteralBuilder() {}
	
	public EnumLiteralBuilder Value(Object value) { this.value = value; return this; }
}

public sealed class EnumLiteralBuilder<TUnderlying> : EnumLiteralBuilderBase<EnumLiteralBuilder<TUnderlying>>
	where TUnderlying : struct, IBinaryInteger<TUnderlying>
{
	internal EnumLiteralBuilder() {}
	
	public EnumLiteralBuilder<TUnderlying> Value(TUnderlying value) { this.value = value; return this; }
}

public abstract class EnumLiteralBuilderBase<T> : EnumLiteralBuilderBase where T : EnumLiteralBuilderBase<T>
{
	private protected EnumLiteralBuilderBase() {}
	
	public T Name(String name) { this.name = name; return (T)this; }
}

public abstract class EnumLiteralBuilderBase
{
	private protected EnumLiteralBuilderBase() {}
	
	internal String? name { get; private protected set; }
	internal Object? value { get; private protected set; }
}
