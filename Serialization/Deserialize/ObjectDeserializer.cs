namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using System.Reflection;
    using Visitors;

    internal class ObjectDeserializer : GenericFunc2New<Func<PropertyInfo, Delegate>>
    {
        private const int EndTag = 0;

        protected override Func<PropertyInfo, Delegate> Call<TObj, TValue>()
        {
            return property =>
            {
                ISetter<TObj, TValue> setter = Setter.Create<TObj, TValue>(property);
                return new ProcessField<Stream, TObj, TValue>(
                    (Stream stream, TObj obj, ref TValue value) =>
                    {
                        value = new TValue();
                        setter.Apply(obj, value);
                        return new Action(() =>
                        {
                            int tag = stream.ReadByte();
                            if (tag != EndTag)
                            {
                                throw new InvalidOperationException(
                                    $"Expected end tag {EndTag}, got {tag}");
                            }
                        });
                    });
            };
        }
    }
}
