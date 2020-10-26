namespace Triangle.Serialization
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Triangle.Serialization.Deserialize;
    using Triangle.Visitors;

    /// <summary>
    ///   Implementation of deserialization logic.
    /// </summary>
    public static class Deserializer
    {
        /// <summary>
        ///   Deserializes a byte array.
        /// </summary>
        public static T Deserialize<T>(byte[] bytes)
            where T : new()
        {
            using (var stream = new MemoryStream(bytes))
            {
                var result = Deserialize<T>(stream);
                if (stream.Length != stream.Position)
                {
                    throw new InvalidOperationException(
                        $"There were {stream.Length - stream.Position} bytes remaining in the stream.");
                }
                return result;
            }
        }

        /// <summary>
        ///   Deserializes a stream.
        /// </summary>
        public static T Deserialize<T>(Stream stream)
            where T : new()
        {
            T value = new T();
            var context = new DeserializeContext(stream);
            DeserializerVisitorFactory<T>.Visitor.Visit(context, value);
            if (context.LastTagOrNull != null)
            {
                throw new InvalidOperationException(
                    $"Expected to read more data after tag {context.LastTagOrNull}.");
            }
            return value;
        }

        private static class DeserializerVisitorFactory<T>
        {
            internal static ClassVisitor<DeserializeContext, T> Visitor = visitorFactory.GetClassVisitor<T>();
        }

        /// <summary>
        ///   The visitor factory for deserializers.
        /// </summary>
        /// <remarks>
        ///   Deserialization logic attempts to deserialize fields one after the other.
        ///
        ///   For fields that have tags, the algorithm deserializes an integer, and if tags match,
        ///   proceeds with deserializing the value, else continues with the next field.
        ///
        ///   So the complexity is O(N), where N is the number of fields.
        /// </remarks>
        private static readonly VisitorFactory<DeserializeContext> visitorFactory =
            new VisitorFactory<DeserializeContext>(
                type =>
                {
                    if (type.IsPrimitive || type == typeof(string) || type.IsValueType)
                    {
                        return MustVisitStatus.Never;
                    }
                    else if (type.SerializableFields().Any())
                    {
                        return new EndOfObjectDeserializer().Call(type);
                    }
                    throw new InvalidOperationException($"Unable to create deserializer for {type}");
                },
                property =>
                {
                    // TODO: Add support for collections (List, Dictionary)
                    Delegate deserializer;
                    if (property.PropertyType.IsPrimitive)
                    {
                        deserializer = new PrimitiveDeserializer().Call(property)(property);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        deserializer = new StringDeserializer().Call(property.DeclaringType)(property);
                    }
                    else if (property.PropertyType.IsValueType)
                    {
                        deserializer = new StructDeserializer().Call(property)(property);
                    }
                    else if (property.PropertyType.SerializableFields().Any())
                    {
                        deserializer = new ObjectDeserializer().Call(property)(property);
                    }
                    else
                    {
                        return MustVisitStatus.Never;
                    }

                    var tag = property.GetCustomAttributes(typeof(TagAttribute)).Cast<TagAttribute>().SingleOrDefault()?.Tag;
                    if (tag != null)
                    {
                        deserializer = new TagDeserializer(tag.Value).Call(property)(
                            property,
                            deserializer);
                    }
                    return deserializer;
                });
    }

    /// <summary>
    ///   The deserialization context keeps track of the last tag that was parsed.
    ///   When the next tag doesn't match the current field, algorithm continues with the next field,
    ///   but should not attempt to read the next tag from the stream. It should use the last parsed tag
    ///   until the corresponding field is found.
    /// </summary>
    internal class DeserializeContext
    {
        internal Stream Stream { get; }

        internal int LastTag
        {
            get
            {
                if (LastTagOrNull == null)
                {
                    int tag = PrimitiveDeserializer.Impl<int>.Instance(Stream);
                    LastTagOrNull = tag;
                    return tag;
                }
                return LastTagOrNull.Value;
            }
        }

        internal int? LastTagOrNull { get; private set; }

        internal DeserializeContext ClearLastTag()
        {
            LastTagOrNull = null;
            return this;
        }

        internal DeserializeContext(Stream stream)
        {
            Stream = stream;
        }
    }
}
