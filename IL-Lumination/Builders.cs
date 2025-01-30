using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using NickStrupat;

namespace Illumination;

public sealed class XAssemblyBuilder(String name)
{
    public String Name { get; } = name;
    public EquatableHashSet<XTypeBuilder> Types { get; } = new(EqCmp<XTypeBuilder>.Create(x => (x.Name, x.TypeParameters.Count)));

    public Assembly Build()
    {
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new(Name), AssemblyBuilderAccess.RunAndCollect);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(Name);
        foreach (var type in Types)
            _ = type.BuildInternal(moduleBuilder);
        return assemblyBuilder;
    }

    // public void GenerateAssembly(String assemblyFilePath)
    // {
    //     AppContext.SetSwitch("Lokad.ILPack.AssemblyGenerator.ReplaceCoreLibWithNetStandard", true);
    //     var generator = new AssemblyGenerator();
    //     var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new(Name), AssemblyBuilderAccess.RunAndCollect);
    //     generator.GenerateAssembly(assemblyBuilder, assemblyFilePath);
    // }
}

// public sealed class TypeBuilders : EquatableHashSet<XTypeBuilder>
// {
//     internal TypeBuilders() : base(EqCmp) {}
//     private static readonly EqCmp<XTypeBuilder> EqCmp = EqCmp<XTypeBuilder>.Create(x => x.Name, x => x.TypeParameters.Count);
// }
//
// public sealed class NestedTypeBuilders : EquatableHashSet<XTypeBuilder<IVisibility.NestedType>>
// {
//     internal NestedTypeBuilders() : base(EqCmp) {}
//     private static readonly EqCmp<XTypeBuilder<IVisibility.NestedType>> EqCmp =
//         EqCmp<XTypeBuilder<IVisibility.NestedType>>.Create(x => x.Name, x => x.TypeParameters);
// }

public interface IEmits<out T>
{
    T Emit();
}

public interface IVisibility
{
    internal TypeAttributes Value { get; }

    public readonly struct NestedType : IVisibility
    {
        public static NestedType Private => new(TypeAttributes.NestedPrivate);
        public static NestedType Public => new(TypeAttributes.NestedPublic);
        public static NestedType Family => new(TypeAttributes.NestedFamily);
        public static NestedType Assembly => new(TypeAttributes.NestedAssembly);
        public static NestedType FamilyAndAssembly => new(TypeAttributes.NestedFamANDAssem);
        public static NestedType FamilyOrAssembly => new(TypeAttributes.NestedFamORAssem);

        private NestedType(TypeAttributes visibility) => this.visibility = visibility;
        private readonly TypeAttributes visibility;
        public TypeAttributes Value => visibility != default ? visibility : Private.Value;
    }

    public readonly struct Type : IVisibility
    {
        public static Type Public => new(TypeAttributes.Public);
        public static Type Family => new(TypeAttributes.NotPublic);

        private Type(TypeAttributes visibility) => this.visibility = visibility;
        private readonly TypeAttributes visibility;
        public TypeAttributes Value => visibility != default ? visibility : Family.Value;
    }
}

public sealed class XTypeBuilder : XTypeBuilder<ModuleBuilder, IVisibility.Type>
{
    protected override TypeBuilder DefineTypeBuilder(ModuleBuilder moduleBuilder)
    {
        return moduleBuilder.DefineType(Name ?? $"{Guid.NewGuid():N}", Visibility.Value);
    }

    public Type Build()
    {
        var name = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
        var builder = new XAssemblyBuilder(name) { Types = { this }};
        var assembly = builder.Build();
        return assembly.GetTypes().Single();
    }
}

public sealed class XNestedTypeBuilder : XTypeBuilder<TypeBuilder, IVisibility.NestedType>
{
    protected override TypeBuilder DefineTypeBuilder(TypeBuilder builder)
    {
        return builder.DefineNestedType(Name ?? $"{Guid.NewGuid():N}", Visibility.Value);
    }
}

public abstract class XTypeBuilder<TTypeBuilderDefiner, TVisibility> where TVisibility : struct, IVisibility
{
    public String? Name { get; set; }
    public TVisibility Visibility { get; set; }
    public EquatableHashSet<XTypeParameterBuilder> TypeParameters { get; } = new(EqCmp<XTypeParameterBuilder>.Create(x => x.Name));
    public EquatableHashSet<XNestedTypeBuilder> NestedTypes { get; } = new(EqCmp<XNestedTypeBuilder>.Create(x => (x.Name, x.TypeParameters.Count)));

    // public sealed class TypeParameterBuilders : EquatableHashSet<XTypeParameterBuilder>
    // {
    //     internal TypeParameterBuilders() : base(EqCmp) {}
    //     private static readonly EqCmp<XTypeParameterBuilder> EqCmp = EqCmp<XTypeParameterBuilder>.Create(x => x.Name);
    // }

    //private delegate TypeBuilder DefineTypeBuilder(String name, ) where TVisibility : struct, IVisibility;
    
    protected abstract TypeBuilder DefineTypeBuilder(TTypeBuilderDefiner builder);

    internal Type BuildInternal(TTypeBuilderDefiner builder)
    {
        var typeBuilder = DefineTypeBuilder(builder);
        var genericParameters = typeBuilder.DefineGenericParameters(TypeParameters.Select(x => x.Name).ToArray());
        foreach (var (genericParameter, typeParameter) in genericParameters.Zip(TypeParameters))
        {
            var constraint = typeParameter.Constraints;
            genericParameter.SetBaseTypeConstraint(constraint.BaseType?.Type);
            genericParameter.SetInterfaceConstraints(constraint.Interfaces.Select(x => x.Type).ToArray());
        }
        foreach (var nestedType in NestedTypes)
        {
            var nestedTypeBuilder = typeBuilder.DefineNestedType(nestedType.Name ?? $"{Guid.NewGuid():N}", nestedType.Visibility.Value);
            _ = nestedType.BuildInternal(nestedTypeBuilder);
            //nestedType.Build();
        }

        return typeBuilder.CreateType();
    }
}

public sealed class XTypeParameterBuilder(String name)
{
    public String Name { get; } = name;
    public Constraints Constraints { get; } = new();
}

public sealed class Constraints
{
    public ClassType? BaseType { get; set; }
    public EquatableHashSet<InterfaceType> Interfaces { get; } = new(EqCmp<InterfaceType>.Create(x => x.Type));
}

public sealed class TypeSubclassOf<T>(Type type)
{
    public Type Type { get; } = type.IsSubclassOf(typeof(T)) ? type : throw new ArgumentException($"Expected a type subclass of {typeof(T).Name}.", nameof(type));
}

public sealed class TypeAssignableTo<T>(Type type)
{
    public Type Type { get; } = type.IsAssignableTo(typeof(T)) ? type : throw new ArgumentException($"Expected a type assignable to {typeof(T).Name}.", nameof(type));
}

public sealed class ClassType(Type type)
{
    public Type Type { get; } = type.IsClass ? type : throw new ArgumentException("Expected a class type.", nameof(type));
}

public sealed class StructType(Type type)
{
    public Type Type { get; } = type.IsValueType ? type : throw new ArgumentException("Expected a struct type.", nameof(type));
}

public sealed class InterfaceType(Type type)
{
    public Type Type { get; } = type.IsInterface ? type : throw new ArgumentException("Expected an interface type.", nameof(type));
}

public class EquatableHashSet<T>(IEqualityComparer<T> ec) : HashSet<T>(ec), IEquatable<HashSet<T>>, IEquatable<EquatableHashSet<T>>
where T : notnull
{
    public Boolean Equals(EquatableHashSet<T>? other) => EqualsInternal(other);
    public Boolean Equals(HashSet<T>? other) => EqualsInternal(other);
    public sealed override Boolean Equals(Object? obj) => obj is HashSet<T> other && EqualsInternal(other);
    private Boolean EqualsInternal(HashSet<T>? other) => other is not null && SetEquals(other);
    public sealed override Int32 GetHashCode() => HashCode.Combine(Count);
}