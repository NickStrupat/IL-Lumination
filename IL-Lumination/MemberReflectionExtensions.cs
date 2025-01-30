using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Illumination;

public static class MemberReflectionExtensions
{
    public static Func<T, TMember> GetPropOrFieldGetter<T, TMember>(this Expression<Func<T, TMember>> expr)
    {
        var paramExpr = expr.Parameters[0];
        var memberExpr = expr.GetMemberInfo() switch
        {
            FieldInfo fi  => Expression.Field(paramExpr, fi),
            PropertyInfo { GetMethod: not null } pi => Expression.Property(paramExpr, pi),
            _ => throw new ArgumentOutOfRangeException(nameof(expr), "Expected a field or gettable property expression.")
        };
        return Expression.Lambda<Func<T, TMember>>(memberExpr, expr.Parameters).Compile();
    }
    
    public static Action<T, TMember> GetPropOrFieldSetter<T, TMember>(this Expression<Func<T, TMember>> expr)
    {
        var valueParam = Expression.Parameter(typeof(TMember), "value");
        var paramExpr = expr.Parameters[0];
        var memberExpr = expr.GetMemberInfo() switch
        {
            FieldInfo { IsInitOnly: false } fi  => Expression.Field(paramExpr, fi),
            PropertyInfo { SetMethod: not null } pi => Expression.Property(paramExpr, pi),
            _ => throw new ArgumentOutOfRangeException(nameof(expr), "Expected a mutable field or settable property expression.")
        };
        var assignExpr = Expression.Assign(memberExpr, valueParam);
        return Expression.Lambda<Action<T, TMember>>(assignExpr, expr.Parameters[0], valueParam).Compile();
    }
    
    public static Func<Object, Object?> GetPropOrFieldGetter(this MemberInfo memberInfo) => memberInfo switch
    {
        FieldInfo fi => fi.GetFieldGetter(),
        PropertyInfo pi => pi.GetPropGetter(),
        _ => throw new ArgumentOutOfRangeException(nameof(memberInfo), "Expected a field or property.")
    };
    
    public static Func<Object, Object?> GetPropGetter(this PropertyInfo propertyInfo)
    {
        var instance = Expression.Parameter(typeof(Object));
        var convertInstance = Expression.Convert(instance, propertyInfo.DeclaringType!);
        var property = Expression.Property(convertInstance, propertyInfo);
        var body = Expression.Convert(property, typeof(Object));
        var lambda = Expression.Lambda<Func<Object, Object?>>(body, instance);
        return lambda.Compile();
    }
    
    public static Func<Object, Object?> GetFieldGetter(this FieldInfo fieldInfo)
    {
        var instance = Expression.Parameter(typeof(Object));
        var convertInstance = Expression.Convert(instance, fieldInfo.DeclaringType!);
        var field = Expression.Field(convertInstance, fieldInfo);
        var body = Expression.Convert(field, typeof(Object));
        var lambda = Expression.Lambda<Func<Object, Object?>>(body, instance);
        return lambda.Compile();
    }
    
    public static Action<Object, Object?> GetPropOrFieldSetter(this MemberInfo memberInfo) => memberInfo switch
    {
        FieldInfo fi => fi.GetFieldSetter(),
        PropertyInfo pi => pi.GetPropSetter(),
        _ => throw new ArgumentOutOfRangeException(nameof(memberInfo), "Expected a field or property.")
    };

    public static Action<Object, Object?> GetPropSetter(this PropertyInfo propertyInfo)
    {
        var instance = Expression.Parameter(typeof(Object));
        var convertInstance = Expression.Convert(instance, propertyInfo.DeclaringType!);
        var value = Expression.Parameter(typeof(Object));
        var convertValue = Expression.Convert(value, propertyInfo.PropertyType);
        var property = Expression.Property(convertInstance, propertyInfo);
        var body = Expression.Assign(property, convertValue);
        var lambda = Expression.Lambda<Action<Object, Object?>>(body, instance, value);
        return lambda.Compile();
    }

    public static Action<Object, Object?> GetFieldSetter(this FieldInfo fieldInfo)
    {
        var value = Expression.Parameter(typeof(Object));
        var convertValue = Expression.Convert(value, fieldInfo.FieldType);
        var instance = Expression.Parameter(typeof(Object));
        var convertInstance = Expression.Convert(instance, fieldInfo.DeclaringType!);
        var field = Expression.Field(convertInstance, fieldInfo);
        var body = Expression.Assign(field, convertValue);
        var lambda = Expression.Lambda<Action<Object, Object?>>(body, instance, value);
        return lambda.Compile();
    }
    
    public static PropertyInfo GetPropertyInfo<T, TMember>(this Expression<Func<T, TMember>> expr) =>
        expr.GetMemberInfo<T, TMember, PropertyInfo>();
    
    public static FieldInfo GetFieldInfo<T, TMember>(this Expression<Func<T, TMember>> expr) =>
        expr.GetMemberInfo<T, TMember, FieldInfo>();
    
    // public static EventInfo GetEventInfo<T, TMember>(this Expression<Func<T, TMember>> expr) =>
    //     expr.GetMemberInfo<T, TMember, EventInfo>();

    public static MemberInfo GetMemberInfo<T, TMember>(this Expression<Func<T, TMember>> expr) =>
        expr.GetMemberInfo<T, TMember, MemberInfo>();
    
    public static TMemberInfo GetMemberInfo<T, TMember, TMemberInfo>(this Expression<Func<T, TMember>> expr) where TMemberInfo : MemberInfo =>
        expr.ThrowIfArgIsNull().Body switch
        {
            MemberExpression { Member: TMemberInfo mi, Expression: ParameterExpression } when mi.DeclaringType == typeof(T) => mi,
            UnaryExpression { Operand: MemberExpression { Member: TMemberInfo mi, Expression: ParameterExpression } } when mi.DeclaringType == typeof(T) => mi,
            _ => throw new ArgumentException($"The expression doesn't indicate a valid {typeof(TMemberInfo).Name}. [ {expr} ]")
        };

    public static T ThrowIfArgIsNull<T>(this T value, [CallerArgumentExpression(nameof(value))] String? paramName = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }
}