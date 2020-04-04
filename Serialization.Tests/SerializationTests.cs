namespace Serialization.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
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

        [Fact]
        public void Performance()
        {
            PerformanceData data = null;
            for (int i = 0; i < 10; i++)
            {
                data = new PerformanceData
                {
                    Id = 101,
                    ExternalId = Guid.NewGuid(),
                    Name = "top " + i,
                    Left = data,
                    Right = data
                };
            }

            byte[] bytes = Serializer.Serialize(data);
            PerformanceData data2 = Deserializer.Deserialize<PerformanceData>(bytes);
            byte[] bytes2 = Serializer.Serialize(data2);

            Assert.Equal(
                string.Join(", ", bytes),
                string.Join(", ", bytes2));

            for (int i = 0; i < 3; i++)
            {
                {
                    var speedometer = new Speedometer();
                    while (speedometer.Continue)
                    {
                        var b = Serializer.Serialize(data);
                        speedometer += b.Length;
                    }
                    Console.WriteLine($"Encode speed: {speedometer.Speed / 1024 / 1024}");
                    Console.WriteLine($"Encode speed: {speedometer.Speed / 1024 / 1024} ({speedometer.Amount} runs)");
                }
                {
                    var speedometer = new Speedometer();
                    while (speedometer.Continue)
                    {
                        Deserializer.Deserialize<PerformanceData>(bytes);
                        speedometer += bytes.Length;
                    }
                    Console.WriteLine($"Decode speed: {speedometer.Speed / 1024 / 1024} ({speedometer.Amount} runs)");
                }
                {
                    var speedometer = new Speedometer();
                    while (speedometer.Continue)
                    {
                        var b = Serializer.Serialize(data);
                        Deserializer.Deserialize<PerformanceData>(b);
                        speedometer += b.Length;
                    }
                    Console.WriteLine($"Encode+Decode speed: {speedometer.Speed / 1024 / 1024} ({speedometer.Amount} runs)");
                }
                Thread.Sleep(millisecondsTimeout: 200);
            }
        }

        internal class PerformanceData
        {
            [Tag(1)]
            internal int Id { get; set; }

            [Tag(2)]
            internal Guid ExternalId { get; set; }

            [Tag(3)]
            internal string Name { get; set; }

            [Tag(4)]
            internal PerformanceData Left { get; set; }

            [Tag(5)]
            internal PerformanceData Right { get; set; }
        }

        internal class Speedometer
        {
            private readonly Stopwatch stopwatch = Stopwatch.StartNew();

            private int count;

            public int Amount { get; set; }

            public double Speed => 1000 * ((double) count) / stopwatch.ElapsedMilliseconds;

            public bool Continue => stopwatch.ElapsedMilliseconds < 200;

            public static Speedometer operator +(Speedometer speedometer, int add)
            {
                speedometer.count += add;
                speedometer.Amount++;
                return speedometer;
            }
        }
    }
}
