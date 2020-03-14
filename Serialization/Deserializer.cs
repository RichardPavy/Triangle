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
                    if (type.GetCustomAttributes(typeof(AutoSerializeAttribute)).Any())
                    {
                        // Don't serialize the object itself. Will serialize field by field.
                        return MustVisitStatus.No;
                    }
                    throw new InvalidOperationException($"Unable to create serializer for {type}");
                },
                property =>
                {
                    var tag = property.GetCustomAttributes(typeof(TagAttribute)).Cast<TagAttribute>().SingleOrDefault()?.Tag;
                    if (tag != null)
                    {
                        return new TagCheck(tag.Value).Call(property);
                    }
                    if (property.PropertyType.IsPrimitive)
                    {
                        return new PrimitiveDeserializer().Call(property);
                    }
                    if (property.PropertyType == typeof(string))
                    {
                        return new StringDeserializer().Call(property.DeclaringType)(property);
                    }
                    if (property.PropertyType.IsValueType)
                    {
                        return new StructDeserializer().Call(property)(property);
                    }
                    return MustVisitStatus.Never;
                });
    }
}
