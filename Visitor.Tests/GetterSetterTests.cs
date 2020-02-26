namespace Serialization.Tests
{
    using System;
    using Visitors;
    using Xunit;

    public class GetterSetterTests
    {
        [Fact]
        public void GetterSetter()
        {
            {
                IGetter<MyClass, string> getter = Getter.Create<MyClass, string>(nameof(MyClass.MyStringProp));
                ISetter<MyClass, string> setter = Setter.Create<MyClass, string>(nameof(MyClass.MyStringProp));
                var obj = new MyClass();
                Assert.Null(getter.Apply(obj));
                setter.Apply(obj, "abc");
                Assert.Equal("abc", getter.Apply(obj));
                setter.Apply(obj, "def");
                Assert.Equal("def", getter.Apply(obj));
            }

            {
                IGetter<MyStruct, string> getter = Getter.Create<MyStruct, string>(nameof(MyStruct.MyStringProp));
                ISetter<MyStruct, string> setter = Setter.Create<MyStruct, string>(nameof(MyStruct.MyStringProp));
                var obj = new MyStruct();
                Assert.Null(getter.Apply(obj));
                setter.Apply(obj, "abc");
                Assert.Equal("abc", getter.Apply(obj));
                setter.Apply(obj, "def");
                Assert.Equal("def", getter.Apply(obj));
            }
        }

        internal class MyClass
        {
            internal string MyStringProp { get; set; }
            internal int MyIntProp { get; set; }
        }

        class MyStruct
        {
            internal string MyStringProp { get; set; }
            internal int MyIntProp { get; set; }
        }
    }
}
