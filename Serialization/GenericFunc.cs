namespace Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;

    public abstract class AbstractGenericFunc<TSelf, TOutput>
        where TSelf : AbstractGenericFunc<TSelf, TOutput>
    {
        internal AbstractGenericFunc() {}

        public TOutput Call(params Type[] types)
        {
            GenericFuncCaller genericFuncCaller =
                (GenericFuncCaller)
                GenericFuncCallerType.MakeGenericType(types)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single()
                    .Invoke(new object[0]);
            return genericFuncCaller.Call(Self);
        }

        protected abstract class GenericFuncCaller
        {
            internal abstract TOutput Call(TSelf genericFunc);
        }

        private TSelf Self => (TSelf)this;
        private Type GenericFuncCallerType =>
            GetType()
                .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(type => typeof(GenericFuncCaller).IsAssignableFrom(type))
                .Single();
    }

    public abstract class GenericFunc<TOutput> : AbstractGenericFunc<GenericFunc<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TInput>();

        private sealed class GenericFuncCaller<TInput> : GenericFuncCaller
        {
            internal override TOutput Call(GenericFunc<TOutput> genericFunc) => genericFunc.Call<TInput>();
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

    public abstract class GenericFunc2Unmanaged<TOutput> : AbstractGenericFunc<GenericFunc2Unmanaged<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TObj, TValue>() where TValue : unmanaged;

        private sealed class GenericFuncCaller<TObj, TValue> : GenericFuncCaller
             where TValue : unmanaged
        {
            internal override TOutput Call(GenericFunc2Unmanaged<TOutput> genericFunc) => genericFunc.Call<TObj, TValue>();
        }
    }

    public abstract class GenericFuncStruct2<TOutput> : AbstractGenericFunc<GenericFuncStruct2<TOutput>, TOutput>
    {
        protected abstract TOutput Call<TObj, TValue>() where TValue : struct;

        private sealed class GenericFuncCaller<TObj, TValue> : GenericFuncCaller
             where TValue : struct
        {
            internal override TOutput Call(GenericFuncStruct2<TOutput> genericFunc) => genericFunc.Call<TObj, TValue>();
        }
    }
}
