using System;
using System.Collections.Generic;

namespace Illumination;

public sealed class TypeParameterBuilder
{
	internal TypeParameterBuilder() {}
	
	internal String? name { get; private set; }
	public TypeParameterBuilder Name(String name) { this.name = name; return this; }
	
	internal TypeRef? constraint { get; private set; }
	public TypeParameterBuilder Constraint(Type type) { constraint = type; return this; }
	public TypeParameterBuilder Constraint(TypeBuilder typeBuilder) { constraint = typeBuilder; return this; }
}