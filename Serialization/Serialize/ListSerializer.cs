namespace Triangle.Serialization.Serialize
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Triangle.Visitors;

    internal class ListSerializer : GenericFunc<Delegate>
    {
        private VisitorFactory<Stream> visitorFactory;

        public ListSerializer(VisitorFactory<Stream> visitorFactory)
        {
            this.visitorFactory = visitorFactory;
        }

        protected override Delegate Call<TObj>()
        {
            ClassVisitor<Stream, TObj> elementVisitor = visitorFactory.GetClassVisitor<TObj>();
            return new ProcessObject<Stream, IList<TObj>>(
                (Stream stream, IList<TObj> value) =>
                {
                    PrimitiveSerializer.Impl<int>.Instance(stream, value.Count());
                    foreach (TObj element in value)
                        elementVisitor.Visit(stream, element);
                    return VisitStatus.SkipChildren;
                });
        }
    }
}
