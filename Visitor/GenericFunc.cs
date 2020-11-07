namespace Triangle.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public abstract class AbstractGenericFunc<TSelf, TOutput>
        where TSelf : AbstractGenericFunc<TSelf, TOutput>
    {
        internal AbstractGenericFunc() {}

        public TOutput Call(params Type[] types)
        {
            Type[] typeParameters = new Type[] { typeof(TOutput) }.Concat(types).ToArray();
            try
            {
                GenericFuncCaller genericFuncCaller =
                    (GenericFuncCaller)Activator.CreateInstance(
                        GenericFuncCallerType.MakeGenericType(typeParameters),
                        true);
                return genericFuncCaller.Call(Self);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Unable to create generic type {GenericFuncCallerType}" +
                    $" with arguments [\n\t{string.Join(",\n\t", typeParameters.Select(x => x.ToString()))}\n]",
                    exception);
            }
        }

        [GenericFuncCaller]
        protected abstract class GenericFuncCaller
        {
            internal abstract TOutput Call(TSelf genericFunc);
        }

        private TSelf Self => (TSelf)this;
        private Type GenericFuncCallerType =>
            GetType()
                .GetTypeHierarchy()
                .SelectMany(type => type.GetNestedTypes(BindingFlags.NonPublic))
                .Where(type => type.GetCustomAttribute<GenericFuncCallerAttribute>(inherit: true) != null)
                .First();
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    internal class GenericFuncCallerAttribute : Attribute
    {
    }

    public abstract class GenericFunc<TOutput> : AbstractGenericFunc<GenericFunc<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TInput>();

        private sealed class GenericFuncCaller<TInput> : GenericFuncCaller
        {
            internal override TOutput Call(GenericFunc<TOutput> genericFunc) => genericFunc.Call<TInput>();
        }
    }

    public abstract class GenericFunc<TOutput, TSuper> : AbstractGenericFunc<GenericFunc<TOutput, TSuper>, TOutput>
    {
        protected abstract TOutput Call<TInput>() where TInput : TSuper;

        private sealed class GenericFuncCaller<TInput> : GenericFuncCaller
             where TInput : TSuper
        {
            internal override TOutput Call(GenericFunc<TOutput, TSuper> genericFunc) => genericFunc.Call<TInput>();
        }
    }

    public abstract class GenericFuncUnmanaged<TOutput> : AbstractGenericFunc<GenericFuncUnmanaged<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TInput>() where TInput : unmanaged;

        private sealed class GenericFuncCaller<TInput> : GenericFuncCaller
             where TInput : unmanaged
        {
            internal override TOutput Call(GenericFuncUnmanaged<TOutput> genericFunc) => genericFunc.Call<TInput>();
        }
    }

    public abstract class GenericFuncClass<TOutput> : AbstractGenericFunc<GenericFuncClass<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TInput>() where TInput : class;

        private sealed class GenericFuncCaller<TInput> : GenericFuncCaller
             where TInput : class
        {
            internal override TOutput Call(GenericFuncClass<TOutput> genericFunc) => genericFunc.Call<TInput>();
        }
    }

    public abstract class GenericFuncStruct<TOutput> : AbstractGenericFunc<GenericFuncStruct<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TInput>() where TInput : struct;

        private sealed class GenericFuncCaller<TInput> : GenericFuncCaller
             where TInput : struct
        {
            internal override TOutput Call(GenericFuncStruct<TOutput> genericFunc) => genericFunc.Call<TInput>();
        }
    }

    public abstract class GenericFunc2<TOutput> : AbstractGenericFunc<GenericFunc2<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TObj, TValue>();

        private sealed class GenericFuncCaller<TObj, TValue> : GenericFuncCaller
        {
            internal override TOutput Call(GenericFunc2<TOutput> genericFunc) => genericFunc.Call<TObj, TValue>();
        }
    }

    public abstract class GenericFunc2<TOutput, TValueInterface> : AbstractGenericFunc<GenericFunc2<TOutput, TValueInterface>, TOutput>
    {
        protected abstract TOutput Call<TObj, TValue>() where TValue : TValueInterface;

        private sealed class GenericFuncCaller<TObj, TValue> : GenericFuncCaller
            where TValue : TValueInterface
        {
            internal override TOutput Call(GenericFunc2<TOutput, TValueInterface> genericFunc) => genericFunc.Call<TObj, TValue>();
        }
    }

    public abstract class GenericFunc2Unmanaged<TOutput> : AbstractGenericFunc<GenericFunc2Unmanaged<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TObj, TValue>() where TValue : unmanaged;

        private sealed class GenericFuncCaller<TObj, TValue> : GenericFuncCaller
             where TValue : unmanaged
        {
            internal override TOutput Call(GenericFunc2Unmanaged<TOutput> genericFunc) => genericFunc.Call<TObj, TValue>();
        }
    }

    public abstract class GenericFunc2Equatable<TOutput> : AbstractGenericFunc<GenericFunc2Equatable<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TObj, TValue>() where TValue : IEquatable<TValue>;

        private sealed class GenericFuncCaller<TObj, TValue> : GenericFuncCaller
             where TValue : IEquatable<TValue>
        {
            internal override TOutput Call(GenericFunc2Equatable<TOutput> genericFunc) => genericFunc.Call<TObj, TValue>();
        }
    }

    public abstract class GenericFunc2Struct<TOutput> : AbstractGenericFunc<GenericFunc2Struct<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TObj, TValue>() where TValue : struct;

        private sealed class GenericFuncCaller<TObj, TValue> : GenericFuncCaller
             where TValue : struct
        {
            internal override TOutput Call(GenericFunc2Struct<TOutput> genericFunc) => genericFunc.Call<TObj, TValue>();
        }
    }

    public abstract class GenericFunc2New<TOutput> : AbstractGenericFunc<GenericFunc2New<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TObj, TValue>() where TValue : new();

        private sealed class GenericFuncCaller<TObj, TValue> : GenericFuncCaller
             where TValue : new()
        {
            internal override TOutput Call(GenericFunc2New<TOutput> genericFunc) => genericFunc.Call<TObj, TValue>();
        }
    }

    public static class GenericFuncExtensions
    {
        public static TOutput Call<TSelf, TOutput>(
            this AbstractGenericFunc<TSelf, TOutput> genericFunc,
            PropertyInfo property)
            where TSelf : AbstractGenericFunc<TSelf, TOutput>
        {
            return genericFunc.Call(property.DeclaringType, property.PropertyType);
        }

        internal static IEnumerable<Type> GetTypeHierarchy(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}
