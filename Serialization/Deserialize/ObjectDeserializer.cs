namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class ObjectDeserializer : GenericFunc<Delegate>
    {
        private const int EndTag = 0;

        protected override Delegate Call<TObj>()
        {
            return new ProcessObject<Stream, TObj>(
                (Stream stream, TObj value) =>
                {
                    return new VisitorScope(() =>
                    {
                        int tag = stream.ReadByte();
                        if (tag != EndTag)
                        {
                            throw new InvalidOperationException(
                                $"Expected end tag {EndTag}, got {tag}");
                        }
                    });
                });
        }
    }
}
