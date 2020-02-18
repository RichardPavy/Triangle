namespace Visitors.Tests
{
    using System;
    using System.Text;
    using Visitors;
    using Xunit;

    public class VisitorTests
    {
        [Fact]
        public void GetterSetter()
        {
            VisitorFactory<StringBuilder> visitorFactory =
                new VisitorFactory<StringBuilder>(
                    type =>
                        {
                            if (typeof(Rectangle).IsAssignableFrom(type))
                                return new Visitor.Process<StringBuilder, Rectangle>(
                                    (sb, rectangle) => sb.Append("Rectangle;"));
                            if (typeof(string).IsAssignableFrom(type))
                                return new Visitor.Process<StringBuilder, string>(
                                    (sb, @string) => sb.Append($"string:{@string};"));
                            if (typeof(int).IsAssignableFrom(type))
                                return new Visitor.Process<StringBuilder, int>(
                                    (sb, @int) => sb.Append($"int:{@int};"));
                            return null;
                        },
                    property =>
                        {
                            if (typeof(string).IsAssignableFrom(property.GetMethod.ReturnType))
                                return new Visitor.Process<StringBuilder, string>(
                                    (sb, @string) => sb.Append($"{property.DeclaringType.Name}.{property.Name}=string:{@string};"));
                            if (typeof(int).IsAssignableFrom(property.GetMethod.ReturnType))
                                return new Visitor.Process<StringBuilder, int>(
                                    (sb, @int) => sb.Append($"{property.DeclaringType.Name}.{property.Name}=int:{@int};"));
                            return null;
                        });

            var visitor = visitorFactory.GetClassVisitor<Pair<Pair<Rectangle, Circle>, string>>();
            Pair<Pair<Rectangle, Circle>, string> data =
                new Pair<Pair<Rectangle, Circle>, string>(
                    new Pair<Rectangle, Circle>(
                        new Rectangle { Length = 1, Width = 2 },
                        new Circle { center = new Pair<int, int>(1, 2), radius = 4 }),
                    "abcdef");
            var sb = new StringBuilder();
            visitor.Visit(sb, data);
            Assert.Equal(
                "Rectangle;Rectangle.Length=int:1;int:1;Rectangle.Width=int:2;int:2;Pair`2.Second=string:abcdef;string:abcdef;",
                sb.ToString());
        }
    }
}
