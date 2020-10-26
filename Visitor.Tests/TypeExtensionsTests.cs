namespace Triangle.Visitors.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Triangle.Visitors.Utils.Types;
    using Xunit;

    public class TypeExtensionsTests
    {
        [Fact]
        public void GetGenericParentType()
        {
            Assert.Throws<ArgumentException>(
                () => typeof(bool).GetGenericParentType(typeof(bool)));
            Assert.Equal(
                typeof(ISet<bool>),
                typeof(HashSet<bool>).GetGenericParentType(typeof(ISet<>)));
            Assert.Null(
                typeof(List<bool>).GetGenericParentType(typeof(ISet<>)));
            Assert.Equal(
                typeof(IDictionary<int, string>),
                typeof(ConcurrentDictionary<int, string>).GetGenericParentType(typeof(IDictionary<,>)));
        }

        [Fact]
        public void GetAllParentTypes()
        {
            Assert.Equal(
                new[] {
                    typeof(bool),
                    typeof(ValueType),
                    typeof(object),
                    typeof(IComparable),
                    typeof(IConvertible),
                    typeof(IComparable<bool>),
                    typeof(IEquatable<bool>)
                },
                typeof(bool).GetAllParentTypes());
            Assert.Equal(
                new[] {
                    typeof(HashSet<bool>),
                    typeof(object),
                    typeof(ICollection<bool>),
                    typeof(IEnumerable<bool>),
                    typeof(IEnumerable),
                    typeof(ISet<bool>),
                    typeof(IReadOnlyCollection<bool>),
                    typeof(ISerializable),
                    typeof(IDeserializationCallback)
                },
                typeof(HashSet<bool>).GetAllParentTypes());
        }
    }
}
