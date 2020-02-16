namespace LevelDB.Iterables
{
    using LevelDB.Iterators;

    /// <summary>
    /// Implementation of IIterable that loops through a range of keys.
    /// </summary>
    internal sealed class RangeIterable : DelegateIterable
    {
        private readonly AbstractIterable iterable;
        private readonly byte[] from;
        private readonly byte[] to;

        internal RangeIterable(AbstractIterable delegateIterable, byte[] from, byte[] to) : base(delegateIterable)
        {
            this.from = from;
            this.to = to;
        }

        public override IIterator GetIterator() => delegateIterable.GetIterator().Range(this.from, this.to);
    }
}
