using System;
using System.Collections.Generic;
using System.Numerics;

namespace Illumination.Builders;

public sealed class AssemblyBuilder : IBuilder<System.Reflection.Emit.AssemblyBuilder>
{
	internal String? name;
	public AssemblyBuilder Name(String name) { this.name = name; return this; }
	
	// internal readonly List<ModuleBuilder> modules = new();
	// public AssemblyBuilder NewModule(out ModuleBuilder moduleBuilder) => this.AddTo(modules, moduleBuilder = new());
	// public AssemblyBuilder NewModule(Action<ModuleBuilder> moduleBuilderAction) => this.AddAction(modules, new(), moduleBuilderAction);
	
	internal readonly List<GlobalMethodBuilder> methods = new();
	public AssemblyBuilder NewMethod(out GlobalMethodBuilder globalMethodBuilder, Action<GlobalMethodBuilder> action) =>
		this.AddAction(methods, globalMethodBuilder = new(this), action);
	public AssemblyBuilder NewMethod(out GlobalMethodBuilder globalMethodBuilder) => NewMethod(out globalMethodBuilder, _ => {});
	public AssemblyBuilder NewMethod(Action<GlobalMethodBuilder> action) => NewMethod(out _, action);
	
	internal readonly List<EnumBuilder> enums = new();
	public AssemblyBuilder NewEnum(out GlobalEnumBuilder globalEnumBuilder, Action<GlobalEnumBuilder> action) =>
		this.AddAction(enums.AsContravariant(), globalEnumBuilder = new(), action);
	public AssemblyBuilder NewEnum(out GlobalEnumBuilder globalEnumBuilder) => NewEnum(out globalEnumBuilder, _ => {});
	public AssemblyBuilder NewEnum(Action<GlobalEnumBuilder> action) => NewEnum(out _, action);
	
	public AssemblyBuilder NewEnum<TIntegral>(out GlobalEnumBuilder<TIntegral> globalEnumBuilder, Action<GlobalEnumBuilder<TIntegral>> action)
		where TIntegral : unmanaged, IBinaryInteger<TIntegral> =>
		this.AddAction(enums.AsContravariant(), globalEnumBuilder = new(), action);
	public AssemblyBuilder NewEnum<TIntegral>(out GlobalEnumBuilder<TIntegral> globalEnumBuilder)
		where TIntegral : unmanaged, IBinaryInteger<TIntegral> =>
		NewEnum(out globalEnumBuilder, _ => {});
	public AssemblyBuilder NewEnum<TIntegral>(Action<GlobalEnumBuilder<TIntegral>> action)
		where TIntegral : unmanaged, IBinaryInteger<TIntegral> =>
		NewEnum(out _, action);

	internal readonly List<GlobalTypeBuilder> types = new();
	public AssemblyBuilder NewType(out GlobalTypeBuilder globalTypeBuilder, Action<GlobalTypeBuilder> action) =>
		this.AddAction(types, globalTypeBuilder = new(this), action);
	public AssemblyBuilder NewType(out GlobalTypeBuilder globalTypeBuilder) => NewType(out globalTypeBuilder, _ => {});
	public AssemblyBuilder NewType(Action<GlobalTypeBuilder> action) => NewType(out _, action);

	internal MethodBuilder? entryPoint;
	// public AssemblyBuilder EntryPoint() { this.entryPoint = null; return this; }
	// public AssemblyBuilder EntryPoint(MethodInfo entryPoint) { this.entryPoint = entryPoint; return this; }
	// public AssemblyBuilder EntryPoint(MethodBuilder entryPoint) { this.entryPoint = entryPoint; return this; }

	public IEnumerable<TypeBuilder> Types => types;
	public IEnumerable<MethodBuilder> Methods => methods;
}