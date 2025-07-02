using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination.Builders;

public sealed class TypeParameterBuilder
{
	internal TypeParameterBuilder() {}
	
	internal String? name { get; private set; }
	public TypeParameterBuilder Name(String name) { this.name = name; return this; }
	
	internal TypeRef? baseType { get; private set; }
	internal readonly List<TypeRef> interfaces = new();
	internal GenericParameterAttributes variance { get; private set; }
	internal GenericParameterAttributes special { get; private set; }
	internal GenericParameterAttributes allowByRefLike { get; private set; }
	public TypeParameterBuilder BaseTypeConstraint(Type type) { baseType = type; return this; }
	public TypeParameterBuilder AddInterfaceConstraint(Type type) { interfaces.Add(type); return this; }
	public TypeParameterBuilder Invariant() { variance = GenericParameterAttributes.None; return this; }
	public TypeParameterBuilder Covariant() { variance = GenericParameterAttributes.Covariant; return this; }
	public TypeParameterBuilder Contravariant() { variance = GenericParameterAttributes.Contravariant; return this; }
	public TypeParameterBuilder Constraint(GenericParameterAttributes attributes) { special |= attributes; return this; }
	public TypeParameterBuilder AllowByRefLike() { allowByRefLike = GenericParameterAttributes.AllowByRefLike; return this; }
}