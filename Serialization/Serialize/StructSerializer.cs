namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class StructSerializer : GenericFuncUnmanaged<Delegate>
    {
        protected override Delegate Call<TStruct>()
        {
            return Impl<TStruct>.Visitor;
        }

        private static class Impl<TStruct>
            where TStruct : struct
        {
            internal static ProcessObject<MemoryStream, TStruct> Visitor { get; } = Process;
        }

        internal static void Process<TStruct>(MemoryStream stream, TStruct value)
            where TStruct : struct
        {
            byte[] valueBytes = Marshallers<TStruct>.Instance.ToBytes(value);
            stream.Write(valueBytes);
        }
    }
}
