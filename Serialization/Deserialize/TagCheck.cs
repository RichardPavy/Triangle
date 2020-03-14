namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using System.Reflection;
    using Visitors;

    internal class TagCheck : GenericFunc2<Func<PropertyInfo, Delegate>>
    {
        private readonly int tag;

        internal TagCheck(int tag)
        {
            this.tag = tag;
        }

        protected override Func<PropertyInfo, Delegate> Call<TObj, TValue>()
        {
            return property =>
            {
                return new ProcessField<MemoryStream, TObj, TValue>(
                    (MemoryStream stream, TObj obj, TValue value) =>
                    {
                        int actualTag = PrimitiveDeserializer.Read<int>(stream);
                        if (tag != actualTag)
                        {
                            throw new InvalidOperationException(
                                $"Expected tag {tag}, got {actualTag}, for {property.DeclaringType}.{property.Name}");
                        }
                    });
            };
        }
    }
}
