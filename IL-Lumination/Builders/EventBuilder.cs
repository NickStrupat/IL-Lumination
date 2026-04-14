using System;
using System.Collections.Generic;
using System.Reflection;

namespace Illumination.Builders;

public sealed class EventBuilder : IBuilder<System.Reflection.Emit.EventBuilder>
{
    internal EventBuilder(TypeBuilder containingType) => this.containingType = containingType;

    internal TypeBuilder containingType { get; }
    internal String? name { get; private set; }
    public EventBuilder Name(String name) { this.name = name; return this; }

    internal TypeRef? eventHandlerTypeRef { get; private set; }
    public EventBuilder HandlerType(Type type) { this.eventHandlerTypeRef = type; return this; }
    public EventBuilder HandlerType<T>() => HandlerType(typeof(T));
    public EventBuilder HandlerType(TypeBuilder typeBuilder) { this.eventHandlerTypeRef = typeBuilder; return this; }

    internal NestedMethodBuilder? addMethodBuilder { get; private set; }
    internal NestedMethodBuilder? removeMethodBuilder { get; private set; }

    public EventBuilder AddMethod(out NestedMethodBuilder methodBuilder, Action<NestedMethodBuilder> action)
    {
        addMethodBuilder = methodBuilder = new(containingType);
        action(methodBuilder);
        return this;
    }
    public EventBuilder AddMethod(Action<NestedMethodBuilder> action) => AddMethod(out _, action);

    public EventBuilder RemoveMethod(out NestedMethodBuilder methodBuilder, Action<NestedMethodBuilder> action)
    {
        removeMethodBuilder = methodBuilder = new(containingType);
        action(methodBuilder);
        return this;
    }
    public EventBuilder RemoveMethod(Action<NestedMethodBuilder> action) => RemoveMethod(out _, action);
}
