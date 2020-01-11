namespace LevelDB.Iterators
{
    using System.Collections.Generic;

    /// <summary>
    /// DB Iterator interface.
    /// </summary>
    public interface IIterator : IEnumerator<KeyValuePair<string, string>>
    {
        string Key { get; }

        string Value { get; }

        IIterator Reverse();

        IIterator Range(string from, string to);
    }
}
