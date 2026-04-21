using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		var refAssembliesPath = GetReferenceAssembliesPath();
		var assemblyPaths = Directory.GetFiles(refAssembliesPath, "*.dll");
		PathAssemblyResolver resolver = new PathAssemblyResolver(assemblyPaths);
		var mlc = new MetadataLoadContext(resolver);
		coreAssembly = mlc.CoreAssembly ?? throw new Exception("Core assembly not found.");
		var pab = new Sre.PersistedAssemblyBuilder(new(assemblyBuilder.name ?? Path.GetFileNameWithoutExtension(path)), coreAssembly);

		var runtimeVersion = Environment.Version;
		var targetFrameworkAttributeBuilder = new Sre.CustomAttributeBuilder(
			typeof(TargetFrameworkAttribute).GetConstructor([typeof(String)])!,
			[$".NETCoreApp,Version=v{runtimeVersion.Major}.{runtimeVersion.Minor}"],
			[typeof(TargetFrameworkAttribute).GetProperty(nameof(TargetFrameworkAttribute.FrameworkDisplayName))!],
			[$".NET {runtimeVersion.Major}.{runtimeVersion.Minor}"]);
		pab.SetCustomAttribute(targetFrameworkAttributeBuilder);
		return pab;
	}

	private static String GetReferenceAssembliesPath()
	{
		var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT")
			?? Path.GetDirectoryName(RuntimeEnvironment.GetRuntimeDirectory().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
			?? throw new InvalidOperationException("Cannot determine .NET SDK root directory.");
		// Walk up from shared/Microsoft.NETCore.App/<version>/ to the dotnet root
		var root = Path.GetFullPath(Path.Combine(dotnetRoot, "..", ".."));
		var packsDir = Path.Combine(root, "packs", "Microsoft.NETCore.App.Ref");
		if (!Directory.Exists(packsDir))
			throw new DirectoryNotFoundException($"Reference assembly pack not found at: {packsDir}");
		var runtimeVersion = Environment.Version;
		var majorPrefix = runtimeVersion.Major + ".";
		var latestVersion = Directory.GetDirectories(packsDir)
			.Select(Path.GetFileName)
			.Where(v => v!.StartsWith(majorPrefix))
			.OrderByDescending(v => v)
			.FirstOrDefault() ?? throw new InvalidOperationException($"No .NET {runtimeVersion.Major} reference assembly pack found.");
		return Path.Combine(packsDir, latestVersion, "ref", $"net{runtimeVersion.Major}.0");
	}

	private static void Build(
		this AssemblyBuilder assemblyBuilder,
		Sre.AssemblyBuilder ab,
		Assembly? coreAssembly,
		out MethodInfo? entryPoint)
	{
		var module = ab.DefineDynamicModule("<Module>"); // only one module is allowed since .NET 5 (https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.assemblybuilder.definedynamicmodule?view=net-5.0#:~:text=Remarks)
		foreach (var ca in assemblyBuilder.customAttributes)
			ab.SetCustomAttribute(ca.ToSre());
		var builderMap = new BuilderMap();
		var buildContext = new BuildContext(builderMap, coreAssembly);

		Sre.TypeBuilder DefineAllType(TypeBuilder typeBuilder, Definer definer)
		{
			if (String.IsNullOrEmpty(typeBuilder.name))
				throw new InvalidOperationException($"{nameof(TypeBuilder)} requires a name to be configured before building.");

			var typeAttributes = typeBuilder.visibility | typeBuilder.classOrNot | typeBuilder.abstractOrSealed;
			var tb = definer.DefineType(typeBuilder.name, typeAttributes);
			if (GetUnbuiltBuilderOrNull(typeBuilder.baseTypeRef) is {} btb)
				DefineAllType(btb, definer);
			foreach (var typeRef in typeBuilder.interfaces)
				if (GetUnbuiltBuilderOrNull(typeRef) is {} itb)
					DefineAllType(itb, definer);
			builderMap.Add(typeBuilder, tb);
			if (typeBuilder.typeParameters.Count > 0)
				DefineGenericParameters(typeBuilder.typeParameters, tb.DefineGenericParameters(typeBuilder.typeParameters.Select(x => x.name ?? throw new InvalidOperationException("Type parameters require a name.")).ToArray()));
			foreach (var ca in typeBuilder.customAttributes)
				tb.SetCustomAttribute(ca.ToSre());
			foreach (var nestedTypeBuilder in typeBuilder.types)
				DefineAllType(nestedTypeBuilder, tb);
			foreach (var @enum in typeBuilder.enums)
				DefineEnum(@enum, tb);
			foreach (var methodBuilder in typeBuilder.methods)
				DefineMethod(methodBuilder, tb);
			foreach (var constructorBuilder in typeBuilder.constructors)
				DefineConstructor(constructorBuilder, tb);
			foreach (var fieldBuilder in typeBuilder.fields)
				DefineField(fieldBuilder, tb);
			foreach (var propertyBuilder in typeBuilder.properties)
				DefineProperty(propertyBuilder, tb);
			foreach (var eventBuilder in typeBuilder.events)
				DefineEvent(eventBuilder, tb);
			return tb;

			TypeBuilder? GetUnbuiltBuilderOrNull(TypeRef? tr) =>
				tr is TypeRef.Builder b && builderMap.GetOrNull(b.TypeBuilder) is null ? b.TypeBuilder : null;
		}

		void DefineEnum(EnumBuilder enumBuilder, Definer definer)
		{
			if (String.IsNullOrEmpty(enumBuilder.name))
				throw new InvalidOperationException("EnumBuilder requires a name to be configured before building.");
			var eb = definer.DefineType(enumBuilder.name, enumBuilder.visibility | TypeAttributes.Sealed);
			builderMap.Add(enumBuilder, eb);
		}

		void DefineMethod(MethodBuilder methodBuilder, Definer definer)
		{
			if (String.IsNullOrEmpty(methodBuilder.name))
				throw new InvalidOperationException("Methods require a name.");
			var returnType = methodBuilder.returnTypeRef is { } rt ? buildContext.ResolveType(rt) : null;
			var parameterTypes = methodBuilder.parameters.Count > 0
				? methodBuilder.parameters.Select(x => buildContext.ResolveType(x.typeRef!)).ToArray()
				: null;
			var mb = definer.DefineMethod(methodBuilder.name!, methodBuilder.visibility | methodBuilder.storageType | methodBuilder.virtuality, returnType, parameterTypes);
			if (methodBuilder.typeParameters.Count > 0)
				DefineGenericParameters(methodBuilder.typeParameters, mb.DefineGenericParameters(methodBuilder.typeParameters.Select(x => x.name ?? throw new InvalidOperationException("Type parameters require a name.")).ToArray()));
			for (var index = 0; index < methodBuilder.parameters.Count; index++)
			{
				var p = methodBuilder.parameters[index];
				var pb = mb.DefineParameter(index + 1, p.attributes, p.name);
				if (p.hasDefaultValue)
					pb.SetConstant(p.defaultValue);
			}
			foreach (var ca in methodBuilder.customAttributes)
				mb.SetCustomAttribute(ca.ToSre());
			builderMap.Add(methodBuilder, mb);
		}

		void DefineConstructor(ConstructorBuilder constructorBuilder, Sre.TypeBuilder tb)
		{
			var parameterTypes = constructorBuilder.parameters.Select(x => buildContext.ResolveType(x.typeRef!)).ToArray();
			var cb = tb.DefineConstructor(constructorBuilder.visibility | constructorBuilder.storageType, CallingConventions.Standard, parameterTypes);
			for (var index = 0; index < constructorBuilder.parameters.Count; index++)
			{
				var p = constructorBuilder.parameters[index];
				var pb = cb.DefineParameter(index + 1, p.attributes, p.name);
				if (p.hasDefaultValue)
					pb.SetConstant(p.defaultValue);
			}
			foreach (var ca in constructorBuilder.customAttributes)
				cb.SetCustomAttribute(ca.ToSre());
			builderMap.Add(constructorBuilder, cb);
		}

		void DefineField(FieldBuilder fieldBuilder, Sre.TypeBuilder tb)
		{
			if (String.IsNullOrEmpty(fieldBuilder.name))
				throw new InvalidOperationException("Fields require a name.");
			var type = fieldBuilder.typeRef is { } tr ? buildContext.ResolveType(tr) : throw new InvalidOperationException("Fields require a type.");
			var fb = tb.DefineField(fieldBuilder.name!, type, fieldBuilder.visibility | fieldBuilder.storageType | fieldBuilder.mutability);
			if (fieldBuilder.hasConstantValue)
				fb.SetConstant(fieldBuilder.constantValue);
			foreach (var ca in fieldBuilder.customAttributes)
				fb.SetCustomAttribute(ca.ToSre());
			builderMap.Add(fieldBuilder, fb);
		}

		void DefineProperty(PropertyBuilder propertyBuilder, Sre.TypeBuilder tb)
		{
			if (String.IsNullOrEmpty(propertyBuilder.name))
				throw new InvalidOperationException("Properties require a name.");
			var type = propertyBuilder.typeRef is { } tr ? buildContext.ResolveType(tr) : throw new InvalidOperationException("Properties require a type.");
			var pb = tb.DefineProperty(propertyBuilder.name!, PropertyAttributes.None, CallingConventions.Standard, type, null);
			builderMap.Add(propertyBuilder, pb);
			if (propertyBuilder.getterBuilder is { } getterBuilder)
			{
				var getter = tb.DefineMethod("get_" + propertyBuilder.name!, getterBuilder.visibility | MethodAttributes.SpecialName | MethodAttributes.HideBySig | propertyBuilder.storageType, type, null);
				pb.SetGetMethod(getter);
				builderMap.Add(getterBuilder, getter);
			}
			if (propertyBuilder.setterBuilder is { } setterBuilder)
			{
				var setter = tb.DefineMethod("set_" + propertyBuilder.name!, setterBuilder.visibility | MethodAttributes.SpecialName | MethodAttributes.HideBySig | propertyBuilder.storageType, null, [type]);
				pb.SetSetMethod(setter);
				builderMap.Add(setterBuilder, setter);
			}
		}

		void DefineEvent(EventBuilder eventBuilder, Sre.TypeBuilder tb)
		{
			if (String.IsNullOrEmpty(eventBuilder.name))
				throw new InvalidOperationException("Events require a name.");
			var handlerType = eventBuilder.eventHandlerTypeRef is { } tr ? buildContext.ResolveType(tr) : throw new InvalidOperationException("Events require a handler type.");
			var eb = tb.DefineEvent(eventBuilder.name!, EventAttributes.None, handlerType);
			builderMap.Add(eventBuilder, eb);
			if (eventBuilder.addMethodBuilder is { } addBuilder)
			{
				DefineMethod(addBuilder, tb);
				eb.SetAddOnMethod(buildContext.GetBuilder(addBuilder));
			}
			if (eventBuilder.removeMethodBuilder is { } removeBuilder)
			{
				DefineMethod(removeBuilder, tb);
				eb.SetRemoveOnMethod(buildContext.GetBuilder(removeBuilder));
			}
			// if (eventBuilder.raiseMethodBuilder is { } raiseBuilder)
			// {
			// 	DefineMethod(raiseBuilder, tb);
			// 	eb.SetRaiseMethod(buildContext.GetBuilder(raiseBuilder));
			// }
		}

		void DefineGenericParameters(List<TypeParameterBuilder> typeParameterBuilders, Sre.GenericTypeParameterBuilder[] gps)
		{
			for (var i = 0; i < typeParameterBuilders.Count; i++)
			{
				var tpb = typeParameterBuilders[i];
				var gp = gps[i];
				gp.SetGenericParameterAttributes(tpb.variance | tpb.special | tpb.allowByRefLike);
				if (tpb.baseType is { } bt)
					gp.SetBaseTypeConstraint(buildContext.ResolveType(bt));
				foreach (var iface in tpb.interfaces)
					gp.SetInterfaceConstraints(buildContext.ResolveType(iface));
				builderMap.Add(tpb, gp);
			}
		}

		// Define all types, methods, constructors, fields, and properties.
		// Types are defined depth-first to ensure base types and interfaces are defined
		// before their derived types and implementing types.
		foreach (var gtb in assemblyBuilder.types)
			DefineAllType(gtb, module);
		foreach (var @enum in assemblyBuilder.enums)
			DefineEnum(@enum, module);
		foreach (var method in assemblyBuilder.methods)
			DefineMethod(method, module);

		foreach (var type in assemblyBuilder.types)
			type.Build(buildContext);
		foreach (var method in assemblyBuilder.methods)
			method.Build(buildContext);
		foreach (var @enum in assemblyBuilder.enums)
			@enum.Build(buildContext);

		module.CreateGlobalFunctions();

		entryPoint = assemblyBuilder.entryPoint is {} ep ? buildContext.GetBuilder(ep) : null;
	}
}