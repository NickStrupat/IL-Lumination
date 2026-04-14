using System;
using System.Reflection;

namespace Illumination.Builders;

public sealed class AutoPropertyBuilder
{
    internal AutoPropertyBuilder() {}

    internal String? name { get; private set; }
    public AutoPropertyBuilder Name(String name) { this.name = name; return this; }

    internal TypeRef? typeRef { get; private set; }
    public AutoPropertyBuilder Type(Type type) { this.typeRef = type; return this; }
    public AutoPropertyBuilder Type<T>() => Type(typeof(T));
    public AutoPropertyBuilder Type(TypeBuilder typeBuilder) { this.typeRef = typeBuilder; return this; }

    internal MethodAttributes visibility { get; private set; }
    public AutoPropertyBuilder Private() { this.visibility = MethodAttributes.Private; return this; }
    public AutoPropertyBuilder Family() { this.visibility = MethodAttributes.Family; return this; }
    public AutoPropertyBuilder FamilyAndAssembly() { this.visibility = MethodAttributes.FamANDAssem; return this; }
    public AutoPropertyBuilder FamilyOrAssembly() { this.visibility = MethodAttributes.FamORAssem; return this; }
    public AutoPropertyBuilder Assembly() { this.visibility = MethodAttributes.Assembly; return this; }
    public AutoPropertyBuilder Public() { this.visibility = MethodAttributes.Public; return this; }

    internal MethodAttributes? getterVisibility { get; private set; }
    public AutoPropertyBuilder GetVisibility(MethodAttributes visibility) { this.getterVisibility = visibility; return this; }

    internal MethodAttributes? setterVisibility { get; private set; }
    public AutoPropertyBuilder SetVisibility(MethodAttributes visibility) { this.setterVisibility = visibility; return this; }

    internal Boolean hasGetter { get; private set; } = true;
    internal Boolean hasSetter { get; private set; } = true;
    public AutoPropertyBuilder GetOnly() { this.hasGetter = true; this.hasSetter = false; return this; }
    public AutoPropertyBuilder SetOnly() { this.hasGetter = false; this.hasSetter = true; return this; }

    internal MethodAttributes storageType { get; private set; }
    public AutoPropertyBuilder Static() { this.storageType = MethodAttributes.Static; return this; }
}
