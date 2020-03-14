namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using System.Reflection;
    using Visitors;

    internal class StringDeserializer : GenericFunc<Func<PropertyInfo, Delegate>>
    {
        protected override Func<PropertyInfo, Delegate> Call<TObj>()
        {
            return property =>
            {
                ISetter<TObj, string> setter = Setter.Create<TObj, string>(property);
                return new ProcessField<MemoryStream, TObj, string>(
                    (MemoryStream stream, TObj obj, string oldValue) =>
                    {
                        int length = PrimitiveDeserializer.Read<int>(stream);
                        byte[] bytes = new byte[length];
                        stream.Read(bytes, 0, length);
                        string newValue = Marshallers<string>.Instance.FromBytes(bytes);
                        setter.Apply(obj, newValue);
                    });
            };
        }
    }
}
