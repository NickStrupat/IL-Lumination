using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace Illumination.Builders;

internal interface IHasTypesAndMethods
{
	IEnumerable<TypeBuilder> Types { get; }
	IEnumerable<MethodBuilder> Methods { get; }
}

public sealed class AssemblyBuilder : IHasTypesAndMethods, IBuilder<System.Reflection.Emit.AssemblyBuilder>
{
	internal String? name;
	public AssemblyBuilder Name(String name) { this.name = name; return this; }
	
	// internal readonly List<ModuleBuilder> modules = new();
	// public AssemblyBuilder NewModule(out ModuleBuilder moduleBuilder) => this.AddTo(modules, moduleBuilder = new());
	// public AssemblyBuilder NewModule(Action<ModuleBuilder> moduleBuilderAction) => this.AddAction(modules, new(), moduleBuilderAction);
	
	internal readonly List<GlobalMethodBuilder> methods = new();
	public AssemblyBuilder NewMethod(out GlobalMethodBuilder globalMethodBuilder, Action<GlobalMethodBuilder> action)
	{
		return this.AddAction(methods, globalMethodBuilder = new(this), action);
	}

	internal readonly List<EnumBuilder> enums = new();
	public AssemblyBuilder NewEnum(out GlobalEnumBuilder globalEnumBuilder, Action<GlobalEnumBuilder> action)
	{
		return this.AddAction(enums.AsContravariant(), globalEnumBuilder = new(), action);
	}

	public AssemblyBuilder NewEnum<TIntegral>(out GlobalEnumBuilder<TIntegral> globalEnumBuilder, Action<GlobalEnumBuilder<TIntegral>> action)
		where TIntegral : unmanaged, IBinaryInteger<TIntegral>
	{
		return this.AddAction(enums.AsContravariant(), globalEnumBuilder = new(), action);
	}

	internal readonly List<GlobalTypeBuilder> types = new();
	public AssemblyBuilder NewType(out GlobalTypeBuilder globalTypeBuilder, Action<GlobalTypeBuilder> action)
	{
		return this.AddAction(types, globalTypeBuilder = new(this), action);
	}

	internal MethodBuilder? entryPoint;
	// public AssemblyBuilder EntryPoint() { this.entryPoint = null; return this; }
	// public AssemblyBuilder EntryPoint(MethodInfo entryPoint) { this.entryPoint = entryPoint; return this; }
	// public AssemblyBuilder EntryPoint(MethodBuilder entryPoint) { this.entryPoint = entryPoint; return this; }

	IEnumerable<TypeBuilder> IHasTypesAndMethods.Types => types;
	IEnumerable<MethodBuilder> IHasTypesAndMethods.Methods => methods;
}

public static class AssemblyBuilderExtensions
{
	public static AssemblyBuilder NewMethod(this AssemblyBuilder ab, out GlobalMethodBuilder globalMethodBuilder) => ab.NewMethod(out globalMethodBuilder, _ => {});
	public static AssemblyBuilder NewMethod(this AssemblyBuilder ab, Action<GlobalMethodBuilder> action) => ab.NewMethod(out _, action);

	public static AssemblyBuilder NewEnum(this AssemblyBuilder ab, out GlobalEnumBuilder globalEnumBuilder) => ab.NewEnum(out globalEnumBuilder, _ => {});
	public static AssemblyBuilder NewEnum(this AssemblyBuilder ab, Action<GlobalEnumBuilder> action) => ab.NewEnum(out _, action);

	public static AssemblyBuilder NewEnum<TIntegral>(this AssemblyBuilder ab, out GlobalEnumBuilder<TIntegral> globalEnumBuilder)
		where TIntegral : unmanaged, IBinaryInteger<TIntegral> =>
		ab.NewEnum(out globalEnumBuilder, _ => {});
	public static AssemblyBuilder NewEnum<TIntegral>(this AssemblyBuilder ab, Action<GlobalEnumBuilder<TIntegral>> action)
		where TIntegral : unmanaged, IBinaryInteger<TIntegral> =>
		ab.NewEnum(out _, action);

	public static AssemblyBuilder NewType(this AssemblyBuilder ab, out GlobalTypeBuilder globalTypeBuilder) => ab.NewType(out globalTypeBuilder, _ => {});
	public static AssemblyBuilder NewType(this AssemblyBuilder ab, Action<GlobalTypeBuilder> action) => ab.NewType(out _, action);
}