namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class PrimitiveSerializer : GenericFuncUnmanaged<Delegate>
    {
        protected override Delegate Call<TPrimitive>()
        {
            return new ProcessObject<Stream, TPrimitive>(Process);
        }

        internal static void Process<TPrimitive>(Stream stream, TPrimitive value)
            where TPrimitive : unmanaged
        {
            unsafe
            {
                void* valuePointer = &value;
                Span<byte> span = new Span<byte>(valuePointer, sizeof(TPrimitive));
                stream.Write(span);
            }
        }

        // Copied from System.IO.System.BinaryWriter
        internal static void Write7BitEncodedInt(int value, Stream stream)
        {
            // Write out an int 7 bits at a time.  The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            uint v = (uint)value;   // support negative numbers
            while (v >= 0x80)
            {
                stream.WriteByte((byte) (v | 0x80));
                v >>= 7;
            }
            stream.WriteByte((byte) v);
        }
    }
}
