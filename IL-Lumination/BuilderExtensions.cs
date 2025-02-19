// using System;
// using System.Reflection;
// using System.Reflection.Emit;
// using System.Runtime.InteropServices;
//
// namespace Illumination;
//
// public static class BuilderExtensions
// {
// 	// public static LazilyResolvedTypeBuilder AsLazilyResolved(this TypeBuilder typeBuilder) =>
// 	// 	typeBuilder as LazilyResolvedTypeBuilder ?? new(typeBuilder);
// 	
// 	public static LazilyResolvedTypeBuilder DefineLazilyResolvedType(this ModuleBuilder moduleBuilder, String name) =>
// 		new(moduleBuilder.DefineType(name));
// 	
// 	public static LazilyResolvedTypeBuilder DefineLazilyResolvedType(this ModuleBuilder moduleBuilder, String name, TypeAttributes visibility) =>
// 		new(moduleBuilder.DefineType(name, visibility));
// 	
// 	public static LazilyResolvedTypeBuilder DefineLazilyResolvedType(this ModuleBuilder moduleBuilder, String name, TypeAttributes visibility, Type? parent) =>
// 		new(moduleBuilder.DefineType(name, visibility, parent));
// 	
// 	public static LazilyResolvedTypeBuilder DefineLazilyResolvedType(this ModuleBuilder moduleBuilder, String name, TypeAttributes visibility, Type? parent, Type[]? interfaces) =>
// 		new(moduleBuilder.DefineType(name, visibility, parent, interfaces));
// 	
// 	public static LazilyResolvedTypeBuilder DefineLazilyResolvedType(this ModuleBuilder moduleBuilder, String name, TypeAttributes visibility, Type? parent, PackingSize packSize) =>
// 		new(moduleBuilder.DefineType(name, visibility, parent, packSize));
// 	
// 	public static LazilyResolvedTypeBuilder DefineLazilyResolvedType(this ModuleBuilder moduleBuilder, String name, TypeAttributes visibility, Type? parent, PackingSize packSize, Int32 typeSize) =>
// 		new(moduleBuilder.DefineType(name, visibility, parent, packSize, typeSize));
//
// 	public sealed class LazilyResolvedTypeBuilder : TypeBuilder
// 	{
// 		private readonly TypeBuilder tb;
//
// 		internal LazilyResolvedTypeBuilder(TypeBuilder tb) => this.tb = tb;
//
// 		protected override void AddInterfaceImplementationCore(Type interfaceType)
// 		{
// 			tb.AddInterfaceImplementation(interfaceType);
// 		}
//
// 		protected override TypeInfo CreateTypeInfoCore()
// 		{
// 			return tb.CreateTypeInfo();
// 		}
//
// 		protected override ConstructorBuilder DefineConstructorCore(MethodAttributes attributes,
// 			CallingConventions callingConvention,
// 			Type[]? parameterTypes,
// 			Type[][]? requiredCustomModifiers,
// 			Type[][]? optionalCustomModifiers)
// 		{
// 			return tb.DefineConstructor(attributes, callingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
// 		}
//
// 		protected override ConstructorBuilder DefineDefaultConstructorCore(MethodAttributes attributes)
// 		{
// 			return tb.DefineDefaultConstructor(attributes);
// 		}
//
// 		protected override EventBuilder DefineEventCore(String name, EventAttributes attributes, Type eventtype)
// 		{
// 			return tb.DefineEvent(name, attributes, eventtype);
// 		}
//
// 		protected override FieldBuilder DefineFieldCore(String fieldName,
// 			Type type,
// 			Type[]? requiredCustomModifiers,
// 			Type[]? optionalCustomModifiers,
// 			FieldAttributes attributes)
// 		{
// 			return tb.DefineField(fieldName, type, requiredCustomModifiers, optionalCustomModifiers, attributes);
// 		}
//
// 		protected override GenericTypeParameterBuilder[] DefineGenericParametersCore(params String[] names)
// 		{
// 			return tb.DefineGenericParameters(names);
// 		}
//
// 		protected override FieldBuilder DefineInitializedDataCore(String name, Byte[] data, FieldAttributes attributes)
// 		{
// 			return tb.DefineInitializedData(name, data, attributes);
// 		}
//
// 		protected override MethodBuilder DefineMethodCore(String name,
// 			MethodAttributes attributes,
// 			CallingConventions callingConvention,
// 			Type? returnType,
// 			Type[]? returnTypeRequiredCustomModifiers,
// 			Type[]? returnTypeOptionalCustomModifiers,
// 			Type[]? parameterTypes,
// 			Type[][]? parameterTypeRequiredCustomModifiers,
// 			Type[][]? parameterTypeOptionalCustomModifiers)
// 		{
// 			return tb.DefineMethod(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
// 		}
//
// 		protected override void DefineMethodOverrideCore(MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
// 		{
// 			tb.DefineMethodOverride(methodInfoBody, methodInfoDeclaration);
// 		}
//
// 		protected override TypeBuilder DefineNestedTypeCore(String name,
// 			TypeAttributes attr,
// 			Type? parent,
// 			Type[]? interfaces,
// 			PackingSize packSize,
// 			Int32 typeSize)
// 		{
// 			return interfaces is null
// 				? tb.DefineNestedType(name, attr, parent, packSize, typeSize)
// 				: tb.DefineNestedType(name, attr, parent, interfaces);
// 		}
//
// 		protected override MethodBuilder DefinePInvokeMethodCore(String name,
// 			String dllName,
// 			String entryName,
// 			MethodAttributes attributes,
// 			CallingConventions callingConvention,
// 			Type? returnType,
// 			Type[]? returnTypeRequiredCustomModifiers,
// 			Type[]? returnTypeOptionalCustomModifiers,
// 			Type[]? parameterTypes,
// 			Type[][]? parameterTypeRequiredCustomModifiers,
// 			Type[][]? parameterTypeOptionalCustomModifiers,
// 			CallingConvention nativeCallConv,
// 			CharSet nativeCharSet)
// 		{
// 			return tb.DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, nativeCallConv, nativeCharSet);
// 		}
//
// 		protected override PropertyBuilder DefinePropertyCore(String name,
// 			PropertyAttributes attributes,
// 			CallingConventions callingConvention,
// 			Type? returnType,
// 			Type[]? returnTypeRequiredCustomModifiers,
// 			Type[]? returnTypeOptionalCustomModifiers,
// 			Type[]? parameterTypes,
// 			Type[][]? parameterTypeRequiredCustomModifiers,
// 			Type[][]? parameterTypeOptionalCustomModifiers)
// 		{
// 			return tb.DefineProperty(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
// 		}
//
// 		protected override ConstructorBuilder DefineTypeInitializerCore()
// 		{
// 			return tb.DefineTypeInitializer();
// 		}
//
// 		protected override FieldBuilder DefineUninitializedDataCore(String name, Int32 size, FieldAttributes attributes)
// 		{
// 			return tb.DefineUninitializedData(name, size, attributes);
// 		}
//
// 		protected override Boolean IsCreatedCore()
// 		{
// 			return tb.IsCreated();
// 		}
//
// 		protected override void SetCustomAttributeCore(ConstructorInfo con, ReadOnlySpan<Byte> binaryAttribute)
// 		{
// 			tb.SetCustomAttribute(con, binaryAttribute.ToArray());
// 		}
//
// 		protected override void SetParentCore(Type? parent)
// 		{
// 			tb.SetParent(parent);
// 		}
//
// 		protected override PackingSize PackingSizeCore => tb.PackingSize;
//
// 		protected override Int32 SizeCore => tb.Size;
// 	}
// }