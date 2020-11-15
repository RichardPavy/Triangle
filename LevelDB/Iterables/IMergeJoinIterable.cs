namespace Triangle.LevelDB.Iterables
{
    using Triangle.LevelDB.Iterators;

    public interface IMergeJoinIterable<TK1, TK2, TV1, TV2>
        : IIterable<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>>
    {
        IMergeJoinIterable<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2> Cast<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2>();
        new IMergeJoinIterator<TK1, TK2, TV1, TV2> GetIterator();
        new IMergeJoinIterable<TK1, TK2, TV1, TV2> Reverse();
        IMergeJoinIterable<TK1, TK2, TV1, TV2> Range(TK1 fromLeft, TK2 fromRight, TK1 toLeft, TK2 toRight);
        IMergeJoinIterable<TK1, TK2, TV1, TV2> Prefix(TK1 prefixLeft, TK2 prefixRight);
        new IMergeJoinIterable<TK1, TK2, TV1, TV2> Snapshot();
        new IMergeJoinIterable<TK1, TK2, TV1, TV2> FillCache(bool fillCache);
        new IMergeJoinIterable<TK1, TK2, TV1, TV2> VerifyChecksums(bool verifyChecksums);
    }
}
