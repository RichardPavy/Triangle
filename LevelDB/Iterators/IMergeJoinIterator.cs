namespace Triangle.LevelDB.Iterators
{
    public interface IMergeJoinIterator<TK1, TK2, TV1, TV2>
        : IIterator<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>>
    {
        new IMergeJoinIterator<TK1, TK2, TV1, TV2> Reverse();
        IMergeJoinIterator<TK1, TK2, TV1, TV2> Range(TK1 fromLeft, TK2 fromRight, TK1 toLeft, TK2 toRight);
        IMergeJoinIterator<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2> Cast<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2>();
    }
}
