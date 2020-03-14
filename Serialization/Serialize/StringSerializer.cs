namespace Serialization.Serialize
{
    using System.IO;
    using Visitors;

    internal static class StringSerializer
    {
        internal static ProcessObject<MemoryStream, string> Instance { get; } =
            (stream, value) =>
            {
                byte[] bytes = Marshallers<string>.Instance.ToBytes(value);
                PrimitiveSerializer.Process(stream, bytes.Length);
                stream.Write(bytes);
            };
    }
}
