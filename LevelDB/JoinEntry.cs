namespace Triangle.LevelDB
{
    public struct JoinEntry<TLeft, TRight>
    {
        public TLeft Left { get; set; }
        public TRight Right { get; set; }

        private JoinEntry(TLeft left, TRight right)
        {
            Left = left;
            Right = right;
        }

        public static JoinEntry<TLeft, TRight> Of(TLeft left, TRight right)
        {
            return new JoinEntry<TLeft, TRight>(left, right);
        }
    }

    public static class JoinEntry
    {
        public static JoinEntry<TLeft, TRight> Of<TLeft, TRight>(TLeft left, TRight right)
        {
            return JoinEntry<TLeft, TRight>.Of(left, right);
        }
    }
}
