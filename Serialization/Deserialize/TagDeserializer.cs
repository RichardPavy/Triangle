namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using System.Reflection;
    using Visitors;

    internal class TagDeserializer : GenericFunc2<Func<PropertyInfo, Delegate, Delegate>>
    {
        private readonly int tag;

        internal TagDeserializer(int tag)
        {
            this.tag = tag;
        }

        protected override Func<PropertyInfo, Delegate, Delegate> Call<TObj, TValue>()
        {
            return (property, deserializer) =>
            {
                var fieldDeserializer = (ProcessField<Stream, TObj, TValue>) deserializer;
                return new ProcessField<Stream, TObj, TValue>(
                    (Stream stream, TObj obj, ref TValue value) =>
                    {
                        int actualTag = PrimitiveDeserializer.Impl<int>.Instance(stream);
                        if (tag != actualTag)
                        {
                            if (actualTag == 0)
                            {
                                stream.Seek(-1, SeekOrigin.Current);
                                return VisitStatus.SkipChildren;
                            }

                            throw new InvalidOperationException(
                                $"Expected tag {tag}, got {actualTag}, for {property.DeclaringType}.{property.Name}");
                        }
                        return fieldDeserializer(stream, obj, ref value);
                    });
            };
        }
    }
}
