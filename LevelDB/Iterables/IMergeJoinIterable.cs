namespace Triangle.LevelDB.Iterables
{
    using Triangle.LevelDB.Iterators;

    public interface IMergeJoinIterable<TLeftKey, TRightKey, TLeftValue, TRightValue>
        : IIterable<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>>
    {
        IMergeJoinIterable<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2> Cast<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2>();
        new IMergeJoinIterator<TLeftKey, TRightKey, TLeftValue, TRightValue> GetIterator();
        new IMergeJoinIterable<TLeftKey, TRightKey, TLeftValue, TRightValue> Reverse();
        IMergeJoinIterable<TLeftKey, TRightKey, TLeftValue, TRightValue> Range(TLeftKey fromLeft, TRightKey fromRight, TLeftKey toLeft, TRightKey toRight);
        IMergeJoinIterable<TLeftKey, TRightKey, TLeftValue, TRightValue> Prefix(TLeftKey prefixLeft, TRightKey prefixRight);
        new IMergeJoinIterable<TLeftKey, TRightKey, TLeftValue, TRightValue> Snapshot();
        new IMergeJoinIterable<TLeftKey, TRightKey, TLeftValue, TRightValue> FillCache(bool fillCache);
        new IMergeJoinIterable<TLeftKey, TRightKey, TLeftValue, TRightValue> VerifyChecksums(bool verifyChecksums);
    }
}
