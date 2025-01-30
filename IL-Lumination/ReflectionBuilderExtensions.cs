using System.Reflection;
using System.Reflection.Emit;

namespace Illumination;

public static class ReflectionBuilderExtensions
{
	public static Body Body(MethodBuilder mb) => new(mb.GetILGenerator());

	public static MethodBuilder Method(this TypeBuilder tb, string name, MethodAttributes attributes, System.Type returnType, params System.Type[] parameterTypes)
	{
		var mb = tb.DefineMethod(name, attributes, returnType, parameterTypes);
		return mb;
	}
}
