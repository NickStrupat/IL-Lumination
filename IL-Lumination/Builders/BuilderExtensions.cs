using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static System.Reflection.Emit.AssemblyBuilderAccess;
using Sre = System.Reflection.Emit;

namespace Illumination;

public static class BuilderExtensions
{
	public static Assembly Create(this AssemblyBuilder assemblyBuilder)
	{
		var ab = Sre.AssemblyBuilder.DefineDynamicAssembly(new(assemblyBuilder.name ?? Guid.NewGuid().ToString("N")), RunAndCollect);
		assemblyBuilder.Build(ab, out _);
		return ab;
	}

	public static void Save(this AssemblyBuilder assemblyBuilder, String path)
	{
		if (String.IsNullOrEmpty(assemblyBuilder.name))
			throw new InvalidOperationException("Saving an assembly requires a name.");
		var ab = new Sre.PersistedAssemblyBuilder(new(assemblyBuilder.name), typeof(Object).Assembly);
		assemblyBuilder.Build(ab, out var entryPoint);
		ab.Save(path, entryPoint);
	}

	private static void Build(
		this AssemblyBuilder assemblyBuilder,
		Sre.AssemblyBuilder ab,
		out MethodInfo? entryPoint)
	{
		var module = ab.DefineDynamicModule("<Module>"); // only one module is allowed since .NET 5 (https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.assemblybuilder.definedynamicmodule?view=net-5.0#:~:text=Remarks)

		var typez = assemblyBuilder.typeBuildersWhoseBaseTypeOrInterfacesReferenceAtLeastOneTypeBuilder.ToList();
		typez.Sort((a, b) => a.dependencies.Count < b.dependencies.Count ? -1 : 1);
		
		var typeBuilderMap = DefineTypes(assemblyBuilder, module);
		
		BuildContext buildContext = new();
		
		foreach (var methodBuilder in assemblyBuilder.methods)
		{
			var mb = methodBuilder.Build(module);
			buildContext.MethodBuilderMap.Add(methodBuilder, mb);
		}
		module.CreateGlobalFunctions();

		foreach (var type in assemblyBuilder.types)
		{
			if (String.IsNullOrEmpty(type.name))
				throw new InvalidOperationException("Types require a name.");
			
			var tb = module.DefineType(type.name, type.visibility);

			//type.Build(mb);
			tb.CreateType();
		}

		// foreach (var @enum in assemblyBuilder.enums)
		// {
		// 	var eb = @enum.Build((n, v, u) => module.DefineEnum(n, v, u));
		// 	eb.CreateType();
		// }

		entryPoint = assemblyBuilder.entryPoint is {} ep ? buildContext.Resolve(ep) : null;
	}

	private static IReadOnlyDictionary<TypeBuilder, Sre.TypeBuilder> DefineTypes(
		AssemblyBuilder assemblyBuilder,
		Sre.ModuleBuilder mb)
	{
		var typeBuilderMap = new Dictionary<TypeBuilder, Sre.TypeBuilder>();

		foreach (var type in assemblyBuilder.types)
		{
			
		}
		
		return typeBuilderMap;

		// void DefineTypesInternal(IHasTypeBuilderDependenciess hasTypeBuilders, TypeDefiner typeDefiner)
		// {
		// 	foreach (var typeBuilder in hasTypeBuilders.TypeBuilders)
		// 	{
		// 		typeDefiner.Define()
		// 	}
		// }
	}

	internal static Sre.MethodBuilder Build(this MethodBuilder methodBuilder, MethodDefiner methodDefiner)
	{
		if (String.IsNullOrEmpty(methodBuilder.name))
			throw new InvalidOperationException("Methods require a name.");
		foreach (var parameterBuilder in methodBuilder.parameters)
		{
			if (parameterBuilder.typeRef == null)
				throw new InvalidOperationException("Parameters require a type.");
		}
		var mb = methodDefiner.Define(
			methodBuilder.name!,
			methodBuilder.visibility | MethodAttributes.Static,
			methodBuilder.returnTypeRef!.Resolve(),
			methodBuilder.parameters.Select(x => x.typeRef!.Resolve()).ToArray()
		);
		for (var index = 0; index < methodBuilder.parameters.Count; index++)
		{
			var pb = mb.DefineParameter(index + 1, ParameterAttributes.None, methodBuilder.parameters[index].name);
		}

		var dictionary = methodBuilder.parameters.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? ParameterLookup(String s) => dictionary.TryGetValue(s, out var index) ? index : null;
		var body = new BodyBuilder(mb.GetILGenerator(), ParameterLookup);

		methodBuilder.bodyActions.ForEach(a => a.Invoke(body));

		return mb;
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

	private sealed class BuildContext
	{
		public Queue<TypeBuilder> TypeQueue { get; } = new();
		public Queue<MethodBuilder> MethodQueue { get; } = new();
		public Dictionary<TypeBuilder, Sre.TypeBuilder> TypeBuilderMap { get; } = new();
		public Dictionary<MethodBuilder, Sre.MethodBuilder> MethodBuilderMap { get; } = new();
		
		public Maybe<Type> TryResolve(TypeRef typeRef) => typeRef switch
		{
			TypeRef.Declared declared => new(declared.Type),
			TypeRef.Builder builder => new(TypeBuilderMap[builder.TypeBuilder]),
			_ => new()
		};
		
		public Maybe<MethodInfo> TryResolve(MethodRef methodRef) => methodRef switch
		{
			MethodRef.Declared declared => new(declared.MethodInfo),
			MethodRef.Builder builder => new(MethodBuilderMap[builder.MethodBuilder]),
			_ => new()
		};

		public Type Resolve(TypeRef typeRef) => typeRef switch
		{
			TypeRef.Declared declared => declared.Type,
			TypeRef.Builder builder => TypeBuilderMap[builder.TypeBuilder],
			_ => throw new ArgumentOutOfRangeException(nameof(typeRef))
		};

		public MethodInfo Resolve(MethodRef methodRef) => methodRef switch
		{
			MethodRef.Declared declared => declared.MethodInfo,
			MethodRef.Builder builder => MethodBuilderMap[builder.MethodBuilder],
			_ => throw new ArgumentOutOfRangeException(nameof(methodRef))
		};
	}
	
	internal sealed class MethodDefiner
	{
		public static implicit operator MethodDefiner(Sre.ModuleBuilder x) => new(x.DefineGlobalMethod);
		public static implicit operator MethodDefiner(Sre.TypeBuilder x) => new(x.DefineMethod);
	
		public delegate Sre.MethodBuilder Definer(String name, MethodAttributes attributes, Type returnType, Type[] parameterTypes);
		
		public Definer Define { get; }
	
		private MethodDefiner(Definer define) => Define = define;
	}

	internal sealed class TypeDefiner
	{
		public static implicit operator TypeDefiner(Sre.ModuleBuilder x) => new(x.DefineType);
		public static implicit operator TypeDefiner(Sre.TypeBuilder x) => new(x.DefineNestedType);
	
		public delegate Sre.TypeBuilder Definer(String name, TypeAttributes attributes, Type? baseType, Type[]? interfaces);
		
		public Definer Define { get; }
		
		private TypeDefiner(Definer define) => Define = define;
	}
}

internal static class TypeRefExtensions
{
	public static Type Resolve(this TypeRef typeRef, IReadOnlyDictionary<TypeBuilder, Sre.TypeBuilder> typeBuilderMap) => typeRef switch
	{
		TypeRef.Declared declared => declared.Type,
		TypeRef.Builder builder => typeBuilderMap[builder.TypeBuilder],
		_ => throw new ArgumentOutOfRangeException(nameof(typeRef))
	};
	
	public static IEnumerable<TypeBuilder> AsTypeBuilders(this TypeRef? tr) => tr is TypeRef.Builder(var tb) ? [tb] : [];
	public static IEnumerable<TypeBuilder> AsTypeBuilders(this IEnumerable<TypeRef> trs) => trs.SelectMany(x => x.AsTypeBuilders());
}