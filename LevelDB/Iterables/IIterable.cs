namespace LevelDB.Iterables
{
    using System.Collections.Generic;
    using LevelDB.Iterators;

    /// <summary>
    /// Represents data in a DB.
    /// </summary>
    public interface IIterable : IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        IIterator GetIterator();

        IIterable Reverse();

        IIterable Range(byte[] from, byte[] to);
    }
}
