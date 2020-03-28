namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Visitors;

    internal class StructDeserializer : GenericFunc2Struct<Func<PropertyInfo, Delegate>>
    {
        protected override Func<PropertyInfo, Delegate> Call<TObj, TStruct>()
        {
            int size = Marshal.SizeOf<TStruct>();
            return property =>
            {
                ISetter<TObj, TStruct> setter = Setter.Create<TObj, TStruct>(property);
                return new ProcessField<Stream, TObj, TStruct>(
                    (Stream stream, TObj obj, ref TStruct value) =>
                    {
                        byte[] bytes = new byte[size];
                        stream.Read(bytes, 0, size);
                        value = Marshallers<TStruct>.Instance.FromBytes(bytes);
                        setter.Apply(obj, value);
                        return VisitStatus.SkipChildren;
                    });
            };
        }
    }
}
