using System;
using System.Reflection;

namespace Illumination;

internal sealed class TypeRef
{
	public static implicit operator TypeRef(Type type) => new(() => type);
	public static implicit operator TypeRef(TypeBuilder typeBuilder) => new(typeBuilder.Build);

	private readonly Func<Type> resolve;
	public Type Resolve() => resolve();
	
	private TypeRef(Func<Type> resolve) => this.resolve = resolve;
}

internal sealed class MethodRef
{
	public static implicit operator MethodRef(MethodInfo methodInfo) => new(() => methodInfo);
	public static implicit operator MethodRef(MethodBuilder methodBuilder) => throw new();//new(methodBuilder.Build);

	private readonly Func<MethodInfo> resolve;
	public MethodInfo Resolve() => resolve();
	
	private MethodRef(Func<MethodInfo> resolve) => this.resolve = resolve;
}