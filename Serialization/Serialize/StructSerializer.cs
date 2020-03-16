﻿namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class StructSerializer : GenericFuncUnmanaged<Delegate>
    {
        protected override Delegate Call<TStruct>()
        {
            return Impl<TStruct>.Instance;
        }

        private static class Impl<TStruct>
            where TStruct : struct
        {
            internal static ProcessObject<Stream, TStruct> Instance { get; } = Process;
        }

        internal static VisitStatus Process<TStruct>(Stream stream, TStruct value)
            where TStruct : struct
        {
            byte[] valueBytes = Marshallers<TStruct>.Instance.ToBytes(value);
            stream.Write(valueBytes);
            return VisitStatus.SkipChildren;
        }
    }
}
