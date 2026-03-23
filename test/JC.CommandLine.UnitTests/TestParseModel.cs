using System.Collections.Immutable;

namespace JC.CommandLine.UnitTests
{
    internal static class TestParseModel
    {
        public static ParseModel Create(ImmutableArray<Argument> arguments = default,
            ImmutableArray<char> argumentDelimitters = default,
            bool caseSensitive = false, NameMatchingOptions nameMatching = NameMatchingOptions.Stem,
            bool allowUnnamedValues = true, Argument helpArgument = null)
        {
            var effectiveArguments = arguments.IsDefault ? ImmutableArray<Argument>.Empty : arguments;
            var effectiveArgumentDelimitters = argumentDelimitters.IsDefault
                ? new char[] { '-', '/' }.ToImmutableArray() : argumentDelimitters;
            return new ParseModel(effectiveArguments, effectiveArgumentDelimitters,
                caseSensitive, nameMatching, allowUnnamedValues, '@', helpArgument);
        }
    }
}
