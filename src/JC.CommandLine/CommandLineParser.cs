using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine
{
    internal class CommandLineParser : ICommandLineParser
    {
        private readonly ParseModel _model;
        private readonly IFilesystem _filesystem;
        private readonly IObjectBinder _objectBinder;

        internal CommandLineParser(ParseModel model, IObjectBinder objectBinder)
            :this(model, objectBinder, new Filesystem())
        {
        }

        internal CommandLineParser(ParseModel model, IObjectBinder objectBinder, 
            IFilesystem filesystem)
        {
            Guard.IsNotNull(model, nameof(model));
            Guard.IsNotNull(objectBinder, nameof(objectBinder));
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _model = model;
            _objectBinder = objectBinder;
            _filesystem = filesystem;

            //
            //  Note: It *is* actually possible for an argument name to be 
            //  repeated when this method is called.
            var duplicateNames = ModelValidation.GetDuplicateNames(_model.Arguments.SelectMany(o => o.Names));
            if (duplicateNames.Any())
            {
                var duplicateNamesStr = string.Join(", ", duplicateNames);
                var msg = $"Invalid model. The following argument names are " +
                    $"used more than once: {duplicateNamesStr}";
                throw new CommandLineParseException(msg);
            }
        }

        ICommandLineParseResults ICommandLineParser.Parse()
        {
            return (this as ICommandLineParser).Parse(Environment.GetCommandLineArgs());
        }

        ICommandLineParseResults ICommandLineParser.Parse(IEnumerable<string> arguments)
        {
            Guard.IsNotNull(arguments, nameof(arguments));

            var effectiveArgs = _model.ArgsFileDelimitter == null
                                    ? arguments
                                    : new CommandLineArgumentEnumerator(_model.ArgsFileDelimitter.Value, _filesystem).Enumerate(arguments);
            var nodes = CommandLineNode.Parse(effectiveArgs, _model.ArgumentDelimitters);
            var nodeGroups = CommandLineNodeGroup.Parse(nodes).ToImmutableArray();
            nodeGroups = ParsingFixups.SplitExeNode(nodeGroups);
            nodeGroups = ParsingFixups.SplitEndingUnnamedValues(nodeGroups,
                _model.Arguments, _model.StringComparisons);
            nodeGroups = ParsingFixups.ConsolidateDuplicateArguments(
                nodeGroups, _model.Arguments, _model.StringComparisons, _model.NameMatching);
            var resolutions = new ActualModelResolution(nodeGroups, _model, _filesystem);
            (var errors, var warnings) = resolutions.Validate();
            if (errors != null)
            {
                if (errors.ParseErrors.Count() == 1)
                {
                    throw errors.ParseErrors.Single();
                }
                else
                {
                    throw errors;
                }
            }
            var results = new CommandLineParseResults(_objectBinder, resolutions, 
                _filesystem);
            return results;
        }


        public bool AllowUnnamedValues => _model.AllowUnnamedValues;

        ImmutableArray<char> ICommandLineParser.ArgumentDelimitters => _model.ArgumentDelimitters;

        bool ICommandLineParser.CaseSensitive => _model.CaseSensitive;

        internal ImmutableArray<Argument> Arguments => _model.Arguments;
    }
}
