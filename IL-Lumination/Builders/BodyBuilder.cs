using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Illumination.Builders;

public sealed class BodyBuilder : BodyBase<BodyBuilder>
{
	private readonly BuildContext buildContext;
	private readonly Func<String, Int16?> parameterNameToIndexLookup;

	internal BodyBuilder(
		ILGenerator ilGenerator
		, BuildContext buildContext
		, Func<String, Int16?> parameterNameToIndexLookup
		) : base(ilGenerator)
	{
		this.buildContext = buildContext;
		this.parameterNameToIndexLookup = parameterNameToIndexLookup;
	}

	/// <inheritdoc cref="BodyBase{TBody}.Ldarg(short)"/>
	public BodyBuilder Ldarg(ParameterBuilderBase parameter) => base.Ldarg(parameter.index);
	/// <inheritdoc cref="BodyBase{TBody}.Ldarg(short)"/>
	public BodyBuilder Ldarg(String name) => base.Ldarg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));

	/// <inheritdoc cref="BodyBase{TBody}.Starg(short)"/>
	public BodyBuilder Starg(ParameterBuilderBase parameter) => base.Starg(parameter.index);
	/// <inheritdoc cref="BodyBase{TBody}.Starg(short)"/>
	public BodyBuilder Starg(String name) => base.Starg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));

	/// <inheritdoc cref="BodyBase{TBody}.Ldfld(FieldInfo)"/>
	public BodyBuilder Ldfld(FieldBuilder field) => base.Ldfld(buildContext.GetBuilder(field));

	/// <inheritdoc cref="BodyBase{TBody}.Stfld(FieldInfo)"/>
	public BodyBuilder Stfld(FieldBuilder field) => base.Stfld(buildContext.GetBuilder(field));

	/// <inheritdoc cref="BodyBase{TBody}.Ldsfld(FieldInfo)"/>
	public BodyBuilder Ldsfld(FieldBuilder field) => base.Ldsfld(buildContext.GetBuilder(field));
	/// <inheritdoc cref="BodyBase{TBody}.Stsfld(FieldInfo)"/>
	public BodyBuilder Stsfld(FieldBuilder field) => base.Stsfld(buildContext.GetBuilder(field));
	/// <inheritdoc cref="BodyBase{TBody}.Ldflda(FieldInfo)"/>
	public BodyBuilder Ldflda(FieldBuilder field) => base.Ldflda(buildContext.GetBuilder(field));
	/// <inheritdoc cref="BodyBase{TBody}.Ldsflda(FieldInfo)"/>
	public BodyBuilder Ldsflda(FieldBuilder field) => base.Ldsflda(buildContext.GetBuilder(field));

	/// <inheritdoc cref="BodyBase{TBody}.Call(MethodInfo)"/>
	public BodyBuilder Call(MethodBuilder method) => base.Call(buildContext.GetBuilder(method));
	/// <inheritdoc cref="BodyBase{TBody}.Callvirt(MethodInfo)"/>
	public BodyBuilder Callvirt(MethodBuilder method) => base.Callvirt(buildContext.GetBuilder(method));
	/// <inheritdoc cref="BodyBase{TBody}.Ldftn(MethodInfo)"/>
	public BodyBuilder Ldftn(MethodBuilder method) => base.Ldftn(buildContext.GetBuilder(method));
	/// <inheritdoc cref="BodyBase{TBody}.Newobj(ConstructorInfo)"/>
	public BodyBuilder Newobj(ConstructorBuilder constructor) => base.Newobj(buildContext.GetBuilder(constructor));

	/// <inheritdoc cref="BodyBase{TBody}.Ldtoken(FieldInfo)"/>
	public BodyBuilder Ldtoken(FieldBuilder field) => base.Ldtoken(buildContext.GetBuilder(field));
	/// <inheritdoc cref="BodyBase{TBody}.Ldtoken(MethodInfo)"/>
	public BodyBuilder Ldtoken(MethodBuilder method) => base.Ldtoken(buildContext.GetBuilder(method));
	/// <inheritdoc cref="BodyBase{TBody}.Ldtoken(Type)"/>
	public BodyBuilder Ldtoken(TypeBuilder type) => base.Ldtoken(buildContext.GetBuilder(type));

	private protected override BodyBuilder Emit(OpCode opcode, MethodInfo meth) => base.Emit(opcode, buildContext.GetCoreAssemblyMethod(meth));
	private protected override BodyBuilder Emit(OpCode opcode, Type type) => base.Emit(opcode, buildContext.GetCoreAssemblyType(type));
}
