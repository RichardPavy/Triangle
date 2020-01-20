namespace LevelDB.Iterators
{
    internal abstract class DelegateIterator : AbstractIterator
    {
        protected readonly AbstractIterator delegateIterator;

        internal DelegateIterator(AbstractIterator delegateIterator)
        {
            this.delegateIterator = delegateIterator;
        }

        public override byte[] Key => delegateIterator.Key;
        public override byte[] Value => delegateIterator.Value;

        public override IIterator Reverse() => delegateIterator.Reverse();
        public override IIterator Range(byte[] from, byte[] to) => delegateIterator.Range(from, to);
        public override void Dispose() => delegateIterator.Dispose();

        internal override bool IsValid => delegateIterator.IsValid;

        internal override void Next() => delegateIterator.Next();
        internal override void Previous() => delegateIterator.Previous();

        internal override IIterator Seek(byte[] key) => delegateIterator.Seek(key);
        internal override IIterator SeekToFirst() => delegateIterator.SeekToFirst();
        internal override IIterator SeekToLast() => delegateIterator.SeekToLast();
        internal override int CompareKeys(byte[] a, byte[] b) => delegateIterator.CompareKeys(a, b);
    }
}
