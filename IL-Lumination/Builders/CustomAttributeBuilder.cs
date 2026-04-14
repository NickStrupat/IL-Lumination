using System;
using System.Reflection;

using Sre = System.Reflection.Emit;

namespace Illumination.Builders;

public sealed class CustomAttributeBuilder
{
    internal CustomAttributeBuilder(ConstructorInfo constructor, Object[] constructorArgs, PropertyInfo[]? namedProperties, Object[]? propertyValues, FieldInfo[]? namedFields, Object[]? fieldValues)
    {
        this.constructor = constructor;
        this.constructorArgs = constructorArgs;
        this.namedProperties = namedProperties;
        this.propertyValues = propertyValues;
        this.namedFields = namedFields;
        this.fieldValues = fieldValues;
    }

    internal readonly ConstructorInfo constructor;
    internal readonly Object[] constructorArgs;
    internal readonly PropertyInfo[]? namedProperties;
    internal readonly Object[]? propertyValues;
    internal readonly FieldInfo[]? namedFields;
    internal readonly Object[]? fieldValues;

    internal Sre.CustomAttributeBuilder ToSre() =>
        namedProperties is not null && namedFields is not null
            ? new(constructor, constructorArgs, namedProperties, propertyValues!, namedFields, fieldValues!)
            : namedProperties is not null
                ? new(constructor, constructorArgs, namedProperties, propertyValues!)
                : namedFields is not null
                    ? new(constructor, constructorArgs, namedFields, fieldValues!)
                    : new(constructor, constructorArgs);
}
