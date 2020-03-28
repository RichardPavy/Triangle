namespace Serialization
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Serialization.Deserialize;
    using Visitors;

    public static class Deserializer
    {
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

        public static T Deserialize<T>(Stream stream)
            where T : new()
        {
            T value = new T();
            DeserializerVisitorFactory<T>.Visitor.Visit(stream, value);
            return value;
        }

        private static class DeserializerVisitorFactory<T>
        {
            internal static ClassVisitor<Stream, T> Visitor = visitorFactory.GetClassVisitor<T>();
        }

        private static readonly VisitorFactory<Stream> visitorFactory =
            new VisitorFactory<Stream>(
                type =>
                {
                    if (type.IsPrimitive || type == typeof(string) || type.IsValueType)
                    {
                        return MustVisitStatus.Never;
                    }
                    else if (type.SerializableFields().Any())
                    {
                        return MustVisitStatus.No;
                    }
                    throw new InvalidOperationException($"Unable to create deserializer for {type}");
                },
                property =>
                {
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
                        return new ObjectDeserializer().Call(property)(property);
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
}
