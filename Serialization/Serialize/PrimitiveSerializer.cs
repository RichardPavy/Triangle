namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class PrimitiveSerializer : GenericFuncUnmanaged<Delegate>
    {
        protected override Delegate Call<TPrimitive>()
        {
            return Impl<TPrimitive>.Visitor;
        }

        private static class Impl<TPrimitive>
            where TPrimitive : unmanaged
        {
            internal static ProcessObject<MemoryStream, TPrimitive> Visitor { get; } = Process;
        }

        internal static void Process<TPrimitive>(MemoryStream stream, TPrimitive value)
            where TPrimitive : unmanaged
        {
            unsafe
            {
                void* valuePointer = &value;
                Span<byte> span = new Span<byte>(valuePointer, sizeof(TPrimitive));
                stream.Write(span);
            }
        }
    }
}
