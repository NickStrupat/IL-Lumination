using System;
using System.Collections.Generic;

namespace Illumination;

// public interface IHasName { String Name { get; } }
// public interface IHasModules { Dictionary<String, IModule> Modules { get; } }
// public interface IHasTypes { Dictionary<String, IType> Types { get; } }
// public interface IHasMethods { Dictionary<String, IMethod> Methods { get; } }
// public interface IHasEnums { Dictionary<String, IEnum> Enums { get; } }
// public interface IHasProperties { Dictionary<String, IProperty> Properties { get; } }
// public interface IHasEvents { Dictionary<String, IEvent> Events { get; } }
// public interface IHasFields { Dictionary<String, IField> Fields { get; } }
// public interface IHasConstructors { Dictionary<String, IConstructor> Constructors { get; } }
// public interface IHasGenericParameters { Dictionary<String, IGenericParameter> GenericParameters { get; } }

public interface IAssembly
{
	String Name { get; }
	Dictionary<String, IModule> Modules { get; }
}

public interface IModule
{
	String Name { get; }
	Dictionary<String, IGlobalType> Types { get; }
	Dictionary<String, IGlobalMethod> Methods { get; }
	Dictionary<String, IEnum> Enums { get; }
}

public interface ITypeBase
{
	String Name { get; }
	ITypeInitializer? Initializer { get; }
	Dictionary<List<IParameter>, IConstructor> Constructors { get; }
	Dictionary<String, IGenericParameter> GenericParameters { get; }
	Dictionary<String, IType> Types { get; }
	Dictionary<String, IMethod> Methods { get; }
	Dictionary<String, IEnum> Enums { get; }
	Dictionary<String, IEvent> Events { get; }
	Dictionary<String, IField> Fields { get; }
	Dictionary<String, IProperty> Properties { get; }
}

public interface IType : ITypeBase
{
	Visibility Visibility { get; }
}

public enum Visibility
{
	Private,
	ProtectedAndInternal,
	Protected,
	ProtectedOrInternal,
	Internal,
	Public,
}

public interface IGlobalType : ITypeBase
{
	GlobalVisibility Visibility { get; }
}

public enum GlobalVisibility
{
	Internal,
	Public,
}

public interface ITypeInitializer
{
	public Body Body { get; }
}

public interface IGenericParameter
{
	String Name { get; }
	IGenericConstraint? Constrains { get; }
}

public interface IGenericConstraint
{
	IType IsAssignableToType { get; }
	ISet<IInterface> ImplementsInterfaces { get; }
}

public interface IInterface : ITypeBase;

public interface IGlobalMethod
{
	String? Name { get; }
	Dictionary<String, IGenericParameter> GenericParameters { get; }
	Dictionary<String, IParameter> Parameters { get; }
	public Body Body { get; }
}

public interface IMethod
{
	String? Name { get; }
	Dictionary<String, IGenericParameter> GenericParameters { get; }
	Dictionary<String, IParameter> Parameters { get; }
	Body Body { get; }
}

public interface IParameter
{
	String Name { get; }
}

public interface IEnum
{
}

public interface IEvent
{
}

public interface IConstructor
{
	Body Body { get; }
}

public interface IField
{
}

public interface IProperty
{
	public IGetter? Getter { get; }
	public ISetter? Setter { get; }
}

public interface IAccessor
{
	Visibility Visibility { get; }
	Body Body { get; }
}

public interface IGetter : IAccessor;
public interface ISetter : IAccessor;