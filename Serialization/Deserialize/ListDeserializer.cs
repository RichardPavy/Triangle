namespace Triangle.Serialization.Deserialize
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Triangle.Visitors;

    internal class ListDeserializer : GenericFunc2New<Func<PropertyInfo, Delegate>>
    {
        private VisitorFactory<DeserializeContext> visitorFactory;

        public ListDeserializer(VisitorFactory<DeserializeContext> visitorFactory)
        {
            this.visitorFactory = visitorFactory;
        }

        protected override Func<PropertyInfo, Delegate> Call<TObj, TValue>()
        {
            ClassVisitor<DeserializeContext, TValue> elementVisitor = visitorFactory.GetClassVisitor<TValue>();
            return property =>
            {
                ISetter<TObj, IList<TValue>> setter = Setter.Create<TObj, IList<TValue>>(property);
                return new ProcessField<DeserializeContext, TObj, IList<TValue>>(
                    (DeserializeContext context, TObj obj, ref IList<TValue> value) =>
                    {
                        value = new List<TValue>();
                        setter.Apply(obj, value);
                        int length = PrimitiveDeserializer.Impl<int>.Instance(context.Stream);
                        for (int i = 0; i < length; i++)
                        {
                            TValue element = new TValue();
                            if (elementVisitor.Visit(context, element) == VisitStatus.Exit)
                                return VisitStatus.Exit;
                            value.Add(element);
                        }
                        return VisitStatus.SkipChildren;
                    });
            };
        }
    }
}
