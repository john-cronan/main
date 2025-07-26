using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine
{
    internal static class ModelValidation
    {
        public static IEnumerable<string> GetNamesAlreadyInUse(
            IEnumerable<Argument> arguments, IEnumerable<string> names,
            bool caseSensitive)
        {
            Guard.IsNotNull(arguments, nameof(arguments));

            if (!arguments.Any())
            {
                return new string[0];
            }
            var stringComparison = caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            var allNames = arguments.SelectMany(om => om.Names).ToArray();
            var dupes = from innerName in allNames
                        from outerName in names
                        where innerName.Equals(outerName, stringComparison)
                        select innerName;
            return dupes;
        }

        public static IEnumerable<string> GetDuplicateNames(IEnumerable<string> names)
        {
            Guard.IsNotNull(names, nameof(names));

            var dupes =
                from name in names
                group name by name into g
                where g.Count() > 1
                select g.Key;
            return dupes;
        }
    }
}
