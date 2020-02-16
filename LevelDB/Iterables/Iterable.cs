namespace LevelDB.Iterables
{
    using System.Collections;
    using System.Collections.Generic;
    using LevelDB.Iterators;

    /// <summary>
    /// Implementation of IIterable that loops through all the keys.
    /// </summary>
    internal sealed class Iterable : AbstractIterable
    {
        internal override DB DB { get; }

        internal override ReadOptions ReadOptions { get; }

        internal Iterable(DB db, ReadOptions readOptions)
        {
            DB = db;
            ReadOptions = readOptions;
        }

        public override IIterator GetIterator() => new Iterator(DB, ReadOptions);
    }

    internal sealed class Iterable<TKey, TValue> : IIterable<TKey, TValue>
    {
        private readonly IIterable delegateIterable;

        internal Iterable(IIterable delegateIterable)
        {
            this.delegateIterable = delegateIterable;
        }

        public IIterator<TKey, TValue> GetIterator() => delegateIterable.GetIterator().Cast<TKey, TValue>();
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => GetIterator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IIterable<TKey, TValue> Range(TKey from, TKey to) =>
            delegateIterable
                .Range(
                    Marshallers<TKey>.Instance.ToBytes(from),
                    Marshallers<TKey>.Instance.ToBytes(to))
                .Cast<TKey, TValue>();

        public IIterable<TKey, TValue> Reverse() =>
            delegateIterable.Reverse().Cast<TKey, TValue>();

        public IIterable<TKey2, TValue2> Cast<TKey2, TValue2>() =>
            new Iterable<TKey2, TValue2>(delegateIterable);

        public IIterable<TKey, TValue> Snapshot()
        {
            delegateIterable.Snapshot();
            return this;
        }

        public IIterable<TKey, TValue> FillCache(bool fillCache)
        {
            delegateIterable.FillCache(fillCache);
            return this;
        }

        public IIterable<TKey, TValue> VerifyChecksums(bool verifyChecksums)
        {
            delegateIterable.VerifyChecksums(verifyChecksums);
            return this;
        }
    }
}
