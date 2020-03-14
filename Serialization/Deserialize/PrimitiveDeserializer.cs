namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using System.Reflection;
    using Visitors;

    internal class PrimitiveDeserializer : GenericFunc2Unmanaged<Func<PropertyInfo, Delegate>>
    {
        protected override Func<PropertyInfo, Delegate> Call<TObj, TPrimitive>()
        {
            return property =>
            {
                ISetter<TObj, TPrimitive> setter = Setter.Create<TObj, TPrimitive>(property);
                return new ProcessField<MemoryStream, TObj, TPrimitive>(
                    (MemoryStream stream, TObj obj, TPrimitive oldValue) =>
                    {
                        setter.Apply(obj, Read<TPrimitive>(stream));
                    });
            };
        }

        internal static TPrimitive Read<TPrimitive>(Stream stream)
             where TPrimitive : unmanaged
        {
            TPrimitive newValue;
            unsafe
            {
                void* valuePointer = &newValue;
                Span<byte> span = new Span<byte>(valuePointer, sizeof(TPrimitive));
                stream.Read(span);
            }
            return newValue;
        }
    }
}
