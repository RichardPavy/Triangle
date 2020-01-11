namespace LevelDB.Iterables
{
    using LevelDB.Iterators;

    /// <summary>
    /// Implementation of IIterable that loops through all the keys.
    /// </summary>
    internal sealed class Iterable : AbstractIterable
    {
        private readonly DB db;
        private readonly ReadOptions readOptions;

        internal Iterable(DB db, ReadOptions readOptions)
        {
            this.db = db;
            this.readOptions = readOptions;
        }

        public override IIterator GetIterator() => new Iterator(db, readOptions);
    }
}
