namespace Visitors.Utils.Types
{
    using System;
    using System.Collections.Generic;

    public static class TypeExtensions
    {
        public static Type GetGenericParentType(this Type type, Type genericSuperType)
        {
            if (!genericSuperType.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"Not a generic type definition: {genericSuperType}");
            }

            foreach (Type parentType in GetAllParentTypes(type))
            {
                if (!parentType.IsGenericType)
                {
                    continue;
                }

                if (genericSuperType == parentType.GetGenericTypeDefinition())
                {
                    return parentType;
                }
            }
            return null;
        }

        public static IEnumerable<Type> GetAllParentTypes(this Type type, ISet<Type> unique = null)
        {
            if (type == null)
            {
                yield break;
            }

            unique = unique ?? new HashSet<Type>();
            if (!unique.Add(type))
            {
                yield break;
            }

            yield return type;
            foreach (var @super in GetAllParentTypes(type.BaseType, unique))
            {
                yield return @super;
            }

            foreach (var @interface in type.GetInterfaces())
            {
                foreach (var @super in GetAllParentTypes(@interface, unique))
                {
                    yield return @super;
                }
            }
        }
    }
}
