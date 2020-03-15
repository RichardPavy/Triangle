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
                    if (type.GetCustomAttributes<AutoSerializeAttribute>().Any())
                    {
                        // Don't serialize the object itself. Will serialize field by field.
                        return MustVisitStatus.No;
                    }
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
                    throw new InvalidOperationException($"Unable to create serializer for ${type}");
                },
                property =>
                {
                    var tag = property.GetCustomAttributes(typeof(TagAttribute)).Cast<TagAttribute>().SingleOrDefault()?.Tag;
                    if (tag != null)
                    {
                        return ConstSerializer.Create(tag.Value).Call(property);
                    }
                    return MustVisitStatus.Never;
                });
    }
}
