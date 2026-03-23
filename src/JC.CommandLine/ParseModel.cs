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
        private readonly Argument _helpArgument;
        
        public ParseModel(ImmutableArray<Argument> arguments,
            ImmutableArray<char> argumentDelimitters, bool caseSensitive,
            NameMatchingOptions nameMatching, bool allowUnnamedValues,
            char? argsFileDelimitter, Argument helpArgument)
        {
            if (helpArgument != null && !arguments.Contains(helpArgument))
            {
                throw new ArgumentException($"The {nameof(helpArgument)} must be one of the defined " +
                    $"{nameof(arguments)}", nameof(helpArgument));
            }

            _arguments = arguments;
            _argumentDelimitters = argumentDelimitters;
            _caseSensitive = caseSensitive;
            _nameMatching = nameMatching;
            _allowUnnamedValues = allowUnnamedValues;
            _argsFileDelimitter = argsFileDelimitter;
            _helpArgument = helpArgument;
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

        internal Argument HelpArgument => _helpArgument;
    }
}
