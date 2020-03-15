namespace Serialization.Tests
{
    using System;
    using Serialization;
    using Xunit;

    public class SerializationTests
    {
        [Fact]
        public void Serialize()
        {
            byte[] myClassBytes =
                Serializer.Serialize(new MyClass
                {
                    MyStringProp = "abc",
                    MyIntProp = 123,
                });
            Console.WriteLine($"MyClass bytes = {string.Join(",", myClassBytes)}");
            MyClass myClassDeserialized =
                Deserializer.Deserialize<MyClass>(myClassBytes);
            Assert.Equal("abc", myClassDeserialized.MyStringProp);
            Assert.Equal(123, myClassDeserialized.MyIntProp);
        }

        [AutoSerialize]
        internal class MyClass
        {
            [Tag(1)]
            internal string MyStringProp { get; set; }

            [Tag(2)]
            internal int MyIntProp { get; set; }
        }
    }
}
