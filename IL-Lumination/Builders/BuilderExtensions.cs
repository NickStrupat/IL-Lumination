using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static System.Reflection.Emit.AssemblyBuilderAccess;
using Sre = System.Reflection.Emit;
using Task = System.Threading.Tasks.Task;

namespace Illumination.Builders;

public static class BuilderExtensions
{
	public static Assembly Create(this AssemblyBuilder assemblyBuilder) => assemblyBuilder.Create(out _);
	public static Assembly Create(this AssemblyBuilder assemblyBuilder, out MethodInfo? entryPoint)
	{
		var ab = Sre.AssemblyBuilder.DefineDynamicAssembly(new(assemblyBuilder.name ?? Guid.NewGuid().ToString("N")), RunAndCollect);
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
		
		var buildContext = DefineAllTypesAndMethods(assemblyBuilder, module, coreAssembly);

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

	private static MethodInfo GetCreatedMethodInfo(this Sre.MethodBuilder methodBuilder)
	{
        ArgumentNullException.ThrowIfNull(methodBuilder);
        var dt = methodBuilder.DeclaringType;
        return null!;
	}

	private static BuildContext DefineAllTypesAndMethods(
		AssemblyBuilder assemblyBuilder,
		Sre.ModuleBuilder mb,
		Assembly? coreAssembly)
	{
		var builderMap = new BuilderMap();
		var buildContext = new BuildContext(builderMap, coreAssembly);
		// var typeBuilderMap = new Dictionary<TypeBuilder, Sre.TypeBuilder>();
		// var enumBuilderMap = new Dictionary<EnumBuilder, Sre.TypeBuilder>();
		// var methodBuilderMap = new Dictionary<MethodBuilder, Sre.MethodBuilder>();

		foreach (var type in assemblyBuilder.types)
			DefineTypesInternal(type, mb);
		foreach (var @enum in assemblyBuilder.enums)
			DefineEnumsInternal(@enum, mb);
		foreach (var method in assemblyBuilder.methods)
			DefineMethodsInternal(method, mb);
		return buildContext;

		void DefineTypesInternal(TypeBuilder typeBuilder, Definer definer)
		{
			var typeAttributes = typeBuilder.visibility | typeBuilder.classOrNot | typeBuilder.abstractOrSealed;
			var tb = definer.DefineType(typeBuilder.name!, typeAttributes);
			builderMap.Add(typeBuilder, tb);
			foreach (var nestedTypeBuilder in typeBuilder.types)
				DefineTypesInternal(nestedTypeBuilder, tb);
			foreach (var @enum in typeBuilder.enums)
				DefineEnumsInternal(@enum, tb);
			foreach (var nestedMethodBuilder in typeBuilder.methods)
				DefineMethodsInternal(nestedMethodBuilder, tb);
			foreach (var field in typeBuilder.fields)
				DefineFieldInternal(field, tb);
			foreach (var property in typeBuilder.properties)
				DefinePropertyInternal(property, tb);
		}
		
		void DefineEnumsInternal(EnumBuilder enumBuilder, Definer definer)
		{
			if (String.IsNullOrEmpty(enumBuilder.name))
				throw new InvalidOperationException("Methods require a name.");
			var tb = definer.DefineType(enumBuilder.name!, enumBuilder.visibility | TypeAttributes.Sealed);
			builderMap.Add(enumBuilder, tb);
		}

		void DefineMethodsInternal(MethodBuilder methodBuilder, Definer definer)
		{
			if (String.IsNullOrEmpty(methodBuilder.name))
				throw new InvalidOperationException("Methods require a name.");
			var mb = definer.DefineMethod(methodBuilder.name!, methodBuilder.visibility | methodBuilder.storageType);
			builderMap.Add(methodBuilder, mb);
		}

		void DefineFieldInternal(FieldBuilder fieldBuilder, Sre.TypeBuilder typeBuilder)
		{
			if (String.IsNullOrEmpty(fieldBuilder.name))
				throw new InvalidOperationException("Methods require a name.");
			var type = fieldBuilder.typeRef is { } tr ? buildContext.ResolveType(tr) : throw new InvalidOperationException("Fields require a type.");
			var fb = typeBuilder.DefineField(fieldBuilder.name!, type, fieldBuilder.visibility | fieldBuilder.storageType);
			builderMap.Add(fieldBuilder, fb);
		}

		void DefinePropertyInternal(PropertyBuilder propertyBuilder, Sre.TypeBuilder typeBuilder)
		{
			if (String.IsNullOrEmpty(propertyBuilder.name))
				throw new InvalidOperationException("Methods require a name.");
			var type = propertyBuilder.typeRef is { } tr ? buildContext.ResolveType(tr) : throw new InvalidOperationException("Properties require a type.");
			var pb = typeBuilder.DefineProperty(propertyBuilder.name!, PropertyAttributes.None, CallingConventions.Standard, type, null);
			builderMap.Add(propertyBuilder, pb);
			if (propertyBuilder.setterBuilder != null)
			{
				var setter = typeBuilder.DefineMethod("set_" + propertyBuilder.name!, propertyBuilder.setterBuilder.visibility, type, null);
				pb.SetSetMethod(setter);
				builderMap.Add(propertyBuilder.setterBuilder, setter);
			}
			if (propertyBuilder.getterBuilder != null)
			{
				var getter = typeBuilder.DefineMethod("get_" + propertyBuilder.name!, propertyBuilder.getterBuilder.visibility, type, null);
				pb.SetGetMethod(getter);
				builderMap.Add(propertyBuilder.getterBuilder, getter);
			}
		}
	}

	internal static Sre.MethodBuilder Build(this MethodBuilder methodBuilder, BuildContext buildContext)
	{
		if (String.IsNullOrEmpty(methodBuilder.name))
			throw new InvalidOperationException("Methods require a name.");
		if (methodBuilder.returnTypeRef is null)
			throw new InvalidOperationException("Methods require a return type.");
		foreach (var parameterBuilder in methodBuilder.parameters)
		{
			if (parameterBuilder.typeRef is null)
				throw new InvalidOperationException("Parameters require a type.");
		}
		var mb = buildContext.GetBuilder(methodBuilder);
		mb.SetReturnType(buildContext.ResolveType(methodBuilder.returnTypeRef));
		var parameterTypes = methodBuilder.parameters.Select(x => buildContext.ResolveType(x.typeRef!)).ToArray();
		mb.SetParameters(parameterTypes);
		for (var index = 0; index < methodBuilder.parameters.Count; index++)
		{
			_ = mb.DefineParameter(index + 1, ParameterAttributes.None, methodBuilder.parameters[index].name);
		}

		var dictionary = methodBuilder.parameters.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? ParameterLookup(String s) => dictionary.TryGetValue(s, out var index) ? index : null;
		var locals = methodBuilder.locals.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? LocalLookup(String s) => locals.TryGetValue(s, out var index) ? index : null;
		var body = new BodyBuilder(mb.GetILGenerator(), buildContext, ParameterLookup, LocalLookup);
		methodBuilder.bodyActions.ForEach(a => a.Invoke(body));

		return mb;
	}

	private static void Build(this TypeBuilder typeBuilder, BuildContext buildContext)
	{
		if (String.IsNullOrEmpty(typeBuilder.name))
			throw new InvalidOperationException("Types require a name.");
		var tb = buildContext.GetBuilder(typeBuilder);

		tb.SetParent(typeBuilder.baseTypeRef is {} btr ? buildContext.ResolveType(btr) : buildContext.GetCoreAssemblyType(typeof(Object)));
		foreach (var interfaceBuilder in typeBuilder.interfaces)
			tb.AddInterfaceImplementation(buildContext.ResolveType(interfaceBuilder));
		foreach (var nestedTypeBuilder in typeBuilder.types)
			nestedTypeBuilder.Build(buildContext);
		foreach (var nestedMethodBuilder in typeBuilder.methods)
			nestedMethodBuilder.Build(buildContext);
		foreach (var enumBuilder in typeBuilder.enums)
			enumBuilder.Build(buildContext);
		foreach (var fieldBuilder in typeBuilder.fields)
		{
			
		}
		
		tb.CreateType();
	}

	internal static void Build(this EnumBuilder enumBuilder, BuildContext buildContext)
	{
		if (String.IsNullOrEmpty(enumBuilder.name))
			throw new InvalidOperationException("Enums require a name.");
		var eb = buildContext.GetBuilder(enumBuilder);
		eb.SetParent(buildContext.GetCoreAssemblyType(typeof(Enum)));
		eb.DefineField("value__", buildContext.GetCoreAssemblyType(enumBuilder.underlyingType), FieldAttributes.Private | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
		foreach (var enumBuilderLiteral in enumBuilder.literals)
		{
			if (String.IsNullOrEmpty(enumBuilderLiteral.name))
				throw new InvalidOperationException("Enum literals require a name.");
			if (enumBuilderLiteral.value == null)
				throw new InvalidOperationException("Enum literals require a value.");
			var fb = eb.DefineField(enumBuilderLiteral.name, eb, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
			fb.SetConstant(enumBuilderLiteral.value);
		}
		eb.CreateType();
	}

	private sealed class Definer
	{
		public static implicit operator Definer(Sre.ModuleBuilder x) => new((name, attributes) => x.DefineGlobalMethod(name, attributes, null, null), x.DefineType);
		public static implicit operator Definer(Sre.TypeBuilder x) => new(x.DefineMethod, x.DefineNestedType);
	
		public delegate Sre.MethodBuilder MethodDefiner(String name, MethodAttributes attributes);
		public delegate Sre.TypeBuilder TypeDefiner(String name, TypeAttributes attributes);
		
		public MethodDefiner DefineMethod { get; }
		public TypeDefiner DefineType { get; }
	
		private Definer(MethodDefiner defineMethod, TypeDefiner defineType) => (DefineMethod, DefineType) = (defineMethod, defineType);
	}
}