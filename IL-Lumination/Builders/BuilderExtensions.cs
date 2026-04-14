using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Sre = System.Reflection.Emit;

namespace Illumination.Builders;

public static class BuilderExtensions
{
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
		foreach (var eventBuilder in typeBuilder.events)
		{
			if (eventBuilder.addMethodBuilder is { } addBuilder)
				addBuilder.Build(buildContext);
			if (eventBuilder.removeMethodBuilder is { } removeBuilder)
				removeBuilder.Build(buildContext);
		}

		tb.CreateType();
	}

	internal static void Build(this ConstructorBuilder constructorBuilder, BuildContext buildContext)
	{
		var cb = buildContext.GetBuilder(constructorBuilder);
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
		if (enumBuilder.literals.DistinctBy(x => x.name).Count() != enumBuilder.literals.Count)
			throw new InvalidOperationException("Enum literals must have unique names.");
		var eb = buildContext.GetBuilder(enumBuilder);
		eb.SetParent(buildContext.GetCoreAssemblyType(typeof(Enum)));
		eb.DefineField("value__", buildContext.GetCoreAssemblyType(enumBuilder.underlyingType), FieldAttributes.Private | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
		// enumBuilder.literals.Sort((x, y) => y.value - x.value);
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