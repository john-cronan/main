using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine
{
    internal static class NameMatching
    {
        public static bool IsMatch(string actualName,
            IEnumerable<string> argumentNames, NameMatchingOptions nameMatching,
            StringComparison stringComparisons)
        {
            Guard.IsNotNullOrWhitespace(actualName, nameof(actualName));
            Guard.IsNotNull(argumentNames, nameof(argumentNames));

            var adjustedActualName = actualName.Replace("-", string.Empty);
            foreach (var modelName in argumentNames)
            {
                var adjustedModelName = modelName.Replace("-", string.Empty);
                switch (nameMatching)
                {
                    case NameMatchingOptions.Exact:
                        if (adjustedActualName.Equals(adjustedModelName, stringComparisons))
                        {
                            return true;
                        }
                        break;
                    case NameMatchingOptions.Stem:
                        if (adjustedModelName.StartsWith(adjustedActualName, stringComparisons))
                        {
                            return true;
                        }
                        break;
                    default:
                        throw new ArgumentException($"Name matching argument {nameMatching} not supported");
                }
            }
            return false;
        }
    }
}
