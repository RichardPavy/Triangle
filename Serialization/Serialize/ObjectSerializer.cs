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
                    Action writeEndOfObject = () => stream.WriteByte(0);
                    VisitorScope<Stream> scope =
                        value != null
                            ? VisitStatus.Continue
                            : VisitStatus.SkipChildren;
                    return scope + writeEndOfObject;
                });
        }
    }
}
