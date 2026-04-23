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

	public BodyBuilder Ldarg(ParameterBuilderBase parameter) => base.Ldarg(parameter.index);
	public BodyBuilder Ldarg(String name) => base.Ldarg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));

	public BodyBuilder Starg(ParameterBuilderBase parameter) => base.Starg(parameter.index);
	public BodyBuilder Starg(String name) => base.Starg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));

	public BodyBuilder Ldfld(FieldBuilder field) => base.Ldfld(buildContext.GetBuilder(field));

	public BodyBuilder Stfld(FieldBuilder field) => base.Stfld(buildContext.GetBuilder(field));

	public BodyBuilder Ldsfld(FieldBuilder field) => base.Ldsfld(buildContext.GetBuilder(field));
	public BodyBuilder Stsfld(FieldBuilder field) => base.Stsfld(buildContext.GetBuilder(field));
	public BodyBuilder Ldflda(FieldBuilder field) => base.Ldflda(buildContext.GetBuilder(field));
	public BodyBuilder Ldsflda(FieldBuilder field) => base.Ldsflda(buildContext.GetBuilder(field));

	public BodyBuilder Call(MethodBuilder method) => base.Call(buildContext.GetBuilder(method));
	public BodyBuilder Callvirt(MethodBuilder method) => base.Callvirt(buildContext.GetBuilder(method));
	public BodyBuilder Ldftn(MethodBuilder method) => base.Ldftn(buildContext.GetBuilder(method));
	public BodyBuilder Newobj(ConstructorBuilder constructor) => base.Newobj(buildContext.GetBuilder(constructor));

	private protected override BodyBuilder Emit(OpCode opcode, MethodInfo meth) => base.Emit(opcode, buildContext.GetCoreAssemblyMethod(meth));
	private protected override BodyBuilder Emit(OpCode opcode, Type type) => base.Emit(opcode, buildContext.GetCoreAssemblyType(type));
}
