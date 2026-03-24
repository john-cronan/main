using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine
{
    /// <summary>
    /// Implements a builder that constructs instances of <see cref="ICommandLineParser"/>.
    /// </summary>
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
        private Argument _helpSwitch;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public CommandLineParserBuilder()
        {
            _arguments = new List<Argument>();
            _argumentDelimitters = "-/".ToCharArray();
            _bindingType = BindingTypes.PropertyBinding;
            _caseSensitive = false;
            _nameMatchingOption = NameMatchingOptions.Stem;
            _allowUnnamedValues = true;
        }

        /// <summary>
        /// Defines a boolean argument with the specified name.
        /// </summary>
        public CommandLineParserBuilder AddSwitch(string name)
        {
            Guard.IsNotNullOrWhitespace(name, nameof(name));

            return AddSwitch(new string[] { name });
        }

        /// <summary>
        /// Defines a boolean argument identified by any of the specified 
        /// names.
        /// </summary>
        public CommandLineParserBuilder AddSwitch(IEnumerable<string> names)
        {
            Guard.IsNotNullOrEmpty(names, nameof(names));

            AddAndReturnSwitch(names);
            return this;
        }

        /// <summary>
        /// Adds an argument with the specified name and number of possible
        /// or required values.
        /// </summary>
        public CommandLineParserBuilder AddArgument(string name,
            ArgumentMultiplicity multiplicity, bool required)
        {
            Guard.IsNotNullOrWhitespace(name, nameof(name));

            return AddArgument(new string[] { name }, multiplicity, required);
        }

        /// <summary>
        /// Adds an argument idendified by any of the specified names, and 
        /// having a specified number of possible or required values.
        /// </summary>
        public CommandLineParserBuilder AddArgument(IEnumerable<string> names,
            ArgumentMultiplicity multiplicity, bool required)
        {
            return AddArgument(names, multiplicity, required, ArgumentFlags.None);
        }

        /// <summary>
        /// Adds an argument idendified by any of the specified names, and 
        /// having a specified number of possible or required values.
        /// </summary>
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

        /// <summary>
        /// Adds an argument idendified by any of the specified names, and 
        /// having a specified number of possible or required values.
        /// </summary>
        public CommandLineParserBuilder AddArgument(string name, ArgumentMultiplicity multiplicity,
            bool required, ArgumentFlags flags)
        {
            Guard.IsNotNullOrWhitespace(name, nameof(name));

            var names = new string[] { name };
            return AddArgument(names, multiplicity, required, flags);
        }

        /// <summary>
        /// Adds a help switch with the default names of { "?", "Help" }.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The presence of a help switch on a command line will weaken normal command
        /// line validation. Conditions that would normally result in exceptions
        /// being thrown, such as missing required options or undefined options, will
        /// instead manifest as parse warnings, which can be received by a property
        /// or constructor parameter named "parseWarnings" of type
        /// <see cref="CommandLineParseException"/>.
        /// </para>
        /// <para>
        /// As a result of this weakened validation, if your help switch is true, your
        /// command line may not acutally fully pass validation.
        /// </para>
        /// </remarks>
        public CommandLineParserBuilder AddHelpSwitch()
            => AddHelpSwitch(new string[] { "?", "Help" });

        /// <summary>
        /// Adds a help switch with the specified name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The presence of a help switch on a command line will weaken normal command
        /// line validation. Conditions that would normally result in exceptions
        /// being thrown, such as missing required options or undefined options, will
        /// instead manifest as parse warnings, which can be received by a property
        /// or constructor parameter named "parseWarnings" of type
        /// <see cref="CommandLineParseException"/>.
        /// </para>
        /// <para>
        /// As a result of this weakened validation, if your help switch is true, your
        /// command line may not acutally fully pass validation.
        /// </para>
        /// </remarks>
        public CommandLineParserBuilder AddHelpSwitch(string name)
        {
            Guard.IsNotNullOrWhitespace(name, nameof(name));

            return AddHelpSwitch(new string[] { name });
        }

        /// <summary>
        /// Adds a help switch with any of the specified names.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The presence of a help switch on a command line will weaken normal command
        /// line validation. Conditions that would normally result in exceptions
        /// being thrown, such as missing required options or undefined options, will
        /// instead manifest as parse warnings, which can be received by a property
        /// or constructor parameter named "parseWarnings" of type
        /// <see cref="CommandLineParseException"/>.
        /// </para>
        /// <para>
        /// As a result of this weakened validation, if your help switch is true, your
        /// command line may not acutally fully pass validation.
        /// </para>
        /// </remarks>
        public CommandLineParserBuilder AddHelpSwitch(IEnumerable<string> names)
        {
            Guard.IsNotNullOrEmpty(names, nameof(names));

            _helpSwitch = AddAndReturnSwitch(names);
            return this;
        }

        /// <summary>
        /// Creates and returns a configured command line parser.
        /// </summary>
        public ICommandLineParser CreateParser()
        {
            var arguments = _arguments.ToImmutableArray<Argument>();
            var delimitters = _argumentDelimitters.ToImmutableArray();
            var model = new ParseModel(arguments, delimitters, _caseSensitive,
                _nameMatchingOption, _allowUnnamedValues, _argsFileDelimitter,
                _helpSwitch);
            var objectBinder = _bindingType == BindingTypes.ConstructorBinding ?
                    (IObjectBinder)new ConstructorBinder() : (IObjectBinder)new PropertyBinder();
            return new CommandLineParser(model, objectBinder);
        }

        /// <summary>
        /// Specifies that the configured parser will consider any argument
        /// starting with the specified character to be a file containing
        /// additional arguments.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Argument files specify addtional arguments, one per line. An
        /// argument and any associated values must be on separate lines.
        /// </para>
        /// <para>
        /// Multiple argument files can be specified on a single command line.
        /// </para>
        /// </remarks>
        public CommandLineParserBuilder AllowArgsFiles(char delimitter)
        {
            _argsFileDelimitter = delimitter;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser will allow values at the
        /// beginning and at the end of the command line that are not associated
        /// with any argument.
        /// </summary>
        public CommandLineParserBuilder AllowUnnamedValues()
        {
            _allowUnnamedValues = true;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser will not allow and 
        /// parse argument files.
        /// </summary>
        public CommandLineParserBuilder DisallowArgsFiles()
        {
            _argsFileDelimitter = null;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parse will not allow values at the
        /// beginning and at the end of the command line that are not 
        /// associated with any argument.
        /// </summary>
        public CommandLineParserBuilder DisallowUnnamedValues()
        {
            _allowUnnamedValues = false;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser will ignore case when
        /// considering argument and switch names. This case-insentivity
        /// does not apply to object binding. Binding to properties and
        /// constructor parameters is always case-insensitive.
        /// </summary>
        public CommandLineParserBuilder IsCaseInsensitive()
        {
            _caseSensitive = false;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser will match case when
        /// considering argument and switch names. This case-sentivity
        /// does not apply to object binding. Binding to properties and
        /// constructor parameters is always case-insensitive.
        /// </summary>
        public CommandLineParserBuilder IsCaseSensitive()
        {
            _caseSensitive = true;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser will match argument and
        /// switch names exactly as they are defined by the 
        /// <see cref="M:AddArgument"/> and <see cref="M:AddSwitch"/> methods, 
        /// within the bounds of the confiured case-sensitivity.
        /// </summary>
        public CommandLineParserBuilder UseExactNameMatching()
        {
            _nameMatchingOption = NameMatchingOptions.Exact;
            return this;
        }

        /// <summary>
        /// Configures the configured parser will use stem name matching,
        /// which allows command lines to specify the beginning of an
        /// argument's name. If the portion specified is ambiguous, 
        /// an exception is thrown. This behavior is the builder's
        /// default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example: If an argument is defined with the name "Verbose", a
        /// command line can specify, for example, "-verbose", "-v", or
        /// "-ver", providing there is no defined "Version" argument.
        /// </para>
        /// </remarks>
        public CommandLineParserBuilder UseStemNameMatching()
        {
            _nameMatchingOption = NameMatchingOptions.Stem;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser will consider a node on 
        /// command line to be an argument if it starts with the specified
        /// character.
        /// </summary>
        public CommandLineParserBuilder UseArgumentDelimitter(char delimitter)
        {
            _argumentDelimitters = new char[] { delimitter };
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser will consider a node on 
        /// command line to be an argument if it starts with any of the 
        /// specified characters.
        /// </summary>
        public CommandLineParserBuilder UseArgumentDelimitters(
            params char[] delimitters)
        {
            Guard.IsNotNullOrEmpty(delimitters, nameof(delimitters));

            _argumentDelimitters = delimitters;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser's 
        /// <see cref="ICommandLineParser.Parse()"/> method will return an
        /// instance of <see cref="ICommandLineParseResults"/> configured
        /// to bind to objects by passing parsed values into their 
        /// constructors.
        /// </summary>
        public CommandLineParserBuilder UseConstructorBinding()
        {
            _bindingType = BindingTypes.ConstructorBinding;
            return this;
        }

        /// <summary>
        /// Specifies that the configured parser's 
        /// <see cref="ICommandLineParser.Parse()"/> method will return an
        /// instance of <see cref="ICommandLineParseResults"/> configured
        /// to bind to objects by setting properties with parsed values.
        /// This is the builder's default.
        /// </summary>
        public CommandLineParserBuilder UsePropertyBinding()
        {
            _bindingType = BindingTypes.PropertyBinding;
            return this;
        }


        /// <summary>
        /// Gets the currently configured set of argument delimitters.
        /// </summary>
        public IEnumerable<char> ArgumentDelimitters => _argumentDelimitters;

        /// <summary>
        /// Gets a flag indicating whether the configured parser will
        /// match case with respect to argument and switch names.
        /// </summary>
        public bool CaseSensitive => _caseSensitive;

        /// <summary>
        /// Gets a flag indicating whether the configured parser will allow
        /// values at the beginning and at the end of the command line that are
        /// not associated with any argument.
        /// </summary>
        public bool UnnamedValuesAllowed => _allowUnnamedValues;


        private Argument AddAndReturnSwitch(IEnumerable<string> names)
        {
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
            return argument;
        }
    }
}
