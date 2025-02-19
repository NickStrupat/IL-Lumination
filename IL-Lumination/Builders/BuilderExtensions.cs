using System;
using System.Linq;
using System.Reflection;
using static System.Reflection.Emit.AssemblyBuilderAccess;
using Sre = System.Reflection.Emit;

namespace Illumination;

public enum AssemblyBuilderAccess { Run, Save }

public static class BuilderExtensions
{
	public static Assembly Create(this AssemblyBuilder assemblyBuilder)
	{
		var ab = Sre.AssemblyBuilder.DefineDynamicAssembly(new(assemblyBuilder.name ?? Guid.NewGuid().ToString("N")), RunAndCollect);
		assemblyBuilder.Build(ab);
		return ab;
	}

	public static void Save(this AssemblyBuilder assemblyBuilder, string path)
	{
		if (String.IsNullOrEmpty(assemblyBuilder.name))
			throw new InvalidOperationException("Saving an assembly requires a name.");
		var ab = new Sre.PersistedAssemblyBuilder(new(assemblyBuilder.name), typeof(Object).Assembly);
		assemblyBuilder.Build(ab);
		ab.Save(path);
	}

	private static void Build(this AssemblyBuilder assemblyBuilder, Sre.AssemblyBuilder ab)
	{
		var mb = ab.DefineDynamicModule("<Module>"); // only one module is allowed since .NET 5 (https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.assemblybuilder.definedynamicmodule?view=net-5.0#:~:text=Remarks)
		
		foreach (var method in assemblyBuilder.methods)
		{
			_ = method.Build((n, v, r, ps) => mb.DefineGlobalMethod(n, v, r, ps));
		}
		mb.CreateGlobalFunctions();

		foreach (var type in assemblyBuilder.types)
		{
			if (String.IsNullOrEmpty(type.name))
				throw new InvalidOperationException("Types require a name.");
			
			var tb = mb.DefineType(type.name, type.visibility);

			//type.Build(mb);
			tb.CreateType();
		}

		foreach (var @enum in assemblyBuilder.enums)
		{
			var eb = @enum.Build((n, v, u) => mb.DefineEnum(n, v, u));
			eb.CreateType();
		}
	}

	internal static MethodBuilder Build(this MethodBuilder methodBuilder, Func<String, MethodAttributes, Type, Type[], Sre.MethodBuilder> defineFunc)
	{
		if (String.IsNullOrEmpty(methodBuilder.name))
			throw new InvalidOperationException("Methods require a name.");
		foreach (var parameterBuilder in methodBuilder.parameters)
		{
			if (String.IsNullOrEmpty(parameterBuilder.name))
				throw new InvalidOperationException("Parameters require a name.");
			if (parameterBuilder.typeRef == null)
				throw new InvalidOperationException("Parameters require a type.");
		}
		var mb = defineFunc(
			methodBuilder.name!,
			methodBuilder.visibility | MethodAttributes.Static,
			methodBuilder.returnTypeRef!.Resolve(),
			methodBuilder.parameters.Select(x => x.typeRef!.Resolve()).ToArray()
		);
		var body = mb.GetILGenerator().Body();
		for (var index = 0; index < methodBuilder.parameters.Count; index++)
		{
			var pb = mb.DefineParameter(index + 1, ParameterAttributes.None, methodBuilder.parameters[index].name);
		}

		methodBuilder.bodyActions.ForEach(a => a.Invoke(body));

		return methodBuilder;
	}

	internal static Sre.TypeBuilder Build(this TypeBuilder typeBuilder)
	{
		throw new NotImplementedException();
	}

	internal static Sre.EnumBuilder Build(this GlobalEnumBuilder enumBuilder, Func<String, TypeAttributes, Type, Sre.EnumBuilder> defineFunc)
	{
		if (String.IsNullOrEmpty(enumBuilder.name))
			throw new InvalidOperationException("Enums require a name.");
		var eb = defineFunc(enumBuilder.name!, enumBuilder.visibility, enumBuilder.underlyingTypeRef.Resolve());
		foreach (var enumBuilderLiteral in enumBuilder.literals)
		{
			if (String.IsNullOrEmpty(enumBuilderLiteral.name))
				throw new InvalidOperationException("Enum literals require a name.");
			if (enumBuilderLiteral.value == null)
				throw new InvalidOperationException("Enum literals require a value.");
			eb.DefineLiteral(enumBuilderLiteral.name, enumBuilderLiteral.value);
		}
		return eb;
	}
}