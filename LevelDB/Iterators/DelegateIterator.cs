namespace Triangle.LevelDB.Iterators
{
    internal abstract class DelegateIterator : AbstractIterator
    {
        protected readonly AbstractIterator delegateIterator;

        internal DelegateIterator(AbstractIterator delegateIterator)
        {
            this.delegateIterator = delegateIterator;
        }

        public override byte[] Key => this.delegateIterator.Key;
        public override byte[] Value => this.delegateIterator.Value;

        public override IIterator Reverse() => this.delegateIterator.Reverse();
        public override IIterator Range(byte[] from, byte[] to) => this.delegateIterator.Range(from, to);

        internal override bool IsValid => this.delegateIterator.IsValid;

        internal override void Next() => this.delegateIterator.Next();
        internal override void Previous() => this.delegateIterator.Previous();

        internal override IIterator Seek(byte[] key) => this.delegateIterator.Seek(key);
        internal override IIterator SeekToFirst() => this.delegateIterator.SeekToFirst();
        internal override IIterator SeekToLast() => this.delegateIterator.SeekToLast();
        internal override int CompareKeys(byte[] a, byte[] b) => this.delegateIterator.CompareKeys(a, b);

        protected override void DisposeManagedDependencies() => this.delegateIterator.Dispose();
    }
}
