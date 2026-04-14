using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Illumination.Builders;

public sealed class BodyBuilder : BodyBase<BodyBuilder>
{
	private readonly BuildContext buildContext;
	private readonly Func<String, Int16?> parameterNameToIndexLookup;
	private readonly Func<String, Int16?> localNameToIndexLookup;
	//private readonly Func<MethodBuilder, System.Reflection.Emit.MethodBuilder> methodBuilderToSreMethodInfoLookup;

	internal BodyBuilder(
		ILGenerator ilGenerator
		, BuildContext buildContext
		, Func<String, Int16?> parameterNameToIndexLookup
		, Func<String, Int16?> localNameToIndexLookup
		//, Func<MethodBuilder, System.Reflection.Emit.MethodBuilder> methodBuilderToSreMethodInfoLookup
		) : base(ilGenerator)
	{
		this.buildContext = buildContext;
		this.parameterNameToIndexLookup = parameterNameToIndexLookup;
		this.localNameToIndexLookup = localNameToIndexLookup;
		//this.methodBuilderToSreMethodInfoLookup = methodBuilderToSreMethodInfoLookup;
	}

	public BodyBuilder Ldarg(ParameterBuilderBase parameter) => base.Ldarg(parameter.index);
	public BodyBuilder Ldarg(String name) => base.Ldarg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));

	public BodyBuilder Starg(ParameterBuilderBase parameter) => base.Starg(parameter.index);
	public BodyBuilder Starg(String name) => base.Starg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));

	public BodyBuilder Ldloc(LocalBuilderBase local) => base.Ldloc(local.index);
	public BodyBuilder Ldloc(String name) => base.Ldloc(localNameToIndexLookup(name) ?? throw new ArgumentException($"Local '{name}' not found", nameof(name)));

	public BodyBuilder Stloc(LocalBuilderBase local) => base.Stloc(local.index);
	public BodyBuilder Stloc(String name) => base.Stloc(localNameToIndexLookup(name) ?? throw new ArgumentException($"Local '{name}' not found", nameof(name)));

	public BodyBuilder Ldfld(FieldBuilder field) => base.Ldfld(buildContext.GetBuilder(field));
	// public BodyBuilder Ldfld(String name) => base.Ldfld();

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