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
                "1, 3, 97, 98, 99, 2, 128, 4, 123, 0, 0",
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
            Assert.Equal(
                "3, 7, 0",
                string.Join(", ", Serializer.Serialize(new MyClass2 { MyIntProp = 7 } )));
            var exception = Assert.Throws<InvalidOperationException>(
                () => Deserializer.Deserialize<MyClass2>(new byte[] { 3, 7, 9 }));
            Assert.Contains("Expected end tag 0, got 9", exception.Message);
        }

        internal class MyBadTagClass
        {
            [Tag(1)]
            internal MyClass2 MyClass2 { get; set; }
        }

        [Fact]
        public void Serialize_UnreadBytes()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => Deserializer.Deserialize<MyClass>(
                    myClassBytes.Concat(new byte[] { 1, 2, 3, 4 }).ToArray()));
            Assert.Contains("There were 4 bytes remaining in the stream.", exception.Message);
        }

        [Fact]
        public void Serialize_Recursive()
        {
            var obj =
                new MyClass
                {
                    MyStringProp = "abc",
                    MyIntProp = 512,
                    NoTag = 123,
                    MyClass2 = new MyClass2
                    {
                        MyStringProp = "def",
                        MyIntProp = 513,
                        MyClass = new MyClass
                        {
                            MyStringProp = "ghi",
                            MyIntProp = 514,
                        }
                    }
                };
            byte[] bytes = Serializer.Serialize(obj);
            MyClass deserialized = Deserializer.Deserialize<MyClass>(bytes);
            Assert.Equal("abc", deserialized.MyStringProp);
            Assert.Equal(512, deserialized.MyIntProp);
            Assert.Equal(123, deserialized.NoTag);

            Assert.Equal("def", deserialized.MyClass2.MyStringProp);
            Assert.Equal(513, deserialized.MyClass2.MyIntProp);

            Assert.Equal("ghi", deserialized.MyClass2.MyClass.MyStringProp);
            Assert.Equal(514, deserialized.MyClass2.MyClass.MyIntProp);
            Assert.Equal(0, deserialized.MyClass2.MyClass.NoTag);

            Assert.NotNull(deserialized.MyClass2.MyClass.MyClass2);
            Assert.Null(deserialized.MyClass2.MyClass.MyClass2.MyStringProp);
        }

        [Fact]
        public void Serialize_Sparse()
        {
            {
                var obj = new MyClass2 { MyIntProp = 513 };
                byte[] bytes = Serializer.Serialize(obj);
                MyClass2 deserialized = Deserializer.Deserialize<MyClass2>(bytes);
                Assert.Null(deserialized.MyStringProp);
                Assert.Equal(513, deserialized.MyIntProp);
                Assert.Null(deserialized.MyClass);
            }
            {
                var obj = new MyClass2
                {
                    MyClass = new MyClass
                    {
                        MyStringProp = "ghi",
                        MyIntProp = 514,
                    }
                };
                byte[] bytes = Serializer.Serialize(obj);
                MyClass2 deserialized = Deserializer.Deserialize<MyClass2>(bytes);
                Assert.Null(deserialized.MyStringProp);
                Assert.Equal(0, deserialized.MyIntProp);
                Assert.Equal("ghi", deserialized.MyClass.MyStringProp);
                Assert.Equal(514, deserialized.MyClass.MyIntProp);
            }
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

            [Tag(4)]
            internal MyClass MyClass { get; set; }
        }

        [Fact]
        public void ParentChild()
        {
            var parent = new ParentKey { Domain = 100, Id = 101 };
            var child = new ChildKey { Parent = parent, Id = 102 };

            byte[] parentBytes = Serializer.Serialize(parent);
            byte[] childBytes = Serializer.Serialize(child);

            Assert.Equal(
                "1, 100, 2, 101, 0",
                string.Join(", ", parentBytes));
            Assert.Equal(
                "1, 100, 2, 101, 0, 3, 102, 0",
                string.Join(", ", childBytes));
        }

        internal class ParentKey
        {
            [Tag(1)]
            internal int Domain { get; set; }

            [Tag(2)]
            internal int Id { get; set; }
        }

        internal class ChildKey
        {
            internal ParentKey Parent { get; set; }

            [Tag(3)]
            internal int Id { get; set; }
        }
    }
}
