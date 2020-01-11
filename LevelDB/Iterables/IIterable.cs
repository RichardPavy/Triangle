namespace LevelDB.Iterables
{
    using System.Collections.Generic;
    using LevelDB.Iterators;

    /// <summary>
    /// Represents data in a DB.
    /// </summary>
    public interface IIterable : IEnumerable<KeyValuePair<string, string>>
    {
        IIterator GetIterator();

        IIterable Reverse();

        IIterable Range(string from, string to);
    }
}
