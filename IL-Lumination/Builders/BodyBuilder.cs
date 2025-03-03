using System;
using System.Reflection.Emit;

namespace Illumination.Builders;

public sealed class BodyBuilder : BodyBase<BodyBuilder>
{
	private readonly Func<String, Int16?> parameterNameToIndexLookup;
	private readonly Func<String, Int16?> localNameToIndexLookup;
	//private readonly Func<MethodBuilder, System.Reflection.Emit.MethodBuilder> methodBuilderToSreMethodInfoLookup;

	internal BodyBuilder(
		ILGenerator ilGenerator
		, Func<String, Int16?> parameterNameToIndexLookup
		, Func<String, Int16?> localNameToIndexLookup
		//, Func<MethodBuilder, System.Reflection.Emit.MethodBuilder> methodBuilderToSreMethodInfoLookup
		) : base(ilGenerator)
	{
		this.parameterNameToIndexLookup = parameterNameToIndexLookup;
		this.localNameToIndexLookup = localNameToIndexLookup;
		//this.methodBuilderToSreMethodInfoLookup = methodBuilderToSreMethodInfoLookup;
	}

	public BodyBuilder Ldarg(ParameterBuilderBase parameter) => Ldarg(parameter.index);
	public BodyBuilder Ldarg(String name) => Ldarg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));
	
	public BodyBuilder Starg(ParameterBuilderBase parameter) => this.Starg(parameter.index);
	public BodyBuilder Starg(String name) => this.Starg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));
	
	public BodyBuilder Ldloc(LocalBuilderBase local) => this.Ldloc(local.index);
	public BodyBuilder Ldloc(String name) => this.Ldloc(localNameToIndexLookup(name) ?? throw new ArgumentException($"Local '{name}' not found", nameof(name)));
	
	public BodyBuilder Stloc(LocalBuilderBase local) => this.Stloc(local.index);
	public BodyBuilder Stloc(String name) => this.Stloc(localNameToIndexLookup(name) ?? throw new ArgumentException($"Local '{name}' not found", nameof(name)));
	
	//public BodyBuilder Call(MethodBuilder method) => this.Call(methodBuilderToSreMethodInfoLookup(method));
}