namespace Triangle.Serialization.Deserialize
{
    using System;
    using System.Reflection;
    using Triangle.Visitors;

    internal class StringDeserializer : GenericFunc<Func<PropertyInfo, Delegate>>
    {
        protected override Func<PropertyInfo, Delegate> Call<TObj>()
        {
            return property =>
            {
                ISetter<TObj, string> setter = Setter.Create<TObj, string>(property);
                return new ProcessField<DeserializeContext, TObj, string>(
                    (DeserializeContext context, TObj obj, ref string value) =>
                    {
                        int length = PrimitiveDeserializer.Impl<int>.Instance(context.Stream);
                        byte[] bytes = new byte[length];
                        context.Stream.Read(bytes, 0, length);
                        value = Marshallers<string>.Instance.FromBytes(bytes);
                        setter.Apply(obj, value);
                        return VisitStatus.SkipChildren;
                    });
            };
        }
    }
}
