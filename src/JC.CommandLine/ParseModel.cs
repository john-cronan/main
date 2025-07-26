using System;
using System.Collections.Immutable;

namespace JC.CommandLine
{
    internal class ParseModel
    {
        private readonly ImmutableArray<Argument> _arguments;
        private readonly ImmutableArray<char> _argumentDelimitters;
        private readonly bool _caseSensitive;
        private readonly NameMatchingOptions _nameMatching;
        private readonly bool _allowUnnamedValues;
        private readonly char? _argsFileDelimitter;

        public ParseModel(ImmutableArray<Argument> arguments,
            ImmutableArray<char> argumentDelimitters, bool caseSensitive,
            NameMatchingOptions nameMatching, bool allowUnnamedValues,
            char? argsFileDelimitter)
        {
            _arguments = arguments;
            _argumentDelimitters = argumentDelimitters;
            _caseSensitive = caseSensitive;
            _nameMatching = nameMatching;
            _allowUnnamedValues = allowUnnamedValues;
            _argsFileDelimitter = argsFileDelimitter;
        }

        public ImmutableArray<Argument> Arguments => _arguments;

        public ImmutableArray<char> ArgumentDelimitters => _argumentDelimitters;

        public bool CaseSensitive => _caseSensitive;

        public NameMatchingOptions NameMatching => _nameMatching;

        public bool AllowUnnamedValues => _allowUnnamedValues;

        public StringComparison StringComparisons
        {
            get
            {
                return _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            }
        }

        public char? ArgsFileDelimitter => _argsFileDelimitter;
    }
}
