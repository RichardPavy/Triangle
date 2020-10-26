namespace Triangle.Serialization.Serialize
{
    using System.IO;
    using Triangle.Visitors;

    internal static class StringSerializer
    {
        internal static ProcessObject<Stream, string> Instance { get; } =
            (stream, value) =>
            {
                byte[] bytes = Marshallers<string>.Instance.ToBytes(value);
                int length = bytes.Length;
                PrimitiveSerializer.Impl<int>.Instance(stream, length);
                stream.Write(bytes);
                return VisitStatus.SkipChildren;
            };
    }
}
