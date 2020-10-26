namespace Triangle.Visitors.Tests
{
    using System;
    using Triangle.Visitors.Utils;
    using Xunit;

    public class OptionalTests
    {
        [Fact]
        public void Boolean()
        {
            {
                Optional<bool> t = Optional.From(true);
                Assert.True(t.HasValue);
                Assert.True(t.Value);

                Optional<bool> f = Optional.From(false);
                Assert.True(f.HasValue);
                Assert.False(f.Value);

                Optional<bool> n = Optional.FromNullable<bool>(null);
                Assert.False(n.HasValue);
                Assert.Throws<InvalidOperationException>(() => n.Value);
            }
        }

        [Fact]
        public void Class()
        {
            {
                Optional<MyClass> t = Optional.From(new MyClass());
                Assert.True(t.HasValue);
                Assert.IsType<MyClass>(t.Value);

                Optional<MyClass> n = Optional.From(default(MyClass));
                Assert.False(n.HasValue);
                Assert.Throws<InvalidOperationException>(() => n.Value);
            }
        }

        [Fact]
        public void Struct()
        {
            {
                Optional<MyStruct> t = Optional.From(new MyStruct());
                Assert.True(t.HasValue);
                Assert.IsType<MyStruct>(t.Value);

                Optional<MyStruct> f = Optional.From(default(MyStruct));
                Assert.True(t.HasValue);
                Assert.IsType<MyStruct>(t.Value);

                Optional<MyStruct> n = Optional.FromNullable(default(MyStruct?));
                Assert.False(n.HasValue);
                Assert.Throws<InvalidOperationException>(() => n.Value);
            }
        }

        private class MyClass
        {
        }

        private struct MyStruct
        {
        }
    }
}
