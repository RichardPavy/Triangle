namespace Serialization.Serialize
{
    using System;
    using System.IO;
    using Visitors;

    internal static class TagSerializer
    {
        internal static Impl.TagSerializer<TPrimitive> Create<TPrimitive>(TPrimitive primitive)
            where TPrimitive : unmanaged
        {
            return new Impl.TagSerializer<TPrimitive>(primitive);
        }
    }

    internal static class Impl
    {
        internal class TagSerializer<TPrimitive> : GenericFunc2<Delegate>
            where TPrimitive : unmanaged
        {
            private readonly TPrimitive primitive;

            internal TagSerializer(TPrimitive primitive)
            {
                this.primitive = primitive;
            }

            protected override Delegate Call<TObj, TValue>()
            {
                byte[] array;
                using (var stream = new MemoryStream())
                {
                    PrimitiveSerializer.Impl<TPrimitive>.Instance(stream, primitive);
                    array = stream.ToArray();
                }
                Func<TValue, bool> isDefault =
                    (Func<TValue, bool>)
                    (typeof(TValue).IsValueType
                         ? new IsDefaultValue<TValue>().Call(typeof(IEquatable<TValue>), typeof(TValue))
                         : new IsNull<TValue>().Call(typeof(TValue)));
                return new ProcessField<Stream, TObj, TValue>(
                    (stream, obj, value) =>
                    {
                        if (isDefault(value))
                        {
                            return VisitStatus.SkipChildren;
                        }
                        stream.Write(array);
                        return VisitStatus.Continue;
                    });
            }
        }

        private class IsDefaultValue<TValue> : GenericFunc<Func<TValue, bool>, IEquatable<TValue>>
        {
            protected override Func<TValue, bool> Call<TInput>()
            {
                return (Func<TValue, bool>) (object)
                       new Func<TInput, bool>(value => value.Equals(default));
            }
        }

        private class IsNull<TValue> : GenericFuncClass<Func<TValue, bool>>
        {
            protected override Func<TValue, bool> Call<TInput>()
            {
                return (Func<TValue, bool>) (object)
                       new Func<TInput, bool>(value => value == null);
            }
        }
    }
}
