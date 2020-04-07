namespace Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public sealed class VisitorFactory<TData>
    {
        private readonly Dictionary<Type, Lazy<Visitor>> visitors =
            new Dictionary<Type, Lazy<Visitor>>();

        private ClassProcessorFactory classProcessorFactory;
        private FieldProcessorFactory fieldProcessorFactory;

        public VisitorFactory(
            ClassProcessorFactory classProcessorFactory,
            FieldProcessorFactory fieldProcessorFactory)
        {
            this.classProcessorFactory = classProcessorFactory;
            this.fieldProcessorFactory = fieldProcessorFactory;
        }

        public ClassVisitor<TData, TObj> GetClassVisitor<TObj>()
        {
            ClassVisitor<TData, TObj> classVisitor = GetClassVisitorImpl<TObj>();
            classVisitor.Initialize();
            return classVisitor;
        }

        public ClassVisitor<TData, TObj> GetClassVisitorImpl<TObj>()
        {
            return (ClassVisitor<TData, TObj>) CreateClassVisitor(typeof(TObj)).Value;
        }

        private Lazy<Visitor> CreateClassVisitor(Type obj)
        {
            if (!visitors.TryGetValue(obj, out Lazy<Visitor> visitor))
            {
                visitor =
                    new Lazy<Visitor>(() =>
                    {
                        ClassVisitorProcessor processor = classProcessorFactory(obj);
                        return (Visitor) typeof(ClassVisitor<,>)
                            .MakeGenericType(typeof(TData), obj)
                            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                            .Single()
                            .Invoke(new object[] { this, processor.Process, processor.MustVisit });
                    });
                visitors.Add(obj, visitor);

            }
            return visitor;
        }

        internal FieldVisitor<TData, TObj> CreateFieldVisitor<TObj>(PropertyInfo property)
        {
            FieldVisitorProcessor processor = fieldProcessorFactory(property) ?? MustVisitStatus.No;
            return (FieldVisitor<TData, TObj>) typeof(FieldVisitor<,,>)
                .MakeGenericType(
                    typeof(TData),
                    property.GetMethod.DeclaringType,
                    property.GetMethod.ReturnType)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single()
                .Invoke(new object[] { this, property, processor.Process, processor.MustVisit });
        }

        public VisitorFactory<TData> Class<T>(ClassProcessorFactory<T> factory)
        {
            var old = this.classProcessorFactory;
            this.classProcessorFactory = type =>
            {
                return type.IsAssignableFrom(typeof(T)) ? factory() : old(type);
            };
            return this;
        }

        public VisitorFactory<TData> Field<T>(FieldProcessorFactory<T> factory)
        {
            var old = this.fieldProcessorFactory;
            this.fieldProcessorFactory = property =>
            {
                return property.DeclaringType.IsAssignableFrom(typeof(T)) ? factory(property.Name) : old(property);
            };
            return this;
        }

        public delegate ClassVisitorProcessor ClassProcessorFactory(Type type);
        public delegate FieldVisitorProcessor FieldProcessorFactory(PropertyInfo property);

        public delegate ClassVisitorProcessor ClassProcessorFactory<T>();
        public delegate FieldVisitorProcessor FieldProcessorFactory<T>(string property);
    }
}
