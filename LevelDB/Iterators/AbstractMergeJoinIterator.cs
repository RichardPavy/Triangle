namespace Triangle.LevelDB.Iterators
{
    using System.Collections;
    using System.Collections.Generic;
    using Triangle.Utils;

    public abstract class AbstractMergeJoinIterator<TK1, TK2, TV1, TV2>
        : AbstractDisposable, IMergeJoinIterator<TK1, TK2, TV1, TV2>
    {
        public abstract JoinEntry<TK1, TK2> Key { get; }
        public abstract JoinEntry<TV1, TV2> Value { get; }
        object IEnumerator.Current => Current;
        public KeyValuePair<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>> Current
            => new KeyValuePair<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>>(Key, Value);

        public abstract IMergeJoinIterator<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2> Cast<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2>();
        public abstract IIterator<TKey2, TValue2> Cast<TKey2, TValue2>();

        public abstract bool MoveNext();
        public abstract void Reset();

        public IIterator<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>> Range(
            JoinEntry<TK1, TK2> from,
            JoinEntry<TK1, TK2> to)
            => Range(from.Left, from.Right, to.Left, to.Right);
        public abstract IMergeJoinIterator<TK1, TK2, TV1, TV2> Range(
            TK1 fromLeft,
            TK2 fromRight,
            TK1 toLeft,
            TK2 toRight);

        IIterator<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>> IIterator<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>>.Reverse()
            => Reverse();
        public abstract IMergeJoinIterator<TK1, TK2, TV1, TV2> Reverse();
    }
}
