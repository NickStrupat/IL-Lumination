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
		
		var tbl = DefineAllTypes(assemblyBuilder, module);
		
		BuildContext buildContext = new();
		
		foreach (var methodBuilder in assemblyBuilder.methods)
		{
			var mb = methodBuilder.Build(module, tbl);
			buildContext.MethodBuilderMap.Add(methodBuilder, mb);
		}
		module.CreateGlobalFunctions();

		foreach (var type in assemblyBuilder.types)
		{
			type.Build(tbl);
		}

		foreach (var @enum in assemblyBuilder.enums)
		{
			@enum.Build(module);
		}

		entryPoint = assemblyBuilder.entryPoint is {} ep ? buildContext.Resolve(ep) : null;
	}

	private static TypeBuilderLookup DefineAllTypes(
		AssemblyBuilder assemblyBuilder,
		Sre.ModuleBuilder mb)
	{
		var typeBuilderMap = new Dictionary<TypeBuilder, Sre.TypeBuilder>();

		foreach (var type in assemblyBuilder.types)
		{
			DefineTypesInternal(type, mb);
		}
		
		return new(typeBuilderMap);

		void DefineTypesInternal(TypeBuilder typeBuilder, TypeDefiner typeDefiner)
		{
			var tb = typeDefiner.Define(typeBuilder.name!, typeBuilder.visibility);
			typeBuilderMap.Add(typeBuilder, tb);
			foreach (var nestedTypeBuilder in typeBuilder.types)
			{
				DefineTypesInternal(nestedTypeBuilder, tb);
			}
		}
	}

	internal static Sre.MethodBuilder Build(this MethodBuilder methodBuilder, MethodDefiner methodDefiner, TypeBuilderLookup tbl)
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
			methodBuilder.visibility | MethodAttributes.Static
		);
		mb.SetReturnType(methodBuilder.returnTypeRef!.Resolve(tbl));
		mb.SetParameters(methodBuilder.parameters.Select(x => x.typeRef!.Resolve(tbl)).ToArray());
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

	private static void Build(this TypeBuilder typeBuilder, TypeBuilderLookup tbl)
	{
		if (String.IsNullOrEmpty(typeBuilder.name))
			throw new InvalidOperationException("Types require a name.");
		var tb = tbl[typeBuilder];
		
		if (typeBuilder.baseTypeRef is {} btr)
			tb.SetParent(btr.Resolve(tbl));
		foreach (var interfaceBuilder in typeBuilder.interfaces)
			tb.AddInterfaceImplementation(interfaceBuilder.Resolve(tbl));
		// foreach (var enumBuilder in typeBuilder.enums)
		// 	enumBuilder.Build(tb, tbl);
		
		tb.CreateType();
	}

	internal static void Build(this EnumBuilder enumBuilder, TypeDefiner typeDefiner)
	{
		if (String.IsNullOrEmpty(enumBuilder.name))
			throw new InvalidOperationException("Enums require a name.");
		var eb = typeDefiner.Define(enumBuilder.name!, enumBuilder.visibility | TypeAttributes.Sealed);
		eb.SetParent(typeof(Enum));
		eb.DefineField("value__", enumBuilder.underlyingType, FieldAttributes.Private | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
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
		public static implicit operator MethodDefiner(Sre.ModuleBuilder x) => new((name, attributes) => x.DefineGlobalMethod(name, attributes, null, null));
		public static implicit operator MethodDefiner(Sre.TypeBuilder x) => new(x.DefineMethod);
	
		public delegate Sre.MethodBuilder Definer(String name, MethodAttributes attributes);
		
		public Definer Define { get; }
	
		private MethodDefiner(Definer define) => Define = define;
	}

	internal sealed class TypeDefiner
	{
		public static implicit operator TypeDefiner(Sre.ModuleBuilder x) => new(x.DefineType);
		public static implicit operator TypeDefiner(Sre.TypeBuilder x) => new(x.DefineNestedType);
	
		public delegate Sre.TypeBuilder Definer(String name, TypeAttributes attributes);
		
		public Definer Define { get; }
		
		private TypeDefiner(Definer define) => Define = define;
	}

	internal readonly struct TypeBuilderLookup(IReadOnlyDictionary<TypeBuilder, Sre.TypeBuilder> map)
	{
		private readonly IReadOnlyDictionary<TypeBuilder, Sre.TypeBuilder> map = map;
		public Sre.TypeBuilder this[TypeBuilder typeBuilder] => map[typeBuilder];
	}
}

internal static class TypeRefExtensions
{
	public static Type Resolve(this TypeRef typeRef, BuilderExtensions.TypeBuilderLookup tbl) => typeRef switch
	{
		TypeRef.Declared declared => declared.Type,
		TypeRef.Builder builder => tbl[builder.TypeBuilder],
		_ => throw new ArgumentOutOfRangeException(nameof(typeRef))
	};
	
	public static IEnumerable<TypeBuilder> AsTypeBuilders(this TypeRef? tr) => tr is TypeRef.Builder(var tb) ? [tb] : [];
	public static IEnumerable<TypeBuilder> AsTypeBuilders(this IEnumerable<TypeRef> trs) => trs.SelectMany(x => x.AsTypeBuilders());
}