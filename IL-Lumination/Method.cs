// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using System.Reflection.Emit;
// using System.Runtime.CompilerServices;
// using System.Text.RegularExpressions;
//
// namespace IL_Lumination;
//
// using Cae = CallerArgumentExpressionAttribute;
//
// public ref struct Method(
// 	String name,
// 	Visibility visibility = Visibility.Private,
// 	MethodType methodType = MethodType.Instance,
// 	Virtuality virtuality = Virtuality.NonVirtual)
// {
// 	internal Type? ReturnType;
// 	internal readonly List<Parameter> ParameterTypes = new();
//
// 	public MethodWithParameter Param<T>(out Parameter parameter, [Cae(nameof(parameter))] String cae = "") => new(ref this, typeof(T), cae, out parameter);
// 	public MethodWithGenericParameter GenericParam(out GenericParameter genericParameter, GenericParameterAttributes gpa) => new(ref this, typeof(T), cae, out parameter);
//
// 	internal DynamicMethod Build()
// 	{
// 		var methodAttributes = (MethodAttributes)visibility | (MethodAttributes)methodType | (MethodAttributes)virtuality;
// 		var dm = new DynamicMethod(name, methodAttributes, CallingConventions.Standard, ReturnType, ParameterTypes.Select(x => x.Type).ToArray(), typeof(Method).Module, skipVisibility:false);
// 		return dm;
// 	}
// }
//
// public ref struct MethodWithReturnType
// {
// 	private Method method;
//
// 	public MethodWithReturnType(scoped ref Method method, Type returnType)
// 	{
// 		this.method = method;
// 		method.ReturnType = returnType;
// 	}
//
// 	public DynamicMethod Build() => method.Build();
// }
//
// public ref struct MethodWithParameter
// {
// 	private Method method;
//
// 	public MethodWithParameter(scoped ref Method method, Type type, String cae, out Parameter parameter)
// 	{
// 		this.method = method;
// 		parameter = ParameterHelper.CreateParameter(cae, type);
// 		method.ParameterTypes.Add(parameter);
// 	}
//
// 	public MethodWithParameter Param<T>(out Parameter parameter, [Cae(nameof(parameter))] String cae = "") => new(ref method, typeof(T), cae, out parameter);
// 	public MethodWithReturnType Returns<T>() => new(ref method, typeof(T));
// 	public DynamicMethod Build() => method.Build();
// }
//
// internal static partial class ParameterHelper
// {
// 	internal static Parameter CreateParameter(String cae, Type type)
// 	{
// 		var match = VarIdentifierRegex().Match(cae);
// 		if (!match.Success)
// 			throw new ArgumentException("Parameter must be specified as a variable declaration, e.g. `out var count`");
// 		var name = match.Groups[1].Value;
// 		return new(name, type);
// 	}
//
// 	[GeneratedRegex("""var\s([a-zA-Z_][a-zA-Z0-9_]*)""")]
// 	private static partial Regex VarIdentifierRegex();
// }
