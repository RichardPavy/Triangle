namespace Serialization.Deserialize
{
    using System;
    using System.IO;
    using Visitors;

    internal class ObjectDeserializer : GenericFunc<Delegate>
    {
        protected override Delegate Call<TObj>()
        {
            return new ProcessObject<Stream, TObj>(
                (Stream stream, TObj value) =>
                {
                    // TODO: Check there is a '0' at end of object.
                    return VisitStatus.Continue;
                });
        }
    }
}
