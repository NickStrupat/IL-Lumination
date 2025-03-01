using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Illumination;

internal sealed class TypeBuilderDependencies
{
	public readonly List<TypeBuilder> typeBuildersWithNoDependencies = new();
	private readonly Dictionary<TypeBuilder, Int32> counts = new();
	
	public void AddDependency(TypeBuilder dependent, TypeBuilder dependency) =>
		CollectionsMarshal.GetValueRefOrAddDefault(counts, dependent, out _)++;
}

public sealed class AssemblyBuilder
{
	internal readonly HashSet<TypeBuilder> typeBuildersWhoseBaseTypeOrInterfacesReferenceAtLeastOneTypeBuilder = new();
	
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
	public AssemblyBuilder NewType(out GlobalTypeBuilder globalTypeBuilder, Action<GlobalTypeBuilder> action) => this.AddAction(types, globalTypeBuilder = new(this), action);
	public AssemblyBuilder NewType(out GlobalTypeBuilder globalTypeBuilder) => NewType(out globalTypeBuilder, _ => {});
	public AssemblyBuilder NewType(Action<GlobalTypeBuilder> action) => NewType(out _, action);
	
	internal MethodRef? entryPoint;
	public AssemblyBuilder EntryPoint(MethodInfo entryPoint) { this.entryPoint = new MethodRef.Declared(entryPoint); return this; }
	public AssemblyBuilder EntryPoint(MethodBuilder entryPoint) { this.entryPoint = new MethodRef.Builder(entryPoint); return this; }
}