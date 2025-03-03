using System.Reflection;

namespace Illumination.Builders;

internal abstract record MethodRef
{
	public sealed record Declared(MethodInfo MethodInfo) : MethodRef;
	public sealed record Builder(MethodBuilder MethodBuilder) : MethodRef;
	
	public static implicit operator MethodRef(MethodInfo methodInfo) => new Declared(methodInfo);
	public static implicit operator MethodRef(MethodBuilder methodBuilder) => new Builder(methodBuilder);

	private MethodRef() {}
}
