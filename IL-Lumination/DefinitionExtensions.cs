using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace Illumination;

public static class DefinitionExtensions
{
	// public static List<AssemblyBuilder> CreateAssemblies(this DbContext context)
	// {
	// 	List<AssemblyBuilder> assemblies = new();
	// 	foreach (var assemblyDef in context.Set<AssemblyDef>().ToList())
	// 	{
	// 		assemblies.Add(assemblyDef.CreateAssembly(context));
	// 	}
	// 	return assemblies;
	// }

	public static AssemblyBuilder CreateAssembly(this AssemblyDef assemblyDef, DbContext context)
	{
		var assemblyName = assemblyDef.Name ?? throw new("Assembly name is null");
		var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new(assemblyName), AssemblyBuilderAccess.RunAndCollect);
		var assemblyDefEntry = context.Entry(assemblyDef);
		assemblyDefEntry.Collection(x => x.Modules).Load();
		var id = assemblyDefEntry.Property<Int32>("Id").CurrentValue;
		foreach (var moduleDef in assemblyDef.Modules)
		{
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleDef.Name);
			context.Entry(moduleDef).Collection(x => x.Methods).Load();
			foreach (var typeDef in moduleDef.Types)
			{
				var typeBuilder = moduleBuilder.DefineType(typeDef.Name, TypeAttributes.Public | TypeAttributes.Class);
				context.Entry(typeDef).Collection(x => x.Methods).Load();
				foreach (var methodDef in typeDef.Methods)
				{
					var methodBuilder = typeBuilder.DefineMethod(methodDef.Name, MethodAttributes.Public | MethodAttributes.Static);
					methodBuilder.SetReturnType(methodDef.ReturnType);
					context.Entry(methodDef).Collection(x => x.Parameters).Load();
					var index = 0;
					var types = methodDef.Parameters.Select(x => x.TypeRef.Resolve()).ToArray();
					methodBuilder.SetParameters(types);
					foreach (var parameterDef in methodDef.Parameters)
					{
						var parameterBuilder = methodBuilder.DefineParameter(index++, ParameterAttributes.Optional, parameterDef.Name);
					}
				}
			}
		}
		return assemblyBuilder;
	}
	
	// public static ModuleBuilder CreateModule(this ModuleDef moduleDef, DbContext context)
	// {
	// 	var moduleBuilder = 
	// 	context.Entry(moduleDef).Collection(x => x.Methods).Load();
	// 	foreach (var methodDef in moduleDef.Methods)
	// 	{
	// 		//context.Emit(methodDef);
	// 	}
	// 	return null;
	// }
}