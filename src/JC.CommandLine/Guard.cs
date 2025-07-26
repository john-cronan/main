using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine
{
    internal static class Guard
    {
        public static void IsNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void IsNotNullOrEmpty<T>(IEnumerable<T> value, string name)
        {
            if (value == null || !value.Any())
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void IsNotNullOrWhitespace(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException($"The parameter {name} cannot be null or whitespace", name);
            }
        }

        public static void IsNotEmpty(Array a, string name)
        {
            if (a.Length == 0)
            {
                throw new ArgumentNullException($"The argument {nameof(name)} cannot be an empty array", nameof(name));
            }
        }

        public static void IsNotEmpty<T>(ImmutableArray<T> a, string name)
        {
            if (a == ImmutableArray<T>.Empty)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
