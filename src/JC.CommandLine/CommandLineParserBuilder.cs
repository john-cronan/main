using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine
{
    public sealed class CommandLineParserBuilder
    {
        private enum BindingTypes
        {
            PropertyBinding,
            ConstructorBinding
        }

        private readonly List<Argument> _arguments;
        private IEnumerable<char> _argumentDelimitters;
        private BindingTypes _bindingType;
        private bool _caseSensitive;
        private NameMatchingOptions _nameMatchingOption;
        private bool _allowUnnamedValues;
        private char? _argsFileDelimitter;

        public CommandLineParserBuilder()
        {
            _arguments = new List<Argument>();
            _argumentDelimitters = "-/".ToCharArray();
            _bindingType = BindingTypes.PropertyBinding;
            _caseSensitive = false;
            _nameMatchingOption = NameMatchingOptions.Stem;
            _allowUnnamedValues = true;
        }

        public CommandLineParserBuilder AddSwitch(string name)
        {
            Guard.IsNotNullOrWhitespace(name, nameof(name));

            return AddSwitch(new string[] { name });
        }

        public CommandLineParserBuilder AddSwitch(IEnumerable<string> names)
        {
            Guard.IsNotNullOrEmpty(names, nameof(names));

            var duplicateEntries = ModelValidation.GetDuplicateNames(names);
            if (duplicateEntries.Any())
            {
                var namesAlreadyInUseStr = string.Join(", ", duplicateEntries);
                var msg = $"The following names are duplicated: {namesAlreadyInUseStr}";
                throw new ArgumentException(msg, nameof(names));
            }
            var namesAlreadyInUse = ModelValidation.GetNamesAlreadyInUse(
                _arguments, names, _caseSensitive);
            if (namesAlreadyInUse.Any())
            {
                var namesAlreadyInUseStr = string.Join(", ", namesAlreadyInUse);
                var msg = $"The following names are already in use: {namesAlreadyInUseStr}";
                throw new ArgumentException(msg, nameof(names));
            }

            var asImmutable = names.ToImmutableArray<string>();
            var argument = new Argument(asImmutable, ArgumentMultiplicity.Zero, false);
            _arguments.Add(argument);
            return this;
        }

        public CommandLineParserBuilder AddArgument(string name, ArgumentMultiplicity multiplicity, 
            bool required)
        {
            Guard.IsNotNullOrWhitespace(name, nameof(name));

            return AddArgument(new string[] { name }, multiplicity, required);
        }

        public CommandLineParserBuilder AddArgument(IEnumerable<string> names, 
            ArgumentMultiplicity multiplicity, bool required)
        {
            return AddArgument(names, multiplicity, required, ArgumentFlags.None);
        }

        public CommandLineParserBuilder AddArgument(IEnumerable<string> names,
            ArgumentMultiplicity multiplicity, bool required, ArgumentFlags flags)
        {
            Guard.IsNotNullOrEmpty(names, nameof(names));

            var duplicateEntries = ModelValidation.GetDuplicateNames(names);
            if (duplicateEntries.Any())
            {
                var namesAlreadyInUseStr = string.Join(", ", duplicateEntries);
                var msg = $"The following names are duplicated: ${namesAlreadyInUseStr}";
                throw new ArgumentException(msg, nameof(names));
            }
            var namesAlreadyInUse = ModelValidation.GetNamesAlreadyInUse(
                _arguments, names, _caseSensitive);
            if (namesAlreadyInUse.Any())
            {
                var namesAlreadyInUseStr = string.Join(", ", namesAlreadyInUse);
                var msg = $"The following names are already in use: ${namesAlreadyInUseStr}";
                throw new ArgumentException(msg, nameof(names));
            }

            var asImmutable = names.ToImmutableArray();
            var argument = new Argument(asImmutable, multiplicity, required, flags);
            _arguments.Add(argument);
            return this;
        }

        public CommandLineParserBuilder AddArgument(string name, ArgumentMultiplicity multiplicity,
            bool required, ArgumentFlags flags)
        {
            Guard.IsNotNullOrWhitespace(name, nameof(name));

            var names = new string[] { name };
            return AddArgument(names, multiplicity, required, flags);
        }

        public ICommandLineParser CreateParser()
        {
            var arguments = _arguments.ToImmutableArray<Argument>();
            var delimitters = _argumentDelimitters.ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, _caseSensitive,
                _nameMatchingOption, _allowUnnamedValues, _argsFileDelimitter);
            var objectBinder = _bindingType == BindingTypes.ConstructorBinding ?
                    (IObjectBinder)new ConstructorBinder() : (IObjectBinder)new PropertyBinder();
            return new CommandLineParser(model, objectBinder);
        }

        public CommandLineParserBuilder AllowArgsFiles(char delimitter)
        {
            _argsFileDelimitter = delimitter;
            return this;
        }

        public CommandLineParserBuilder AllowUnnamedValues()
        {
            _allowUnnamedValues = true;
            return this;
        }

        public CommandLineParserBuilder DisallowArgsFiles()
        {
            _argsFileDelimitter = null;
            return this;
        }

        public CommandLineParserBuilder DisallowUnnamedValues()
        {
            _allowUnnamedValues = false;
            return this;
        }

        public CommandLineParserBuilder IsCaseInsensitive()
        {
            _caseSensitive = false;
            return this;
        }

        public CommandLineParserBuilder IsCaseSensitive()
        {
            _caseSensitive = true;
            return this;
        }

        public CommandLineParserBuilder UseExactNameMatching()
        {
            _nameMatchingOption = NameMatchingOptions.Exact;
            return this;
        }

        public CommandLineParserBuilder UseStemNameMatching()
        {
            _nameMatchingOption = NameMatchingOptions.Stem;
            return this;
        }

        public CommandLineParserBuilder UseArgumentDelimitter(char delimitter)
        {
            _argumentDelimitters = new char[] { delimitter };
            return this;
        }

        public CommandLineParserBuilder UseArgumentDelimitters(
            params char[] delimitters)
        {
            Guard.IsNotNullOrEmpty(delimitters, nameof(delimitters));

            _argumentDelimitters = delimitters;
            return this;
        }

        public CommandLineParserBuilder UseConstructorBinding()
        {
            _bindingType = BindingTypes.ConstructorBinding;
            return this;
        }

        public CommandLineParserBuilder UsePropertyBinding()
        {
            _bindingType = BindingTypes.PropertyBinding;
            return this;
        }


        public IEnumerable<char> ArgumentDelimitters => _argumentDelimitters;
        public bool CaseSensitive => _caseSensitive;
        //public NameMatchingOptions NameMatchingOption => _nameMatchingOption;
        public bool UnnamedValuesAllowed => _allowUnnamedValues;
    }
}
