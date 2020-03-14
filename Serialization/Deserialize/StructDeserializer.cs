namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Visitors;

    internal class StructDeserializer : GenericFuncStruct2<Func<PropertyInfo, Delegate>>
    {
        protected override Func<PropertyInfo, Delegate> Call<TObj, TStruct>()
        {
            int size = Marshal.SizeOf<TStruct>();
            return property =>
            {
                ISetter<TObj, TStruct> setter = Setter.Create<TObj, TStruct>(property);
                return new ProcessField<MemoryStream, TObj, TStruct>(
                    (MemoryStream stream, TObj obj, TStruct oldValue) =>
                    {
                        byte[] bytes = new byte[size];
                        stream.Read(bytes, 0, size);
                        TStruct newValue = Marshallers<TStruct>.Instance.FromBytes(bytes);
                        setter.Apply(obj, newValue);
                    });
            };
        }
    }
}
