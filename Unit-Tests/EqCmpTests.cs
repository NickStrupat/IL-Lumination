using Illumination;
using Xunit;

namespace Unit_Tests;

public class EqCmpTests
{
    [Fact]
    public void Foo()
    {
        var foo1 = new Foo { Bar = "bar", Baz = 42 };
        var foo2 = new Foo { Bar = "bar", Baz = 42 };
        //Assert.Equal(foo1, foo2);
        Assert.True(foo1.Equals(foo2));
        var hashCode = foo1.GetHashCode();
        var actual = foo2.GetHashCode();
        Assert.Equal(hashCode, actual);
    }
    
    [Fact]
    public void FooNotEqual()
    {
        var foo1 = new Foo { Bar = "bar", Baz = 42 };
        var foo2 = new Foo { Bar = "bar", Baz = 43 };
        //Assert.NotEqual(foo1, foo2);
        Assert.False(foo1.Equals(foo2));
        var hashCode = foo1.GetHashCode();
        var actual = foo2.GetHashCode();
        Assert.NotEqual(hashCode, actual);
    }
}