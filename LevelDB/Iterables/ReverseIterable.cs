namespace LevelDB.Iterables
{
    using LevelDB.Iterators;

    /// <summary>
    /// Implementation of IIterable that loops in reverse order.
    /// </summary>
    internal sealed class ReverseIterable : DelegateIterable
    {
        internal ReverseIterable(AbstractIterable delegateIterable) : base(delegateIterable)
        {
        }

        public override IIterable<byte[], byte[]> Prefix(byte[] prefix) =>
            delegateIterable.Prefix(prefix).Reverse();

        public override IIterator GetIterator() =>
            delegateIterable.GetIterator().Reverse();
    }
}
