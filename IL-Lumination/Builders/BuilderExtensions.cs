using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static System.Reflection.Emit.AssemblyBuilderAccess;
using Sre = System.Reflection.Emit;

namespace Illumination.Builders;

public static class BuilderExtensions
{
	public static Assembly Create(this AssemblyBuilder assemblyBuilder)
	{
		var ab = Sre.AssemblyBuilder.DefineDynamicAssembly(new(assemblyBuilder.name ?? Guid.NewGuid().ToString("N")), RunAndCollect);
		assemblyBuilder.Build(ab, typeof(Object).Assembly, out _);
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
		var refAssembliesPath = RuntimeEnvironment.GetRuntimeDirectory();
		PathAssemblyResolver resolver = new PathAssemblyResolver(Directory.GetFiles(refAssembliesPath, "*.dll"));
		var mlc = new MetadataLoadContext(resolver);
		coreAssembly = mlc.CoreAssembly ?? throw new Exception("Core assembly not found.");
		return new(new(assemblyBuilder.name ?? Path.GetFileNameWithoutExtension(path)), coreAssembly);
	}

	private static void Build(
		this AssemblyBuilder assemblyBuilder,
		Sre.AssemblyBuilder ab,
		Assembly coreAssembly,
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

		entryPoint = assemblyBuilder.entryPoint is {} ep ? buildContext.ResolveMethod(ep) : null;
	}

	private static BuildContext DefineAllTypesAndMethods(
		AssemblyBuilder assemblyBuilder,
		Sre.ModuleBuilder mb,
		Assembly coreAssembly)
	{
		var typeBuilderMap = new Dictionary<TypeBuilder, Sre.TypeBuilder>();
		var enumBuilderMap = new Dictionary<EnumBuilder, Sre.TypeBuilder>();
		var methodBuilderMap = new Dictionary<MethodBuilder, Sre.MethodBuilder>();

		foreach (var type in assemblyBuilder.types)
			DefineTypesInternal(type, mb);
		foreach (var @enum in assemblyBuilder.enums)
			DefineEnumsInternal(@enum, mb);
		foreach (var method in assemblyBuilder.methods)
			DefineMethodsInternal(method, mb);
		
		return new(typeBuilderMap, enumBuilderMap, methodBuilderMap, coreAssembly);

		void DefineTypesInternal(TypeBuilder typeBuilder, Definer definer)
		{
			var tb = definer.DefineType(typeBuilder.name!, typeBuilder.visibility);
			typeBuilderMap.Add(typeBuilder, tb);
			foreach (var nestedTypeBuilder in typeBuilder.types)
				DefineTypesInternal(nestedTypeBuilder, tb);
			foreach (var @enum in typeBuilder.enums)
				DefineEnumsInternal(@enum, tb);
			foreach (var nestedMethodBuilder in typeBuilder.methods)
				DefineMethodsInternal(nestedMethodBuilder, tb);
		}
		
		void DefineEnumsInternal(EnumBuilder enumBuilder, Definer definer)
		{
			var tb = definer.DefineType(enumBuilder.name!, enumBuilder.visibility | TypeAttributes.Sealed);
			enumBuilderMap.Add(enumBuilder, tb);
		}

		void DefineMethodsInternal(MethodBuilder methodBuilder, Definer definer)
		{
			var mb = definer.DefineMethod(methodBuilder.name!, methodBuilder.visibility | methodBuilder.storageType);
			methodBuilderMap.Add(methodBuilder, mb);
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
		var mb = buildContext.ResolveMethodBuilder(methodBuilder);
		mb.SetReturnType(buildContext.ResolveType(methodBuilder.returnTypeRef!));
		mb.SetParameters(methodBuilder.parameters.Select(x => buildContext.ResolveType(x.typeRef!)).ToArray());
		for (var index = 0; index < methodBuilder.parameters.Count; index++)
		{
			_ = mb.DefineParameter(index + 1, ParameterAttributes.None, methodBuilder.parameters[index].name);
		}

		var dictionary = methodBuilder.parameters.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? ParameterLookup(String s) => dictionary.TryGetValue(s, out var index) ? index : null;
		var locals = methodBuilder.locals.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? LocalLookup(String s) => locals.TryGetValue(s, out var index) ? index : null;
		var body = new BodyBuilder(mb.GetILGenerator(), ParameterLookup, LocalLookup);
		methodBuilder.bodyActions.ForEach(a => a.Invoke(body));

		return mb;
	}

	private static void Build(this TypeBuilder typeBuilder, BuildContext buildContext)
	{
		if (String.IsNullOrEmpty(typeBuilder.name))
			throw new InvalidOperationException("Types require a name.");
		var tb = buildContext.ResolveTypeBuilder(typeBuilder);

		tb.SetParent(typeBuilder.baseTypeRef is {} btr ? buildContext.ResolveType(btr) : buildContext.GetCoreAssemblyType(typeof(Object)));
		foreach (var interfaceBuilder in typeBuilder.interfaces)
			tb.AddInterfaceImplementation(buildContext.ResolveType(interfaceBuilder));
		foreach (var nestedTypeBuilder in typeBuilder.types)
			nestedTypeBuilder.Build(buildContext);
		foreach (var nestedMethodBuilder in typeBuilder.methods)
			nestedMethodBuilder.Build(buildContext);
		foreach (var enumBuilder in typeBuilder.enums)
			enumBuilder.Build(buildContext);
		
		tb.CreateType();
	}

	internal static void Build(this EnumBuilder enumBuilder, BuildContext buildContext)
	{
		if (String.IsNullOrEmpty(enumBuilder.name))
			throw new InvalidOperationException("Enums require a name.");
		var eb = buildContext.ResolveTypeBuilder(enumBuilder);
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

	internal sealed class BuildContext(
		IReadOnlyDictionary<TypeBuilder, Sre.TypeBuilder> typeBuilderMap,
		IReadOnlyDictionary<EnumBuilder, Sre.TypeBuilder> enumBuilderMap,
		IReadOnlyDictionary<MethodBuilder, Sre.MethodBuilder> methodBuilderMap,
		Assembly coreAssembly
		)
	{
		public Sre.TypeBuilder ResolveTypeBuilder(TypeBuilder typeBuilder) => typeBuilderMap[typeBuilder];
		public Sre.TypeBuilder ResolveTypeBuilder(EnumBuilder enumBuilder) => enumBuilderMap[enumBuilder];
		public Sre.MethodBuilder ResolveMethodBuilder(MethodBuilder methodBuilder) => methodBuilderMap[methodBuilder];
		
		public Type GetCoreAssemblyType(Type type) => type.Assembly == coreAssembly ? type : coreAssembly.GetType(type.FullName!) ?? type;

		public Type ResolveType(TypeRef typeRef) => typeRef switch
		{
			TypeRef.Declared d => GetCoreAssemblyType(d.Type),
			TypeRef.Builder b => typeBuilderMap[b.TypeBuilder],
			_ => throw new ArgumentOutOfRangeException(nameof(typeRef))
		};

		public MethodInfo ResolveMethod(MethodRef methodRef) => methodRef switch
		{
			MethodRef.Declared d => d.MethodInfo,
			MethodRef.Builder b => methodBuilderMap[b.MethodBuilder],
			_ => throw new ArgumentOutOfRangeException(nameof(methodRef))
		};
	}
	
	internal sealed class Definer
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

// internal static class TypeRefExtensions
// {
// 	public static Type Resolve(this TypeRef typeRef, BuilderExtensions.BuildContext buildContext) => typeRef switch
// 	{
// 		TypeRef.Declared declared => declared.Type,
// 		TypeRef.Builder builder => buildContext.ResolveType(builder),
// 		_ => throw new ArgumentOutOfRangeException(nameof(typeRef))
// 	};
// 	
// 	public static Type Resolve(this MethodRef methodRef, BuilderExtensions.TypeBuilderLookup tbl) => typeRef switch
// 	{
// 		TypeRef.Declared declared => declared.Type,
// 		TypeRef.Builder builder => tbl[builder.TypeBuilder],
// 		_ => throw new ArgumentOutOfRangeException(nameof(typeRef))
// 	};
// }