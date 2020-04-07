namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class PrimitiveSerializer : GenericFuncUnmanaged<Delegate>
    {
        protected override Delegate Call<TPrimitive>()
        {
            return Impl<TPrimitive>.Instance;
        }

        internal static class Impl<TPrimitive>
            where TPrimitive : unmanaged
        {
            internal static ProcessObject<Stream, TPrimitive> Instance { get; } =
                typeof(TPrimitive) == typeof(int)
                    ? (ProcessObject<Stream, TPrimitive>) (Delegate)
                      new ProcessObject<Stream, int>(Write7BitEncodedInt)
                    : Write;
        }

        private static VisitorScope<Stream> Write<TPrimitive>(Stream stream, TPrimitive value)
            where TPrimitive : unmanaged
        {
            unsafe
            {
                void* valuePointer = &value;
                Span<byte> span = new Span<byte>(valuePointer, sizeof(TPrimitive));
                stream.Write(span);
            }
            return VisitStatus.SkipChildren;
        }

        // Copied from System.IO.System.BinaryWriter
        private static VisitorScope<Stream> Write7BitEncodedInt(Stream stream, int value)
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
            return VisitStatus.SkipChildren;
        }
    }
}
