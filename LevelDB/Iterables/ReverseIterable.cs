namespace LevelDB.Iterables
{
    using LevelDB.Iterators;

    /// <summary>
    /// Implementation of IIterable that loops in reverse order.
    /// </summary>
    internal sealed class ReverseIterable : AbstractIterable
    {
        private readonly IIterable iterable;

        internal ReverseIterable(IIterable iterable)
        {
            this.iterable = iterable;
        }

        public override IIterator GetIterator() => iterable.GetIterator().Reverse();
    }
}
