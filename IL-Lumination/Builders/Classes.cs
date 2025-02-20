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
	public AssemblyBuilder NewMethod(out GlobalMethodBuilder globalMethodBuilder) => this.AddTo(methods, globalMethodBuilder = new());
	public AssemblyBuilder NewMethod(Action<GlobalMethodBuilder> action) => this.AddAction(methods, new(), action);
	
	internal readonly List<GlobalEnumBuilder> enums = new();
	public AssemblyBuilder NewEnum(out GlobalEnumBuilder globalEnumBuilder) => this.AddTo(enums, globalEnumBuilder = new());
	public AssemblyBuilder NewEnum(Action<GlobalEnumBuilder> action) => this.AddAction(enums, new(), action);
	
	internal readonly List<GlobalTypeBuilder> types = new();
	public AssemblyBuilder NewType(out GlobalTypeBuilder globalTypeBuilder) => this.AddTo(types, globalTypeBuilder = new());
	public AssemblyBuilder NewType(Action<GlobalTypeBuilder> action) => this.AddAction(types, new(), action);
	
	internal MethodRef? entryPoint;
	public AssemblyBuilder EntryPoint(MethodInfo entryPoint) { this.entryPoint = entryPoint; return this; }
	public AssemblyBuilder EntryPoint(MethodBuilder entryPoint) { this.entryPoint = entryPoint; return this; }
}

public sealed class GlobalMethodBuilder : MethodBuilder<GlobalMethodBuilder>
{
	internal GlobalMethodBuilder() {}

	public GlobalMethodBuilder Internal() { this.visibility = MethodAttributes.Assembly; return this; }
	public GlobalMethodBuilder Public() { this.visibility = MethodAttributes.Public; return this; }
}

public sealed class NestedMethodBuilder : MethodBuilder<NestedMethodBuilder>
{
	internal NestedMethodBuilder() {}
	
	public NestedMethodBuilder Private() { this.visibility = MethodAttributes.Private; return this; }
	public NestedMethodBuilder Internal() { this.visibility = MethodAttributes.Assembly; return this; }
	public NestedMethodBuilder Protected() { this.visibility = MethodAttributes.Family; return this; }
	public NestedMethodBuilder ProtectedAndInternal() { this.visibility = MethodAttributes.FamANDAssem; return this; }
	public NestedMethodBuilder ProtectedOrInternal() { this.visibility = MethodAttributes.FamORAssem; return this; }
	public NestedMethodBuilder Public() { this.visibility = MethodAttributes.Public; return this; }
	
	internal MethodAttributes instantiation;
	public NestedMethodBuilder Static() { this.instantiation = MethodAttributes.Static; return this; }
	public NestedMethodBuilder NonStatic() { this.instantiation = default; return this; }
}

public abstract class MethodBuilder<T> : MethodBuilder where T : MethodBuilder<T>
{
	private protected MethodBuilder() {}
	
	public T Name(String name) { this.name = name; return (T)this; }
	
	public T ReturnType(Type returnType) { this.returnTypeRef = returnType; return (T)this; }
	public T ReturnType(TypeBuilder returnTypeBuilder) { this.returnTypeRef = returnTypeBuilder; return (T)this; }

	public T NewParameter(out ParameterBuilder parameterBuilder, Action<ParameterBuilder> parameterBuilderAction) => (T)this.AddAction(parameters, parameterBuilder = new((Int16)parameters.Count), parameterBuilderAction);
	public T NewParameter(out ParameterBuilder parameterBuilder) => NewParameter(out parameterBuilder, _ => {});
	public T NewParameter(Action<ParameterBuilder> parameterBuilderAction) => NewParameter(out _, parameterBuilderAction);
	public T NewParameter<TParam>(out ParameterBuilder parameterBuilder, Action<ParameterBuilder> parameterBuilderAction) => NewParameter(out parameterBuilder, x => parameterBuilderAction(x.Type(typeof(TParam))));
	public T NewParameter<TParam>(out ParameterBuilder parameterBuilder) => NewParameter<TParam>(out parameterBuilder, _ => {});
	public T NewParameter<TParam>(Action<ParameterBuilder> parameterBuilderAction) => NewParameter<TParam>(out _, parameterBuilderAction);
	
	public T NewTypeParameter(out TypeParameterBuilder typeParameterBuilder) => (T)this.AddTo(typeParameters, typeParameterBuilder = new());
	public T NewTypeParameter(Action<TypeParameterBuilder> typeParameterBuilderAction) => (T)this.AddAction(typeParameters, new(), typeParameterBuilderAction);
	
	public T Body(Action<BodyBuilder> bodyAction) { this.bodyActions.Add(bodyAction); return (T)this; }
}

public abstract class MethodBuilder
{
	private protected MethodBuilder() {}
	
	internal String? name;
	internal TypeRef? returnTypeRef;
	internal readonly List<ParameterBuilder> parameters = new();
	internal readonly List<TypeParameterBuilder> typeParameters = new();
	internal readonly List<Action<BodyBuilder>> bodyActions = new();
	
	internal MethodAttributes visibility = MethodAttributes.Private;
}

public sealed class GlobalEnumBuilder
{
	internal GlobalEnumBuilder() {}
	
	internal String? name;
	public GlobalEnumBuilder Name(String name) { this.name = name; return this; }

	internal TypeAttributes visibility = TypeAttributes.NotPublic;
	public GlobalEnumBuilder Internal() { this.visibility = TypeAttributes.NotPublic; return this; }
	public GlobalEnumBuilder Public() { this.visibility = TypeAttributes.Public; return this; }
	
	internal TypeRef underlyingTypeRef = typeof(Int32);
	public GlobalEnumBuilder UnderlyingType(Type underlyingType) { this.underlyingTypeRef = underlyingType; return this; }
	public GlobalEnumBuilder UnderlyingType(TypeBuilder underlyingTypeBuilder) { this.underlyingTypeRef = underlyingTypeBuilder; return this; }

	internal readonly List<EnumLiteralBuilder> literals = new();
	public GlobalEnumBuilder NewLiteral(out EnumLiteralBuilder enumLiteralBuilder) { this.AddTo(literals, enumLiteralBuilder = new()); return this; }
	public GlobalEnumBuilder NewLiteral(Action<EnumLiteralBuilder> enumLiteralBuilderAction) { this.AddAction(literals, new(), enumLiteralBuilderAction); return this; }
}

public sealed class EnumLiteralBuilder
{
	internal EnumLiteralBuilder() {}
	
	internal String? name;
	public EnumLiteralBuilder Name(String name) { this.name = name; return this; }

	internal Object? value;
	public EnumLiteralBuilder Value(Object value) { this.value = value; return this; }
}

public sealed class NestedEnumBuilder;

public sealed class TypeParameterBuilder
{
	internal TypeParameterBuilder() {}
	
	internal String? name;
	public TypeParameterBuilder Name(String name) { this.name = name; return this; }
}

public sealed class ParameterBuilder
{
	internal readonly Int16 index;
	internal ParameterBuilder(Int16 index) => this.index = index;
	
	internal String? name;
	public ParameterBuilder Name(String name) { this.name = name; return this; }

	internal TypeRef? typeRef;
	public ParameterBuilder Type(Type returnType) { this.typeRef = returnType; return this; }
	public ParameterBuilder Type(TypeBuilder returnTypeBuilder) { this.typeRef = returnTypeBuilder; return this; }
}

// public abstract class BaseTypeBuilder
// {
// 	private Type? returnType;
// 	private BaseTypeBuilder? returnTypeBuilder;
// 	
// 	public BaseTypeBuilder ReturnType(Type type) { this.returnType = new TypeRef.A(type); return this; } 
// 	public BaseTypeBuilder ReturnType<TTypeBuilder>(TTypeBuilder typeBuilder) where TTypeBuilder : BaseTypeBuilder { this.returnType = new TypeRef.B(typeBuilder); return this; }
// 	public TypeParameterBuilder NewTypeParameter() => new(this);
// 	public ParameterBuilder NewParameter() => new(this);
// }