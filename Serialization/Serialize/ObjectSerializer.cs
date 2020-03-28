namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class ObjectSerializer : GenericFunc<Delegate>
    {
        protected override Delegate Call<TObj>()
        {
            return new ProcessObject<Stream, TObj>(
                (Stream stream, TObj value) =>
                {
                    if (value == null)
                    {
                        return VisitStatus.SkipChildren;
                    }

                    return new VisitorScope(() => stream.WriteByte(0));
                });
        }
    }
}
