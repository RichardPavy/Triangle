namespace LevelDB
{
    /// <summary>
    /// DB Iterator -- same as <see cref="RangeIterator{TBaseIterator}"/>, but reverse.
    /// </summary>
    public sealed class ReverseRangeIterator<TBaseIterator> :
        AbstractReverseIterator<
            ReverseRangeIterator<TBaseIterator>,
            RangeIterator<TBaseIterator>>
        where TBaseIterator : IIterator<TBaseIterator>
    {
        internal ReverseRangeIterator(RangeIterator<TBaseIterator> forwardIterator) : base(forwardIterator)
        {
        }

        protected override ReverseRangeIterator<TBaseIterator> Self => this;
    }
}
