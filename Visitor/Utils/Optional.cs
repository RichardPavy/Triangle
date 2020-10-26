namespace Triangle.Visitors.Utils
{
    using System;

    /// <summary>
    ///   Similar to <see cref="Nullable{T}"/> but supports reference types as well.
    /// </summary>
    public static class Optional
    {
        public static Optional<T> FromClass<T>(T value)
            where T : class
        {
            return new Optional<T>(value, value != null);
        }

        public static Optional<T> FromStruct<T>(T value)
            where T : struct
        {
            return new Optional<T>(value, true);
        }

        public static Optional<T> FromNullable<T>(T? value)
            where T : struct
        {
            return new Optional<T>(value ?? default, value != null);
        }

        public static Optional<T> From<T>(T value)
        {
            return Optional<T>.FromImpl(value);
        }
    }

    public struct Optional<T>
    {
        internal static readonly Func<T, Optional<T>> FromImpl =
            typeof(T).IsValueType
                ? (Func<T, Optional<T>>) new FromStruct().Call(typeof(T))
                : (Func<T, Optional<T>>) new FromClass().Call(typeof(T));

        private class FromClass : GenericFuncClass<Delegate>
        {
            protected override Delegate Call<TInput>()
            {
                return new Func<TInput, Optional<TInput>>(Optional.FromClass);
            }
        }

        private class FromStruct : GenericFuncStruct<Delegate>
        {
            protected override Delegate Call<TInput>()
            {
                return new Func<TInput, Optional<TInput>>(Optional.FromStruct);
            }
        }

        public static readonly Optional<T> Null = new Optional<T>(default, false);

        private readonly T value;

        public T Value => HasValue ? value : throw new InvalidOperationException($"{GetType()} does not have a value.");
        public bool HasValue { get; }

        internal Optional(T value, bool hasValue)
        {
            this.value = value;
            this.HasValue = hasValue;
        }

        public override int GetHashCode() => HasValue ? value.GetHashCode() : 0;

        public override string ToString() => HasValue ? value.ToString() : "";

        public static T operator |(Optional<T> @optional, T @default) =>
            @optional.HasValue ? @optional.value : @default;
    }
}
