using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using Sre = System.Reflection.Emit;

namespace Illumination.Builders.Extensions;

public static class AssemblyBuilderExtensions
{
	public static Assembly Create(this AssemblyBuilder assemblyBuilder) => assemblyBuilder.Create(out _);
	public static Assembly Create(this AssemblyBuilder assemblyBuilder, out MethodInfo? entryPoint)
	{
		var ab = Sre.AssemblyBuilder.DefineDynamicAssembly(new(assemblyBuilder.name ?? Guid.NewGuid().ToString("N")), Sre.AssemblyBuilderAccess.RunAndCollect);
		assemblyBuilder.Build(ab, coreAssembly:null/*typeof(Object).Assembly*/, out entryPoint);
		return ab;
	}

	public static async Task Save(this AssemblyBuilder assemblyBuilder, String path)
	{
		var ab = GetPab(assemblyBuilder, path, out var coreAssembly);
		assemblyBuilder.Build(ab, coreAssembly, out var entryPoint);
		await ab.Save(path, entryPoint);
	}

	public static async Task SaveToExecutable(this AssemblyBuilder assemblyBuilder, String path)
	{
		var ab = GetPab(assemblyBuilder, path, out var coreAssembly);
		assemblyBuilder.Build(ab, coreAssembly, out var entryPoint);
		await ab.SaveToExecutable(path, entryPoint!);
	}

	private static Sre.PersistedAssemblyBuilder GetPab(this AssemblyBuilder assemblyBuilder, String path, out Assembly coreAssembly)
	{
		//ToolLocationHelper.GetPathToReferenceAssemblies(RuntimeEnvironment.)
		var what = RuntimeEnvironment.GetRuntimeDirectory();
		var refAssembliesPath = "/usr/local/share/dotnet/shared/Microsoft.NETCore.App/9.0.2";//"/usr/local/share/dotnet/packs/NETStandard.Library.Ref/2.1.0/ref/netstandard2.1";//RuntimeEnvironment.GetRuntimeDirectory();
		var assemblyPaths = Directory.GetFiles(what, "*.dll");
		PathAssemblyResolver resolver = new PathAssemblyResolver(assemblyPaths);
		var mlc = new MetadataLoadContext(resolver);
		coreAssembly = mlc.CoreAssembly ?? throw new Exception("Core assembly not found.");
		var pab = new Sre.PersistedAssemblyBuilder(new(assemblyBuilder.name ?? Path.GetFileNameWithoutExtension(path)), coreAssembly);
		
		var targetFrameworkAttributeBuilder = new Sre.CustomAttributeBuilder(
			typeof(TargetFrameworkAttribute).GetConstructor([typeof(String)])!,
			[".NETCoreApp,Version=v9.0"],
			[typeof(TargetFrameworkAttribute).GetProperty(nameof(TargetFrameworkAttribute.FrameworkDisplayName))!],
			[".NET 9.0"]);
		pab.SetCustomAttribute(targetFrameworkAttributeBuilder);
		return pab;
	}

	private static void Build(
		this AssemblyBuilder assemblyBuilder,
		Sre.AssemblyBuilder ab,
		Assembly? coreAssembly,
		out MethodInfo? entryPoint)
	{
		var module = ab.DefineDynamicModule("<Module>"); // only one module is allowed since .NET 5 (https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.assemblybuilder.definedynamicmodule?view=net-5.0#:~:text=Remarks)
		var definerMap = new DefinerMap();
		var builderMap = new BuilderMap();
		var buildContext = new BuildContext(builderMap, coreAssembly);

		Sre.TypeBuilder DefineAllType(TypeBuilder typeBuilder, Definer definer)
		{
			if (String.IsNullOrEmpty(typeBuilder.name))
				throw new InvalidOperationException("TypeBuilder requires a name to be configured before building.");

			var typeAttributes = typeBuilder.visibility | typeBuilder.classOrNot | typeBuilder.abstractOrSealed;
			var tb = definer.DefineType(typeBuilder.name, typeAttributes);
			if (GetUnbuiltBuilderOrNull(typeBuilder.baseTypeRef) is {} btb)
				DefineAllType(btb, definer);
			foreach (var typeRef in typeBuilder.interfaces)
				if (GetUnbuiltBuilderOrNull(typeRef) is {} itb)
					DefineAllType(itb, definer);
			builderMap.Add(typeBuilder, tb);
			foreach (var nestedTypeBuilder in typeBuilder.types)
				DefineAllType(nestedTypeBuilder, tb);
			foreach (var @enum in typeBuilder.enums)
				DefineEnum(@enum, tb);
			return tb;

			TypeBuilder? GetUnbuiltBuilderOrNull(TypeRef? tr) =>
				tr is TypeRef.Builder b && builderMap.GetOrNull(b.TypeBuilder) is null ? b.TypeBuilder : null;

			void DefineEnum(EnumBuilder enumBuilder, Sre.TypeBuilder etb)
			{
				if (String.IsNullOrEmpty(enumBuilder.name))
					throw new InvalidOperationException("EnumBuilder requires a name to be configured before building.");
				var eb = etb.DefineNestedType(enumBuilder.name, enumBuilder.visibility | TypeAttributes.Sealed);
				builderMap.Add(enumBuilder, eb);
			}
		}
		
		// First define all types to ensure that references between types can be resolved.
		// This is a depth-first traversal to ensure base types and interfaces are defined before their
		// derived types and implementing types.
		// This also defines enums.
		foreach (var gtb in assemblyBuilder.types)
			DefineAllType(gtb, module);
		
		// Define all methods to ensure that method references can be resolved.
		// This also defines properties and their accessors (which are methods).
		/////////////foreach (var asdf in assemblyBuilder.methods)
		
		// foreach (var gtb in assemblyBuilder.types)
		// {
		// 	foreach (var @interface in gtb.interfaces)
		// 		if (@interface is TypeRef.Builder b && !builderMap.TryGetBuilder(b.TypeBuilder, out _))
		// 			throw new InvalidOperationException("All interfaces must be defined before their implementing types.");
		// 	var typeAttributes = gtb.visibility | gtb.classOrNot | gtb.abstractOrSealed;
		// 	var tb = module.DefineType(gtb.name!, typeAttributes);
		// 	builderMap.Add(gtb, tb);
		// }
		//var buildContext = DefineAllTypesAndMethods(assemblyBuilder, module, coreAssembly);

		foreach (var type in assemblyBuilder.types)
			type.Build(buildContext);
		foreach (var method in assemblyBuilder.methods)
			method.Build(buildContext);
		foreach (var @enum in assemblyBuilder.enums)
			@enum.Build(buildContext);
		
		module.CreateGlobalFunctions();

		//entryPoint = assemblyBuilder.entryPoint is {} ep ? buildContext.ResolveMethod(ep) : null;
		entryPoint = ab.EntryPoint;
	}
}