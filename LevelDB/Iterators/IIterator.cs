namespace LevelDB.Iterators
{
    using System.Collections.Generic;

    /// <summary>
    /// DB Iterator interface.
    /// </summary>
    public interface IIterator : IEnumerator<KeyValuePair<string, string>>
    {
        bool IsValid { get; }

        string Key { get; }

        string Value { get; }

        IIterator SeekToFirst();

        IIterator SeekToLast();

        IIterator Seek(string key);

        bool MovePrevious();

        IIterator Reverse();

        IIterator Range(string from, string to);
    }
}
