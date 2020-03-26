namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class StructSerializer : GenericFuncUnmanaged<Delegate>
    {
        protected override Delegate Call<TStruct>()
        {
            return new ProcessObject<Stream, TStruct>(
                (Stream stream, TStruct value) =>
                {
                    byte[] valueBytes = Marshallers<TStruct>.Instance.ToBytes(value);
                    stream.Write(valueBytes);
                    return VisitStatus.SkipChildren;
                });
        }
    }
}
