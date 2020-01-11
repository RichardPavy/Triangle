namespace LevelDB.Iterables
{
    using System.Collections;
    using System.Collections.Generic;
    using LevelDB.Iterators;

    /// <summary>
    /// Abstract implementation of IIterable.
    /// </summary>
    internal abstract class AbstractIterable : IIterable
    {
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => GetIterator();
        IEnumerator IEnumerable.GetEnumerator() => GetIterator();
        public IIterable Range(string from, string to) => new RangeIterable(this, from, to);
        public IIterable Reverse() => new ReverseIterable(this);

        public abstract IIterator GetIterator();
    }
}
