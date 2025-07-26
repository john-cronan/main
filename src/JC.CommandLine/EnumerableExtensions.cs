using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine
{
    internal static class EnumerableExtensions
    {
        public static Array ToArray(this IEnumerable<object> self, Type ofType)
        {
            Guard.IsNotNull(self, nameof(self));
            Guard.IsNotNull(ofType, nameof(ofType));

            var selfAsArray = self.ToArray();
            var outputArray = Array.CreateInstance(ofType, selfAsArray.Length);
            for (int i = 0; i < selfAsArray.Length; i++)
            {
                outputArray.SetValue(selfAsArray[i], i);
            }
            return outputArray;
        }

        public static object ToList(this IEnumerable<object> self, Type ofType)
        {
            Guard.IsNotNull(self, nameof(self));
            Guard.IsNotNull(ofType, nameof(ofType));

            var selfAsArray = self.ToArray();
            var outputListType = typeof(List<>).MakeGenericType(ofType);
            var outputList = (IList)Activator.CreateInstance(outputListType);
            for (int i = 0; i < selfAsArray.Length; i++)
            {
                outputList.Add(selfAsArray[i]);
            }
            return outputList;
        }

        public static object ToImmutableArray(this IEnumerable<object> self, Type ofType)
        {
            Guard.IsNotNull(self, nameof(self));
            Guard.IsNotNull(ofType, nameof(ofType));

            var selfAsArray = ToArray(self, ofType);
            var immutableArrayType = typeof(ImmutableArray);
            var toImmutableArrayMethods =
                from method in immutableArrayType.GetMethods()
                where method.IsPublic && method.IsStatic
                && method.Name.Equals("ToImmutableArray", StringComparison.InvariantCulture)
                && method.GetParameters().Length == 1
                select method;
            var toImmutableArrayMethod = toImmutableArrayMethods.FirstOrDefault();
            toImmutableArrayMethod = toImmutableArrayMethod.MakeGenericMethod(ofType);
            if (toImmutableArrayMethod == null)
            {
                throw new InvalidOperationException("ToImmutableArray method not found");
            }
            return toImmutableArrayMethod.Invoke(null, new object[] { selfAsArray });
        }
    }
}
