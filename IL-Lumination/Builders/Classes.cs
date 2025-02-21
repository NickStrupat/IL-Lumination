using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination;

public sealed class AssemblyBuilder
{
	internal String? name;
	public AssemblyBuilder Name(String name) { this.name = name; return this; }
	
	// internal readonly List<ModuleBuilder> modules = new();
	// public AssemblyBuilder NewModule(out ModuleBuilder moduleBuilder) => this.AddTo(modules, moduleBuilder = new());
	// public AssemblyBuilder NewModule(Action<ModuleBuilder> moduleBuilderAction) => this.AddAction(modules, new(), moduleBuilderAction);
	
	internal readonly List<GlobalMethodBuilder> methods = new();
	public AssemblyBuilder NewMethod(out GlobalMethodBuilder globalMethodBuilder, Action<GlobalMethodBuilder> action) => this.AddAction(methods, globalMethodBuilder = new(), action);
	public AssemblyBuilder NewMethod(out GlobalMethodBuilder globalMethodBuilder) => NewMethod(out globalMethodBuilder, _ => {});
	public AssemblyBuilder NewMethod(Action<GlobalMethodBuilder> action) => NewMethod(out _, action);
	
	internal readonly List<GlobalEnumBuilder> enums = new();
	public AssemblyBuilder NewEnum(out GlobalEnumBuilder globalEnumBuilder, Action<GlobalEnumBuilder> action) => this.AddAction(enums, globalEnumBuilder = new(), action);
	public AssemblyBuilder NewEnum(out GlobalEnumBuilder globalEnumBuilder) => NewEnum(out globalEnumBuilder, _ => {});
	public AssemblyBuilder NewEnum(Action<GlobalEnumBuilder> action) => NewEnum(out _, action);
	
	internal readonly List<GlobalTypeBuilder> types = new();
	public AssemblyBuilder NewType(out GlobalTypeBuilder globalTypeBuilder, Action<GlobalTypeBuilder> action) => this.AddAction(types, globalTypeBuilder = new(), action);
	public AssemblyBuilder NewType(out GlobalTypeBuilder globalTypeBuilder) => NewType(out globalTypeBuilder, _ => {});
	public AssemblyBuilder NewType(Action<GlobalTypeBuilder> action) => NewType(out _, action);
	
	internal MethodRef? entryPoint;
	public AssemblyBuilder EntryPoint(MethodInfo entryPoint) { this.entryPoint = new MethodRef.Declared(entryPoint); return this; }
	public AssemblyBuilder EntryPoint(MethodBuilder entryPoint) { this.entryPoint = new MethodRef.Builder(entryPoint); return this; }
}

public sealed class TypeParameterBuilder
{
	internal TypeParameterBuilder() {}
	
	internal String? name { get; private set; }
	public TypeParameterBuilder Name(String name) { this.name = name; return this; }
}

public abstract class ParameterBuilderBase
{
	internal readonly Int16 index;
	private protected ParameterBuilderBase(Int16 index) => this.index = index;
	
	internal String? name { get; private protected set; }
	internal TypeRef? typeRef { get; private protected set; }
}

public abstract class ParameterBuilderBase<T> : ParameterBuilderBase where T : ParameterBuilderBase<T>
{
	private protected ParameterBuilderBase(Int16 index) : base(index) {}
	
	public T Name(String name) { this.name = name; return (T)this; }
}

public sealed class ParameterBuilder<T> : ParameterBuilderBase<ParameterBuilder<T>>
{
	internal ParameterBuilder(Int16 index) : base(index) => typeRef = typeof(T);
}

public sealed class ParameterBuilder : ParameterBuilderBase<ParameterBuilder>
{
	internal ParameterBuilder(Int16 index) : base(index) {}

	public ParameterBuilder Type(Type type) { this.typeRef = type; return this; }
	public ParameterBuilder Type(TypeBuilder typeBuilder) { this.typeRef = typeBuilder; return this; }
}
