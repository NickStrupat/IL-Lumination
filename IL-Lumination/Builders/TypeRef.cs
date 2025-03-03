using System;

namespace Illumination.Builders;

internal abstract record TypeRef
{
	public sealed record Declared(Type Type) : TypeRef;
	public sealed record Builder(TypeBuilder TypeBuilder) : TypeRef;
	
	public static implicit operator TypeRef(Type type) => new Declared(type);
	public static implicit operator TypeRef(TypeBuilder typeBuilder) => new Builder(typeBuilder);

	private TypeRef() {}
}