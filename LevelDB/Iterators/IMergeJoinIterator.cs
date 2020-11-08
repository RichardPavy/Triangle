namespace Triangle.LevelDB.Iterators
{
    public interface IMergeJoinIterator<TLeftKey, TRightKey, TLeftValue, TRightValue>
        : IIterator<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>>
    {
        IIterator<JoinEntry<TLeftKey2, TRightKey2>, JoinEntry<TLeftValue2, TRightValue2>>
            Cast<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2>();
    }
}
