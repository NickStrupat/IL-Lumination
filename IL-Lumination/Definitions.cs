using System;
using System.Collections.Generic;

namespace Illumination;

public abstract class Definition
{
	private protected Definition() {}

	public Int64 Id { get; private set; }
	public Int64? ParentId { get; set; }
	public Definition? Parent { get; set; }
	public String? Name { get; set; }
	
	//public HashSet<Definition> Children { get; } = new();
}

public abstract class NamedDefinition : Definition
{
	private protected NamedDefinition(String name) => Name = name; 
	
	public new String Name { get => base.Name!; set => base.Name = value; }
}

// public abstract record SourceFileDef : Definition
// {
// 	
// }

public sealed class AssemblyDef(String name) : NamedDefinition(name)
{
	public HashSet<ModuleDef> Modules { get; } = new();
}

public sealed class ModuleDef(String name) : NamedDefinition(name)
{
	public HashSet<TypeDef.GlobalTypeDef> Types { get; } = new();
	public HashSet<MethodDef.GlobalMethodDef> Methods { get; } = new();
	public HashSet<EnumDef> Enums { get; } = new();
}

public sealed class EnumDef(String name) : NamedDefinition(name)
{
	public required TypeRef UnderlyingType { get; set; }
	public HashSet<EnumMemberDef> Members { get; } = new();
	
	//public new required Definition ParentDef { get => base.ParentDef!; set => base.ParentDef = value; }
}

public sealed class EnumMemberDef(String name, Int64 value) : NamedDefinition(name)
{
	public Int64 Value { get; set; }
}

public abstract class TypeDef : NamedDefinition
{
	private TypeDef(String name) : base(name) {}
	
	public MethodDef? StaticConstructor { get; set; }
	public HashSet<NestedTypeDef> Types { get; } = new();
	public HashSet<MethodDef> Methods { get; } = new();
	
	public sealed class GlobalTypeDef(String Name) : TypeDef(Name);
	public sealed class NestedTypeDef(String Name) : TypeDef(Name);
}

public abstract class MethodDef : NamedDefinition
{
	private MethodDef(String name) : base(name) {}
	
	public HashSet<GenericParameterDef> GenericParameters { get; } = new();
	public HashSet<ParameterDef> Parameters { get; } = new();
	public required Type ReturnType { get; set; }

	public sealed class GlobalMethodDef(String name) : MethodDef(name);
	public sealed class NestedMethodDef(String name) : MethodDef(name);
}

public sealed class ParameterDef(String name) : NamedDefinition(name)
{
	public required TypeRef TypeRef { get; set; }
}

public sealed class GenericParameterDef(String name) : NamedDefinition(name);

public sealed class TypeRef(String assemblyQualifiedName) : Definition
{
	public String AssemblyQualifiedName { get; private set; } = assemblyQualifiedName;
}