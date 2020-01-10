namespace LevelDB
{
    /// <summary>
    /// DB Iterator -- same as <see cref="Iterator"/>, but reverse.
    /// </summary>
    public sealed class ReverseIterator : AbstractReverseIterator<ReverseIterator, Iterator>
    {
        internal ReverseIterator(Iterator forwardIterator) : base(forwardIterator)
        {
        }

        protected override ReverseIterator Self => this;
    }
}
