namespace Serialization.Tests
{
    using System;
    using Serialization;
    using Xunit;

    public class SerializationTests
    {
        [Fact]
        public void GetterSetter()
        {
            {
                Func<MyClass, string> getter = Getter<MyClass, string>.Create(nameof(MyClass.MyStringProp));
                Action<MyClass, string> setter = Setter<MyClass, string>.Create(nameof(MyClass.MyStringProp));

                var obj = new MyClass();
                Assert.Null(getter(obj));
                setter(obj, "abc");
                Assert.Equal("abc", getter(obj));
                setter(obj, "def");
                Assert.Equal("def", getter(obj));
            }

            {
                Func<MyStruct, string> getter = Getter<MyStruct, string>.Create(nameof(MyStruct.MyStringProp));
                Action<MyStruct, string> setter = Setter<MyStruct, string>.Create(nameof(MyStruct.MyStringProp));

                var obj = new MyStruct();
                Assert.Null(getter(obj));
                setter(obj, "abc");
                Assert.Equal("abc", getter(obj));
                setter(obj, "def");
                Assert.Equal("def", getter(obj));
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
