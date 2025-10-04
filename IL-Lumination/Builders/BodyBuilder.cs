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
	// public BodyBuilder Stfld(String name) => base.Stfld();
	
	private protected override BodyBuilder Emit(OpCode opcode, MethodInfo meth) => base.Emit(opcode, buildContext.GetCoreAssemblyMethod(meth));
	private protected override BodyBuilder Emit(OpCode opcode, Type type) => base.Emit(opcode, buildContext.GetCoreAssemblyType(type));

	//public BodyBuilder Call(MethodBuilder method) => this.Call(methodBuilderToSreMethodInfoLookup(method));
}