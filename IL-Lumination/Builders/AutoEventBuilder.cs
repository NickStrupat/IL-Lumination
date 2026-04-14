using System;
using System.Reflection;

namespace Illumination.Builders;

public sealed class AutoEventBuilder
{
    internal AutoEventBuilder() {}

    internal String? name { get; private set; }
    public AutoEventBuilder Name(String name) { this.name = name; return this; }

    internal Type? handlerType { get; private set; }
    public AutoEventBuilder HandlerType(Type type) { this.handlerType = type; return this; }
    public AutoEventBuilder HandlerType<T>() where T : Delegate => HandlerType(typeof(T));

    internal MethodAttributes visibility { get; private set; }
    public AutoEventBuilder Private() { this.visibility = MethodAttributes.Private; return this; }
    public AutoEventBuilder Family() { this.visibility = MethodAttributes.Family; return this; }
    public AutoEventBuilder FamilyAndAssembly() { this.visibility = MethodAttributes.FamANDAssem; return this; }
    public AutoEventBuilder FamilyOrAssembly() { this.visibility = MethodAttributes.FamORAssem; return this; }
    public AutoEventBuilder Assembly() { this.visibility = MethodAttributes.Assembly; return this; }
    public AutoEventBuilder Public() { this.visibility = MethodAttributes.Public; return this; }
}
