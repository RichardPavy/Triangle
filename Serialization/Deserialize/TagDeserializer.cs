namespace Triangle.Serialization.Deserialize
{
    using System;
    using System.Reflection;
    using Triangle.Visitors;

    internal class TagDeserializer : GenericFunc2<Func<PropertyInfo, Delegate, Delegate>>
    {
        private readonly int tag;

        internal TagDeserializer(int tag)
        {
            this.tag = tag;
        }

        protected override Func<PropertyInfo, Delegate, Delegate> Call<TObj, TValue>()
        {
            return (property, deserializer) =>
            {
                var fieldDeserializer = (ProcessField<DeserializeContext, TObj, TValue>) deserializer;
                return new ProcessField<DeserializeContext, TObj, TValue>(
                    (DeserializeContext context, TObj obj, ref TValue value) =>
                    {
                        int actualTag = context.LastTag;
                        if (tag != actualTag)
                            return VisitStatus.SkipChildren;

                        context.ClearLastTag();
                        return fieldDeserializer(context, obj, ref value);
                    });
            };
        }
    }
}
