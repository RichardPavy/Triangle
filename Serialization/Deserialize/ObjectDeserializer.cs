namespace Triangle.Serialization.Deserialize
{
    using System;
    using System.Reflection;
    using Triangle.Visitors;

    internal class ObjectDeserializer : GenericFunc2New<Func<PropertyInfo, Delegate>>
    {
        protected override Func<PropertyInfo, Delegate> Call<TObj, TValue>()
        {
            return property =>
            {
                ISetter<TObj, TValue> setter = Setter.Create<TObj, TValue>(property);
                return new ProcessField<DeserializeContext, TObj, TValue>(
                    (DeserializeContext context, TObj obj, ref TValue value) =>
                    {
                        value = new TValue();
                        setter.Apply(obj, value);
                        return VisitStatus.Continue;
                    });
            };
        }
    }

    internal class EndOfObjectDeserializer : GenericFunc<Delegate>
    {
        private const int EndTag = 0;

        protected override Delegate Call<TObj>()
        {
            return new ProcessObject<DeserializeContext, TObj>(
                (DeserializeContext context, TObj value) =>
                {
                    return new Action(() =>
                    {
                        int tag = context.LastTag;
                        if (tag != EndTag)
                        {
                            throw new InvalidOperationException(
                                $"Expected end tag {EndTag}, got {tag}, in {typeof(TObj)}");
                        }
                        context.ClearLastTag();
                    });
                });
        }
    }
}
