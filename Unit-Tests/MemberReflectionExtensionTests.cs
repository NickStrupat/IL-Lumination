using System;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using IL_Lumination;
using Xunit;

namespace Unit_Tests;

public class MemberReflectionExtensionTests
{
    class Foo
    {
        public String Name { get; set; } = String.Empty;
        public DateOnly BirthDate;
        public Int32 AgeInYears => DateTime.Now.Year - BirthDate.Year;
    }

    [Fact]
    public void GetAutoPropertyGetterDelegate()
    {
        Expression<Func<Foo, String>> expr = x => x.Name;
        var getter = expr.GetPropOrFieldGetter();
        var expected = getter(new Foo { Name = "test" });
        expected.Should().Be("test");
    }

    [Fact]
    public void GetAutoPropertySetterDelegate()
    {
        Expression<Func<Foo, String>> expr = x => x.Name;
        var setter = expr.GetPropOrFieldSetter();
        var foo = new Foo();
        setter(foo, "test");
        foo.Name.Should().Be("test");
        setter(foo, "asdf");
        foo.Name.Should().Be("asdf");
    }

    [Fact]
    public void GetFieldGetterDelegate()
    {
        Expression<Func<Foo, DateOnly>> expr = x => x.BirthDate;
        var getter = expr.GetPropOrFieldGetter();
        var expected = getter(new Foo { BirthDate = new DateOnly(2021, 1, 1) });
        expected.Should().Be(new DateOnly(2021, 1, 1));
    }
    
    [Fact]
    public void GetFieldSetterDelegate()
    {
        Expression<Func<Foo, DateOnly>> expr = x => x.BirthDate;
        var setter = expr.GetPropOrFieldSetter();
        var foo = new Foo();
        setter(foo, new DateOnly(2021, 1, 1));
        foo.BirthDate.Should().Be(new DateOnly(2021, 1, 1));
        setter(foo, new DateOnly(2022, 2, 2));
        foo.BirthDate.Should().Be(new DateOnly(2022, 2, 2));
    }

    [Fact]
    public void GetPropertyGetterDelegate()
    {
        var getter = MemberReflectionExtensions.GetPropOrFieldGetter<Foo, Int32>(x => x.AgeInYears);
        var expected = getter(new Foo { BirthDate = new DateOnly(2021, 1, 1) });
        expected.Should().Be(DateTime.Now.Year - 2021);
    }

    [Fact]
    public void GetPropertyInfo()
    {
        Expression<Func<Foo, String>> expression = x => x.Name;
        var propertyInfo = expression.GetPropertyInfo();
        propertyInfo.Name.Should().Be("Name");
    }

    [Fact]
    public void GetFieldInfo()
    {
        Expression<Func<Foo, DateOnly>> expression = x => x.BirthDate;
        var fieldInfo = expression.GetFieldInfo();
        fieldInfo.Name.Should().Be("BirthDate");
    }

    [Fact]
    public void GetMemberInfo()
    {
        Expression<Func<Foo, DateOnly>> expression = x => x.BirthDate;
        var memberInfo = expression.GetMemberInfo();
        memberInfo.Name.Should().Be("BirthDate");
    }

    [Fact]
    public void GetMemberInfo_WhenExpressionReferencesANonMember_ShouldThrowArgumentException()
    {
        Expression<Func<Foo, DateOnly>> expression = x => DateOnly.MinValue;
        Func<MemberInfo> act = expression.GetMemberInfo;
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetMemberInfo_WhenExpressionReferencesAMemberOfFooButADifferentInstance_ShouldThrowArgumentException()
    {
        Foo foo = new();
        Expression<Func<Foo, DateOnly>> expression = x => foo.BirthDate;
        Func<MemberInfo> act = expression.GetMemberInfo;
        act.Should().Throw<ArgumentException>();
    }
}