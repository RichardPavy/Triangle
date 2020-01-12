namespace LevelDB.Iterables
{
    using LevelDB.Iterators;

    /// <summary>
    /// Implementation of IIterable that loops through a range of keys.
    /// </summary>
    internal sealed class RangeIterable : AbstractIterable
    {
        private readonly AbstractIterable iterable;
        private readonly string from;
        private readonly string to;

        internal RangeIterable(AbstractIterable iterable, string from, string to)
        {
            this.iterable = iterable;
            this.from = from;
            this.to = to;
        }

        public override IIterator GetIterator() => iterable.GetIterator().Range(this.from, this.to);
    }
}