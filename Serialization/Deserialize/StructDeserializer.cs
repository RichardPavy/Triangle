namespace Triangle.Serialization.Deserialize
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Triangle.Visitors;

    internal class StructDeserializer : GenericFunc2Struct<Func<PropertyInfo, Delegate>>
    {
        protected override Func<PropertyInfo, Delegate> Call<TObj, TStruct>()
        {
            int size = Unsafe.SizeOf<TStruct>();
            return property =>
            {
                ISetter<TObj, TStruct> setter = Setter.Create<TObj, TStruct>(property);
                return new ProcessField<DeserializeContext, TObj, TStruct>(
                    (DeserializeContext context, TObj obj, ref TStruct value) =>
                    {
                        byte[] bytes = new byte[size];
                        context.Stream.Read(bytes, 0, size);
                        value = Marshallers<TStruct>.Instance.FromBytes(bytes);
                        setter.Apply(obj, value);
                        return VisitStatus.SkipChildren;
                    });
            };
        }
    }
}
