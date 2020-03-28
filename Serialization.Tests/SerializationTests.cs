namespace Serialization.Tests
{
    using System;
    using System.Linq;
    using Serialization;
    using Xunit;

    public class SerializationTests
    {
        private readonly byte[] myClassBytes =
            Serializer.Serialize(new MyClass
            {
                MyStringProp = "abc",
                MyIntProp = 512,
                NoTag = 123,
            });

        [Fact]
        public void Serialize()
        {
            Assert.Equal(
                "1, 3, 97, 98, 99, 2, 128, 4, 123, 0",
                string.Join(", ", myClassBytes));
        }

        [Fact]
        public void Deserialize()
        {
            MyClass myClassDeserialized =
                Deserializer.Deserialize<MyClass>(myClassBytes);
            Assert.Equal("abc", myClassDeserialized.MyStringProp);
            Assert.Equal(512, myClassDeserialized.MyIntProp);
            Assert.Equal(123, myClassDeserialized.NoTag);
        }

        [Fact]
        public void Serialize_BadTag()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => Deserializer.Deserialize<MyClass2>(myClassBytes));
            Assert.Contains("Expected tag 3, got 2", exception.Message);
        }

        [Fact]
        public void Serialize_UnreadBytes()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => Deserializer.Deserialize<MyClass>(
                    myClassBytes.Concat(new byte[] { 1, 2, 3, 4 }).ToArray()));
            Assert.Contains("There were 4 bytes remaining in the stream.", exception.Message);
        }

        internal class MyClass
        {
            [Tag(1)]
            internal string MyStringProp { get; set; }

            [Tag(2)]
            internal int MyIntProp { get; set; }

            internal int NoTag { get; set; }

            internal MyClass2 MyClass2 { get; set; }
        }

        internal class MyClass2
        {
            [Tag(1)]
            internal string MyStringProp { get; set; }

            [Tag(3)]
            internal int MyIntProp { get; set; }
        }
    }
}
