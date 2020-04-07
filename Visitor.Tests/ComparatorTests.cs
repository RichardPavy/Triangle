namespace Serialization.Tests
{
    using Visitors.Comparators;
    using Xunit;

    public class ComparatorTests
    {
        [Fact]
        public void Equality()
        {
            Assert.True(Comparator.Compare(
                new MyClass {},
                new MyClass {}));

            Assert.False(Comparator.Compare(
                new MyClass {
                    MyStringProp = "a"
                },
                new MyClass {}));
            Assert.False(Comparator.Compare(
                new MyClass {
                    MyStringProp = "a"
                },
                new MyClass {
                    MyStringProp = "b"
                }));
            Assert.True(Comparator.Compare(
                new MyClass {
                    MyStringProp = "a"
                },
                new MyClass {
                    MyStringProp = "a"
                }));

            Assert.False(Comparator.Compare(
                new MyClass {
                    MyIntProp = 1
                },
                new MyClass {}));
            Assert.False(Comparator.Compare(
                new MyClass {
                    MyIntProp = 1
                },
                new MyClass {
                    MyIntProp = 2
                }));
            Assert.True(Comparator.Compare(
                new MyClass {
                    MyIntProp = 1
                },
                new MyClass {
                    MyIntProp = 1
                }));

            Assert.True(Comparator.Compare(
                new MyClass {
                    MyStringProp = "a",
                    MyIntProp = 1
                },
                new MyClass {
                    MyStringProp = "a",
                    MyIntProp = 1
                }));
            Assert.False(Comparator.Compare(
                new MyClass {
                    MyStringProp = "a",
                    MyIntProp = 1
                },
                new MyClass {
                    MyStringProp = "a",
                    MyIntProp = 2
                }));

            Assert.True(Comparator.Compare(
                new MyClass {
                    MyStringProp = "a",
                    MyIntProp = 1,
                    MyStruct = new MyStruct { MyIntProp = 3 }
                },
                new MyClass {
                    MyStringProp = "a",
                    MyIntProp = 1,
                    MyStruct = new MyStruct { MyIntProp = 3 }
                }));
            Assert.False(Comparator.Compare(
                new MyClass {
                    MyStringProp = "a",
                    MyIntProp = 1,
                    MyStruct = new MyStruct { MyIntProp = 3 }
                },
                new MyClass {
                    MyStringProp = "a",
                    MyIntProp = 1,
                    MyStruct = new MyStruct { MyIntProp = 4 }
                }));
        }

        private class MyClass
        {
            internal string MyStringProp { get; set; }
            internal int MyIntProp { get; set; }
            internal MyStruct MyStruct { get; set; }
        }

        private struct MyStruct
        {
            internal string MyStringProp { get; set; }
            internal int MyIntProp { get; set; }
            internal MyClass MyClass { get; set; }
        }
    }
}
