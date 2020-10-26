namespace Triangle.Serialization.Serialize
{
    using System;
    using System.IO;
    using Triangle.Visitors;

    internal static class TagSerializer
    {
        /// <summary>
        ///   Creates a tag serializer.
        /// </summary>
        /// <typeparam name="TPrimitive">The type of the tag (integer)</typeparam>
        /// <param name="primitive">The tag value</param>
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
                    typeof(TValue).IsValueType
                        ? new IsDefaultValue<TValue>().Call(typeof(IEquatable<TValue>), typeof(TValue))
                        : new IsNull<TValue>().Call(typeof(TValue));
                return new ProcessField<Stream, TObj, TValue>(
                    (Stream stream, TObj obj, ref TValue value) =>
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

        /// <summary>
        ///   For value types, fields with tags ignore the default value.
        /// </summary>
        private class IsDefaultValue<TValue> : GenericFunc<Func<TValue, bool>, IEquatable<TValue>>
        {
            protected override Func<TValue, bool> Call<TInput>()
            {
                return (Func<TValue, bool>) (object)
                       new Func<TInput, bool>(value => value.Equals(default));
            }
        }

        /// <summary>
        ///   For reference types, fields with tags ignore the null value.
        /// </summary>
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
