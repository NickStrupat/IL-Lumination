using System;
using System.Linq;
using System.Reflection;

namespace Illumination.Builders;

internal sealed class BuildContext(BuilderMap builderMap, Assembly? coreAssembly)
{

	public T GetBuilder<T>(IBuilder<T> builder) where T : class => builderMap.Get(builder);
	// public System.Reflection.Emit.TypeBuilder ResolveTypeBuilder(TypeBuilder typeBuilder) => builderMap.Get(typeBuilder);
	// public System.Reflection.Emit.TypeBuilder ResolveTypeBuilder(EnumBuilder enumBuilder) => builderMap.Get(enumBuilder);
	// public System.Reflection.Emit.MethodBuilder ResolveMethodBuilder(MethodBuilder methodBuilder) => builderMap.Get(methodBuilder);

	public Type GetCoreAssemblyType(Type type) => coreAssembly is null ? type : coreAssembly.GetType(type.FullName!) ?? type;
	public MethodInfo GetCoreAssemblyMethod(MethodInfo method)
	{
		//if (method.Module.Assembly == coreAssembly)
		if (coreAssembly is null)
			return method;
		var parameterTypes = method.GetParameters().Select(x => GetCoreAssemblyType(x.ParameterType)).ToArray();
		if (method.DeclaringType is { } dt)
		{
			// var bindingFlags = method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
			// bindingFlags |= method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
			return coreAssembly.GetType(dt.FullName!)?.GetMethod(method.Name, parameterTypes) ?? throw new("No method found in core assembly");
		}

		return coreAssembly.Modules.Select(x => x.GetMethod(method.Name, parameterTypes) ?? throw new("No method found in core assembly")).First();
	}

	public Type ResolveType(TypeRef typeRef) => typeRef switch
	{
		TypeRef.Declared d => GetCoreAssemblyType(d.Type),
		TypeRef.Builder b => builderMap.Get(b.TypeBuilder),
		_ => throw new ArgumentOutOfRangeException(nameof(typeRef))
	};

	public MethodInfo ResolveMethod(MethodRef methodRef) => methodRef switch
	{
		MethodRef.Declared d => d.MethodInfo,
		MethodRef.Builder b => builderMap.Get(b.MethodBuilder),
		_ => throw new ArgumentOutOfRangeException(nameof(methodRef))
	};

	// public System.Reflection.Emit.MethodBuilder ResolveMethod(MethodBuilder mb) => methodBuilderMap[mb];
}