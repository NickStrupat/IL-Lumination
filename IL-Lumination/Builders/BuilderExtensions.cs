using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static System.Reflection.Emit.AssemblyBuilderAccess;
using Sre = System.Reflection.Emit;
using Task = System.Threading.Tasks.Task;

namespace Illumination.Builders;

public static class BuilderExtensions
{

	private static MethodInfo GetCreatedMethodInfo(this Sre.MethodBuilder methodBuilder)
	{
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

		foreach (var type in assemblyBuilder.types)
			DefineTypesInternal(type, mb);
		foreach (var @enum in assemblyBuilder.enums)
			DefineEnumsInternal(@enum, mb);
		foreach (var method in assemblyBuilder.methods)
			DefineMethodInternal(method, mb);
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
			foreach (var constructorBuilder in typeBuilder.constructors)
				DefineConstructorInternal(constructorBuilder, tb);
			foreach (var nestedMethodBuilder in typeBuilder.methods)
				DefineMethodInternal(nestedMethodBuilder, tb);
			foreach (var field in typeBuilder.fields)
				DefineFieldInternal(field, tb);
			foreach (var property in typeBuilder.properties)
				DefinePropertyInternal(property, tb);
		}
		
		void DefineEnumsInternal(EnumBuilder enumBuilder, Definer definer)
		{
			if (String.IsNullOrEmpty(enumBuilder.name))
				throw new InvalidOperationException("EnumBuilder requires a name to be configured before building.");
			var tb = definer.DefineType(enumBuilder.name, enumBuilder.visibility | TypeAttributes.Sealed);
			builderMap.Add(enumBuilder, tb);
		}
		
		void DefineConstructorInternal(ConstructorBuilder constructorBuilder, Sre.TypeBuilder typeBuilder)
		{
			var parameterTypes = constructorBuilder.parameters.Select(x => buildContext.ResolveType(x.typeRef!)).ToArray();
			var cb = typeBuilder.DefineConstructor(constructorBuilder.visibility, CallingConventions.Standard, parameterTypes);
			builderMap.Add(constructorBuilder, cb);
		}

		void DefineMethodInternal(MethodBuilder methodBuilder, Definer definer)
		{
			if (String.IsNullOrEmpty(methodBuilder.name))
				throw new InvalidOperationException("Methods require a name.");
			var returnType = methodBuilder.returnTypeRef is { } rt ? buildContext.ResolveType(rt) : null;
			var parameterTypes = methodBuilder.parameters.Count > 0
				? methodBuilder.parameters.Select(x => buildContext.ResolveType(x.typeRef!)).ToArray()
				: null;
			var mb = definer.DefineMethod(methodBuilder.name!, methodBuilder.visibility | methodBuilder.storageType, returnType, parameterTypes);
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
		var mb = buildContext.GetBuilder(methodBuilder);

		var ilGenerator = mb.GetILGenerator();
		foreach (var local in methodBuilder.locals)
			ilGenerator.DeclareLocal(buildContext.ResolveType(local.typeRef!));

		var dictionary = methodBuilder.parameters.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? ParameterLookup(String s) => dictionary.TryGetValue(s, out var index) ? index : null;
		var locals = methodBuilder.locals.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? LocalLookup(String s) => locals.TryGetValue(s, out var index) ? index : null;
		var body = new BodyBuilder(ilGenerator, buildContext, ParameterLookup, LocalLookup);
		methodBuilder.bodyActions.ForEach(a => a.Invoke(body));

		return mb;
	}

	internal static void Build(this TypeBuilder typeBuilder, BuildContext buildContext)
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
		foreach (var constructorBuilder in typeBuilder.constructors)
			constructorBuilder.Build(buildContext);
		foreach (var propertyBuilder in typeBuilder.properties)
			propertyBuilder.BuildAccessors(buildContext);

		tb.CreateType();
	}

	internal static void Build(this ConstructorBuilder constructorBuilder, BuildContext buildContext)
	{
		var cb = buildContext.GetBuilder(constructorBuilder);
		for (var index = 0; index < constructorBuilder.parameters.Count; index++)
		{
			_ = cb.DefineParameter(index + 1, ParameterAttributes.None, constructorBuilder.parameters[index].name);
		}

		var ilGenerator = cb.GetILGenerator();
		foreach (var local in constructorBuilder.locals)
			ilGenerator.DeclareLocal(buildContext.ResolveType(local.typeRef!));

		var dictionary = constructorBuilder.parameters.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? ParameterLookup(String s) => dictionary.TryGetValue(s, out var index) ? index : null;
		var locals = constructorBuilder.locals.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? LocalLookup(String s) => locals.TryGetValue(s, out var index) ? index : null;
		var body = new BodyBuilder(ilGenerator, buildContext, ParameterLookup, LocalLookup);
		constructorBuilder.bodyActions.ForEach(a => a.Invoke(body));
	}

	internal static void BuildAccessors(this PropertyBuilder propertyBuilder, BuildContext buildContext)
	{
		if (propertyBuilder.getterBuilder is { } getter)
			BuildAccessor(getter, buildContext);
		if (propertyBuilder.setterBuilder is { } setter)
			BuildAccessor(setter, buildContext);
	}

	private static void BuildAccessor<T>(AccessorBuilder<T> accessorBuilder, BuildContext buildContext) where T : AccessorBuilder<T>
	{
		var mb = buildContext.GetBuilder(accessorBuilder);
		var ilGenerator = mb.GetILGenerator();
		foreach (var local in accessorBuilder.locals)
			ilGenerator.DeclareLocal(buildContext.ResolveType(local.typeRef!));

		var locals = accessorBuilder.locals.Where(x => x.name is not null).ToDictionary(x => x.name!, x => x.index);
		Int16? LocalLookup(String s) => locals.TryGetValue(s, out var index) ? index : null;
		var body = new BodyBuilder(ilGenerator, buildContext, _ => null, LocalLookup);
		accessorBuilder.bodyActions.ForEach(a => a.Invoke(body));
	}

	internal static void Build(this EnumBuilder enumBuilder, BuildContext buildContext)
	{
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
}

internal sealed class Definer
{
	public static implicit operator Definer(Sre.ModuleBuilder x) => new(x.DefineGlobalMethod, x.DefineType);
	public static implicit operator Definer(Sre.TypeBuilder x) => new(x.DefineMethod, x.DefineNestedType);

	public delegate Sre.MethodBuilder MethodDefiner(String name, MethodAttributes attributes, Type? returnType, Type[]? parameterTypes);
	public delegate Sre.TypeBuilder TypeDefiner(String name, TypeAttributes attributes);

	public MethodDefiner DefineMethod { get; }
	public TypeDefiner DefineType { get; }

	private Definer(MethodDefiner defineMethod, TypeDefiner defineType) => (DefineMethod, DefineType) = (defineMethod, defineType);
}