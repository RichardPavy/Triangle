namespace Visitors.Tests
{
    public interface IPair { }

    public sealed class Pair<A, B> : IPair
    {
        public A First { get; }
        public B Second { get; }

        public Pair(A first, B second)
        {
            this.First = first;
            this.Second = second;
        }
    }

    public struct Rectangle
    {
        public int Length { get; set; }
        public int Width { get; set; }
    }

    public struct Triangle
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
    }

    public struct Circle
    {
        public Pair<int, int> center;
        public int radius;
    }
}
