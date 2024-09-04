using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IL_Lumination;

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
    
    public static PropertyInfo GetPropertyInfo<T, TMember>(this Expression<Func<T, TMember>> expr) =>
        expr.GetMemberInfo<T, TMember, PropertyInfo>();
    
    public static FieldInfo GetFieldInfo<T, TMember>(this Expression<Func<T, TMember>> expr) =>
        expr.GetMemberInfo<T, TMember, FieldInfo>();
    
    // public static EventInfo GetEventInfo<T, TMember>(this Expression<Func<T, TMember>> expr) =>
    //     expr.GetMemberInfo<T, TMember, EventInfo>();

    public static MemberInfo GetMemberInfo<T, TMember>(this Expression<Func<T, TMember>> expr) =>
        expr.GetMemberInfo<T, TMember, MemberInfo>();
    
    public static TMemberInfo GetMemberInfo<T, TMember, TMemberInfo>(this Expression<Func<T, TMember>> expr) where TMemberInfo : MemberInfo =>
        expr.ThrowNullArgExIfNull().Body switch
        {
            MemberExpression { Member: TMemberInfo mi, Expression: ParameterExpression } when mi.DeclaringType == typeof(T) => mi,
            UnaryExpression { Operand: MemberExpression { Member: TMemberInfo mi, Expression: ParameterExpression } } when mi.DeclaringType == typeof(T) => mi,
            _ => throw new ArgumentException($"The expression doesn't indicate a valid {typeof(TMemberInfo).Name}. [ {expr} ]")
        };

    public static T ThrowNullArgExIfNull<T>(this T value, [CallerArgumentExpression(nameof(value))] String paramName = "") where T : notnull
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }
}