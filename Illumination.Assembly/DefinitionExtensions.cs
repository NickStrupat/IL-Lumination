using System.Reflection;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace Illumination.Assembly;

public static class DefinitionExtensions
{
	public static T AddChild<T>(this Definition def) where T : Definition, new()
	{
		var child = new T();
		child.Parent = def;
		return child;
	}
	
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
		var types = new Dictionary<Int64, TypeBuilder>();
		var assemblyName = assemblyDef.Name ?? throw new("Assembly name is null");
		//var assemblyBuilder = new PersistedAssemblyBuilder(new(assemblyName), typeof(Object).Assembly);
		var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new(assemblyName), AssemblyBuilderAccess.RunAndCollect);
		var assemblyDefEntry = context.Entry(assemblyDef);
		var typeDefs = context.Set<TypeDef>()
			.Where(x => x.Parent == assemblyDefEntry.Entity)
			.OrderBy(x => x.BaseType)
			.ToList();
		assemblyDefEntry.Collection(x => x.Modules).Load();
		//var id = assemblyDefEntry.Property<Int32>("Id").CurrentValue;
		foreach (var moduleDef in assemblyDef.Modules)
		{
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleDef.Name);
			context.Entry(moduleDef).Collection(x => x.Types).Load();
			foreach (var globalTypeDef in moduleDef.Types)
			{
				var typeBuilder = moduleBuilder.DefineType(globalTypeDef.Name, TypeAttributes.Public | TypeAttributes.Class);
				types.Add(globalTypeDef.Id, typeBuilder);
				context.Entry(globalTypeDef).Collection(x => x.Types).Load();
				DefineNestedTypes(typeBuilder, globalTypeDef.Types);

				void DefineNestedTypes(TypeBuilder tb, IEnumerable<TypeDef.NestedTypeDef> nestedTypeDefs)
				{
					foreach (var nestedTypeDef in nestedTypeDefs)
					{
						var nestedTypeBuilder = tb.DefineNestedType(nestedTypeDef.Name, TypeAttributes.NestedPublic | TypeAttributes.Class);
						types.Add(nestedTypeDef.Id, nestedTypeBuilder);
						context.Entry(nestedTypeDef).Collection(x => x.Types).Load();
						DefineNestedTypes(nestedTypeBuilder, nestedTypeDef.Types);
					}
				}
				
				// context.Entry(typeDef).Collection(x => x.Methods).Load();
				// foreach (var methodDef in typeDef.Methods)
				// {
				// 	var methodBuilder = typeBuilder.DefineMethod(methodDef.Name, MethodAttributes.Public | MethodAttributes.Static);
				// 	methodBuilder.SetReturnType(methodDef.ReturnType);
				// 	context.Entry(methodDef).Collection(x => x.Parameters).Load();
				// 	var index = 0;
				// 	var types = methodDef.Parameters.Select(x => x.TypeRef.Resolve(context)).ToArray();
				// 	methodBuilder.SetParameters(types);
				// 	foreach (var parameterDef in methodDef.Parameters)
				// 	{
				// 		var parameterBuilder = methodBuilder.DefineParameter(index++, ParameterAttributes.Optional, parameterDef.Name);
				// 	}
				// }
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