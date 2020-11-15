namespace Triangle.LevelDB.Iterables
{
    internal abstract class DelegateIterable : AbstractIterable
    {
        protected readonly AbstractIterable delegateIterable;

        protected DelegateIterable(AbstractIterable delegateIterable)
        {
            this.delegateIterable = delegateIterable;
        }

        internal sealed override DB DB => this.delegateIterable.DB;
        internal sealed override ReadOptions ReadOptions => this.delegateIterable.ReadOptions;
    }
}
