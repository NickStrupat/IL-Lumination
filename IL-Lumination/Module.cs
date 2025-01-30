using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static System.Reflection.Emit.AssemblyBuilderAccess;

[assembly: InternalsVisibleTo("Unit-Tests")]

namespace Illumination;

file static class ListExtensions
{
	public static T AddAndRet<T>(this List<T> list, T item) { list.Add(item); return item; }
	public static T Add<T>(this IReadOnlyList<T> list, T item) => Unsafe.As<List<T>>(list).AddAndRet(item);
	
	public static T AddAndRet<T>(this Dictionary<String, T> dictionary, String key, T value) { dictionary.Add(key, value); return value; }
}

public sealed class XAssembly(String name)
{
	internal readonly AssemblyBuilder AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new(name), RunAndCollect);

	public String Name => name;
	
	private readonly Dictionary<String, XModule> modules = new();
	public IReadOnlyDictionary<String, XModule> Modules => modules;

	public XModule Module(String name)
	{
		var value = new XModule(AssemblyBuilder.DefineDynamicModule(name));
		modules.Add(name, value);
		return value;
	}
}

public sealed class XModule(ModuleBuilder moduleBuilder)
{
	public String Name => moduleBuilder.Name;
	public IReadOnlyList<XType> Types { get; } = new List<XType>();
	public IReadOnlyList<XMethod> Methods { get; } = new List<XMethod>();

	public XType Type(String name, TypeAttributes attributes, Type? baseType = null, Type[]? interfaces = null) =>
		Types.Add(new(moduleBuilder.DefineType(name, attributes, baseType, interfaces)));

	public XMethod Method(String name, MethodAttributes attributes, Type? @return = null, Type[]? @params = null) =>
		Methods.Add(new(moduleBuilder.DefineGlobalMethod(name, attributes, @return, @params)));
}

public sealed class XType(TypeBuilder typeBuilder)
{
	public IReadOnlyList<XType> Types { get; } = new List<XType>();
	public IReadOnlyList<XMethod> Methods { get; } = new List<XMethod>();

	public XType Type(String name, TypeAttributes attributes, Type? baseType = null, Type[]? interfaces = null) =>
		Types.Add(new(typeBuilder.DefineNestedType(name, attributes, baseType, interfaces)));

	public XMethod Method(String name, MethodAttributes attributes, Type? returnType = null, Type[]? parameterTypes = null) =>
		Methods.Add(new(typeBuilder.DefineMethod(name, attributes, returnType, parameterTypes)));
}

public sealed class XMethod(MethodBuilder methodBuilder)
{
	public IReadOnlyList<XParameter> Parameters { get; } = new List<XParameter>();
	public IReadOnlyList<XGenericTypeParameter> GenericParameters { get; } = new List<XGenericTypeParameter>();

	public XParameter Parameter(String name, ParameterAttributes attributes) =>
		Parameters.Add(new(methodBuilder.DefineParameter(Parameters.Count + 1, attributes, name)));
	
	// public XGenericTypeParameter GenericParameter(String name, GenericParameterAttributes attributes) =>
	// 	GenericParameters.Add(new(methodBuilder.defin(name, attributes)));
}

public class XGenericTypeParameter(GenericTypeParameterBuilder genericTypeParameterBuilder)
{
	
}

public sealed class XParameter(ParameterBuilder parameterBuilder)
{
	public String Name => parameterBuilder.Name ?? throw new("Parameter name is null");
	//public Type ParameterType => parameterBuilder.;
}
