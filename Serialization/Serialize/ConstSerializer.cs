namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal static class ConstSerializer
    {
        internal static ConstSerializer<TPrimitive> Create<TPrimitive>(TPrimitive primitive)
            where TPrimitive : unmanaged
        {
            return new ConstSerializer<TPrimitive>(primitive);
        }
    }

    internal class ConstSerializer<TPrimitive> : GenericFunc<Delegate>
        where TPrimitive : unmanaged
    {
        private readonly TPrimitive primitive;

        internal ConstSerializer(TPrimitive primitive)
        {
            this.primitive = primitive;
        }

        protected override Delegate Call<TObj>()
        {
            byte[] array;
            unsafe
            {
                TPrimitive primitiveLocal = primitive;
                void* primitivePointer = &primitiveLocal;
                Span<byte> span = new Span<byte>(primitivePointer, sizeof(TPrimitive));
                array = span.ToArray();
            }
            return new ProcessField<MemoryStream, TObj, TPrimitive>((stream, obj, value) => stream.Write(array));
        }
    }
}
