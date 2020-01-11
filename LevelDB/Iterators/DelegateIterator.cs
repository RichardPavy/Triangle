namespace LevelDB.Iterators
{
    internal abstract class DelegateIterator : AbstractIterator
    {
        protected readonly AbstractIterator delegateIterator;

        internal DelegateIterator(AbstractIterator delegateIterator)
        {
            this.delegateIterator = delegateIterator;
        }

        public override string Key => delegateIterator.Key;
        public override string Value => delegateIterator.Value;

        public override IIterator Reverse() => delegateIterator.Reverse();
        public override IIterator Range(string from, string to) => delegateIterator.Range(from, to);
        public override void Dispose() => delegateIterator.Dispose();

        internal override bool IsValid => delegateIterator.IsValid;

        internal override void Next() => delegateIterator.Next();
        internal override void Previous() => delegateIterator.Previous();

        internal override IIterator Seek(string key) => delegateIterator.Seek(key);
        internal override IIterator SeekToFirst() => delegateIterator.SeekToFirst();
        internal override IIterator SeekToLast() => delegateIterator.SeekToLast();
        internal override int CompareKeys(string a, string b) => delegateIterator.CompareKeys(a, b);
    }
}
