namespace Triangle.LevelDB.Iterables
{
    using System.Collections;
    using System.Collections.Generic;
    using Triangle.LevelDB.Iterators;

    public abstract class AbstractMergeJoinIterable<TK1, TK2, K1, TV2>
        : IMergeJoinIterable<TK1, TK2, K1, TV2>
    {
        // GetEnumerator / GetIterator
        public IEnumerator<KeyValuePair<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>>> GetEnumerator()
            => GetIterator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetIterator();
        IIterator<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>> IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>>.GetIterator()
            => GetIterator();
        public abstract IMergeJoinIterator<TK1, TK2, K1, TV2> GetIterator();

        // Reverse
        IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>> IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>>.Reverse()
            => Reverse();
        public abstract IMergeJoinIterable<TK1, TK2, K1, TV2> Reverse();

        // Range
        public IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>> Range(JoinEntry<TK1, TK2> from, JoinEntry<TK1, TK2> to)
            => Range(from.Left, from.Right, to.Left, to.Right);
        public abstract IMergeJoinIterable<TK1, TK2, K1, TV2> Range(TK1 fromLeft, TK2 fromRight, TK1 toLeft, TK2 toRight);

        // Prefix
        public IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>> Prefix(JoinEntry<TK1, TK2> prefix)
            => Prefix(prefix.Left, prefix.Right);
        public abstract IMergeJoinIterable<TK1, TK2, K1, TV2> Prefix(TK1 prefixLeft, TK2 prefixRight);

        // Cast
        public abstract IMergeJoinIterable<TTK1, TTK2, TTV1, TTV2> Cast<TTK1, TTK2, TTV1, TTV2>();
        public abstract IIterable<TKey2, TValue2> Cast<TKey2, TValue2>();

        // Snapshot
        IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>> IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>>.Snapshot()
            => Snapshot();
        public abstract IMergeJoinIterable<TK1, TK2, K1, TV2> Snapshot();

        // FillCache
        IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>> IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>>.FillCache(bool fillCache)
            => FillCache(fillCache);
        public abstract IMergeJoinIterable<TK1, TK2, K1, TV2> FillCache(bool fillCache);

        // VerifyChecksums
        IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>> IIterable<JoinEntry<TK1, TK2>, JoinEntry<K1, TV2>>.VerifyChecksums(bool verifyChecksums)
            => VerifyChecksums(verifyChecksums);
        public abstract IMergeJoinIterable<TK1, TK2, K1, TV2> VerifyChecksums(bool verifyChecksums);
    }
}
