namespace Visitors.Tests
{
    using Serialization;
    using System;
    using System.Reflection;
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
                                return new ProcessObject<StringBuilder, Rectangle>(
                                    (sb, rectangle) =>
                                    {
                                        sb.Append("Rectangle;");
                                        return VisitStatus.Continue;
                                    });
                            if (typeof(string).IsAssignableFrom(type))
                                return new ProcessObject<StringBuilder, string>(
                                    (sb, @string) =>
                                    {
                                        sb.Append($"string:{@string};");
                                        return VisitStatus.Continue;
                                    });
                            if (typeof(int).IsAssignableFrom(type))
                                return new ProcessObject<StringBuilder, int>(
                                    (sb, @int) =>
                                    {
                                        sb.Append($"int:{@int};");
                                        return VisitStatus.Continue;
                                    });
                            return MustVisitStatus.No;
                        },
                    property =>
                        {
                            if (typeof(string).IsAssignableFrom(property.GetMethod.ReturnType))
                                return new StringFieldProcessor(property).Call(property.DeclaringType);
                            if (typeof(int).IsAssignableFrom(property.GetMethod.ReturnType))
                                return new IntFieldProcessor(property).Call(property.DeclaringType);
                            return MustVisitStatus.No;
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

        private class IntFieldProcessor : GenericFunc<Delegate>
        {
            private readonly PropertyInfo property;

            internal IntFieldProcessor(PropertyInfo property)
            {
                this.property = property;
            }

            protected override Delegate Call<TObj>()
            {
                return new ProcessField<StringBuilder, TObj, int>(
                    (StringBuilder sb, TObj @obj, ref int @int) =>
                    {
                        sb.Append($"{property.DeclaringType.Name}.{property.Name}=int:{@int};");
                        return VisitStatus.Continue;
                    });
            }
        }

        private class StringFieldProcessor : GenericFunc<Delegate>
        {
            private readonly PropertyInfo property;

            internal StringFieldProcessor(PropertyInfo property)
            {
                this.property = property;
            }

            protected override Delegate Call<TObj>()
            {
                return new ProcessField<StringBuilder, TObj, string>(
                    (StringBuilder sb, TObj @obj, ref string @string) =>
                    {
                        sb.Append($"{property.DeclaringType.Name}.{property.Name}=string:{@string};");
                        return VisitStatus.Continue;
                    });
            }
        }
    }
}
