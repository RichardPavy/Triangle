namespace Serialization
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Serialization.Serialize;
    using Visitors;

    public static class Serializer
    {
        public static byte[] Serialize<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                SerializerVisitorFactory<T>.Visitor.Visit(stream, value);
                return stream.ToArray();
            }
        }

        public static void Serialize<T>(T value, Stream stream)
        {
            SerializerVisitorFactory<T>.Visitor.Visit(stream, value);
        }

        private static class SerializerVisitorFactory<T>
        {
            internal static readonly ClassVisitor<Stream, T> Visitor = visitorFactory.GetClassVisitor<T>();
        }

        private static readonly VisitorFactory<Stream> visitorFactory =
            new VisitorFactory<Stream>(
                type =>
                {
                    if (type.IsPrimitive)
                    {
                        return new PrimitiveSerializer().Call(type);
                    }
                    if (type == typeof(string))
                    {
                        return StringSerializer.Instance;
                    }
                    if (type.IsValueType)
                    {
                        return new StructSerializer().Call(type);
                    }
                    if (type.HasSerializableFields())
                    {
                        return new ObjectSerializer().Call(type);
                    }
                    throw new InvalidOperationException($"Unable to create serializer for ${type}");
                },
                property =>
                {
                    var tag = property.GetCustomAttributes<TagAttribute>().SingleOrDefault()?.Tag;
                    if (tag != null)
                    {
                        return TagSerializer.Create(tag.Value).Call(property);
                    }
                    return MustVisitStatus.No;
                });
    }
}
