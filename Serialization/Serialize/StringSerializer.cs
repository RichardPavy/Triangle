namespace Serialization.Serialize
{
    using System.IO;
    using Visitors;

    internal static class StringSerializer
    {
        internal static ProcessObject<Stream, string> Instance { get; } =
            (stream, value) =>
            {
                byte[] bytes = Marshallers<string>.Instance.ToBytes(value);
                int length = bytes.Length;
                PrimitiveSerializer.Impl<int>.Instance(stream, length);
                stream.Write(bytes);
            };
    }
}
