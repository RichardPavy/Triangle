namespace Triangle.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Triangle.Serialization.Serialize;
    using Triangle.Visitors;
    using Triangle.Visitors.Utils.Types;

    /// <summary>
    ///   Implementation of serlialization logic.
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        ///   Serializes an object.
        /// </summary>
        public static byte[] Serialize<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                SerializerVisitorFactory<T>.Visitor.Visit(stream, value);
                return stream.ToArray();
            }
        }

        /// <summary>
        ///   Appends the serialized representation of an object into the given stream.
        /// </summary>
        public static void Serialize<T>(T value, Stream stream)
        {
            SerializerVisitorFactory<T>.Visitor.Visit(stream, value);
        }

        private static class SerializerVisitorFactory<T>
        {
            internal static readonly ClassVisitor<Stream, T> Visitor = visitorFactory.GetClassVisitor<T>();
        }

        /// <summary>
        ///   The visitor factory for serializers.
        /// </summary>
        /// <remarks>
        ///   The visitor traverses the datastructure and appends the serialization logic into the stream.
        ///   Fields that are annotated with tags are optional: For default values, <see cref="TagSerializer"/>
        ///   skips the class serializer for the field so nothing is written to the stream.
        /// </remarks>
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
                    else if (type.GetGenericParentType(typeof(IList<>)) != null)
                    {
                        Type elementType = type.GetGenericParentType(typeof(IList<>)).GetGenericArguments().Single();
                        return new ListSerializer(visitorFactory).Call(elementType);
                    }
                    if (type.SerializableFields().Any())
                    {
                        return new ObjectSerializer().Call(type);
                    }
                    throw new InvalidOperationException($"Unable to create serializer for ${type}");
                },
                property =>
                {
                    if (property.DeclaringType.GetGenericParentType(typeof(IList<>)) != null)
                    {
                        return MustVisitStatus.Never;
                    }
                    var tag = property.GetCustomAttributes<TagAttribute>().SingleOrDefault()?.Tag;
                    if (tag != null)
                    {
                        return TagSerializer.Create(tag.Value).Call(property);
                    }
                    return MustVisitStatus.No;
                });
    }
}
