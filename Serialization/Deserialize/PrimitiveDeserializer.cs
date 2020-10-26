namespace Triangle.Serialization.Deserialize
{
    using System;
    using System.IO;
    using System.Reflection;
    using Triangle.Visitors;

    internal class PrimitiveDeserializer : GenericFunc2Unmanaged<Func<PropertyInfo, Delegate>>
    {
        protected override Func<PropertyInfo, Delegate> Call<TObj, TPrimitive>()
        {
            return property =>
            {
                ISetter<TObj, TPrimitive> setter = Setter.Create<TObj, TPrimitive>(property);
                return new ProcessField<DeserializeContext, TObj, TPrimitive>(
                    (DeserializeContext context, TObj obj, ref TPrimitive value) =>
                    {
                        value = Impl<TPrimitive>.Instance(context.Stream);
                        setter.Apply(obj, value);
                        return VisitStatus.SkipChildren;
                    });
            };
        }

        internal static class Impl<TPrimitive>
            where TPrimitive : unmanaged
        {
            internal static Func<Stream, TPrimitive> Instance { get; } =
                typeof(TPrimitive) == typeof(int)
                    ? (Func<Stream, TPrimitive>) (Delegate) new Func<Stream, int>(Read7BitEncodedInt)
                    : Read<TPrimitive>;
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

        // Copied from System.IO.System.BinaryReader
        internal static int Read7BitEncodedInt(Stream stream)
        {
            // Read out an Int32 7 bits at a time.  The high bit
            // of the byte when on means to continue reading more bytes.
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                // Check for a corrupted stream.  Read a max of 5 bytes.
                // In a future version, add a DataFormatException.
                if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
                    throw new FormatException("Bad 7-bit encoded integer");

                // ReadByte handles end of stream cases for us.
                b = (byte) stream.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }
    }
}
