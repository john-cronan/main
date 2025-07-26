using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine
{
    internal class ActualModelResolution
    {
        private readonly ImmutableArray<CommandLineNodeGroup> _actuals;
        private readonly ImmutableArray<CommandLineNodeGroup> _ambiguousActuals;
        private readonly IFilesystem _filesystem;
        private readonly ImmutableArray<ActualModelMatch> _matches;
        private readonly ParseModel _model;
        private readonly ImmutableArray<Argument> _missingModelArguments;
        private readonly ImmutableArray<CommandLineNodeGroup> _undefinedActuals;

        public ActualModelResolution(
            ImmutableArray<CommandLineNodeGroup> actuals, ParseModel model)

            : this(actuals, model, new Filesystem())
        {
        }

        public ActualModelResolution(
            ImmutableArray<CommandLineNodeGroup> actuals, ParseModel model,
            IFilesystem filesystem)
        {
            Guard.IsNotNull(model, nameof(model));
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _actuals = actuals;
            _model = model;
            _filesystem = filesystem;

            var im =
                (from actual in actuals
                 select new
                 {
                     Actual = actual,
                     MatchingModelArguments =
                     (
                         from modelItem in _model.Arguments
                         where NameMatching.IsMatch(actual.KeyNode.Text, modelItem.Names,
                             _model.NameMatching, _model.StringComparisons)
                         select modelItem
                     ).ToArray()
                 }).ToArray();
            _ambiguousActuals =
                im.Where(i => i.MatchingModelArguments.Count() > 1)
                    .Select(i => i.Actual)
                    .ToImmutableArray();

            //
            //  Note: The exe node is always defined, since it should always be
            //  present, regardless of what the caller specifies.
            _undefinedActuals =
                im.Where(i => !i.MatchingModelArguments.Any())
                    .Where(i => !i.Actual.IsUnnamedValuesNodeGroup)
                    .Where(i => !i.Actual.IsExeNodeGroup)
                    .Select(i => i.Actual)
                    .ToImmutableArray();
            var matchedModelArguments =
                    im.SelectMany(i => i.MatchingModelArguments)
                        .Distinct()
                        .ToArray();
            _missingModelArguments =
                model.Arguments.Where(m => m.Required)
                    .Where(m => !matchedModelArguments.Contains(m))
                    .ToImmutableArray();
            _matches =
                (from item in im
                 where item.MatchingModelArguments.Count() == 1
                 select new ActualModelMatch
                 (
                     item.Actual,
                     item.MatchingModelArguments.First()
                 )).ToImmutableArray();
        }

        public (CommandLineParseException, CommandLineParseException)
            Validate()
        {
            var errors = new List<CommandLineParseException>();
            var warnings = new List<CommandLineParseException>();
            ValidateUndefinedActuals(errors, warnings);
            ValidateAmbiguousActuals(errors, warnings);
            ValidateMissing(errors, warnings);
            ValidateUnnamedValues(errors, warnings);
            ValidateByMultiplicity(errors, warnings);
            ValidateFilesAndDirectories(errors, warnings);
            var errorEx = errors.Any() ? new CommandLineParseException(
                "One or more errors were encountered parsing the command line", errors)
                : null;
            var warningsEx = warnings.Any() ? new CommandLineParseException(
                "One or more warnings were encountered parsing the command line", warnings)
                : null;
            return (errorEx, warningsEx);
        }

        private void ValidateUndefinedActuals(
            List<CommandLineParseException> errors,
            List<CommandLineParseException> warnings)
        {
            var undefined =
                _undefinedActuals
                    .Select(u => $"The argument '{u.KeyNode.Text}' is undefined")
                    .Select(m => new CommandLineParseException(m));
            errors.AddRange(undefined);
        }

        private void ValidateAmbiguousActuals(
            List<CommandLineParseException> errors,
            List<CommandLineParseException> warnings)
        {
            var ambiguous =
                _ambiguousActuals
                    .Select(a => $"The argument '{a.KeyNode.Text}' is ambiguous")
                    .Select(m => new CommandLineParseException(m));
            errors.AddRange(ambiguous);
        }

        private void ValidateMissing(
            List<CommandLineParseException> errors,
            List<CommandLineParseException> warnings)
        {
            var missing =
                _missingModelArguments
                    .Select(m => $"The argument '{m.Names[0]}' is required")
                    .Select(m => new CommandLineParseException(m));
            errors.AddRange(missing);
        }

        private void ValidateUnnamedValues(
            List<CommandLineParseException> errors,
            List<CommandLineParseException> warnings)
        {
            if (!_model.AllowUnnamedValues && _actuals.Any(a => a.IsUnnamedValuesNodeGroup))
            {
                var msg = "Unnamed values are not permitted";
                var error = new CommandLineParseException(msg);
                errors.Add(error);
            }
            if (_actuals.Count(a => a.IsUnnamedValuesNodeGroup) > 2)
            {
                var ex = new CommandLineParseException("There are more than two unnamed value nodes. This is probably an error. Normally, there are at most one leading and one trailing unnamed value nodes");
                warnings.Add(ex);
            }
        }

        private void ValidateByMultiplicity(
            List<CommandLineParseException> errors,
            List<CommandLineParseException> warnings)
        {
            var exactlyOneValue =
                from match in _matches
                where match.Model.Multiplicity == ArgumentMultiplicity.One
                && match.Actual.ValueNodes.Count() != 1
                select $"The argument '{match.Actual.KeyNode.Text} is required to have exactly one value" into msg
                select new CommandLineParseException(msg);
            errors.AddRange(exactlyOneValue);
            var oneOrMoreValues =
                from match in _matches
                where match.Model.Multiplicity == ArgumentMultiplicity.OneOrMore
                && !match.Actual.HasValues
                select $"The argument '{match.Actual.KeyNode.Text} is required to have one or more values" into msg
                select new CommandLineParseException(msg);
            errors.AddRange(oneOrMoreValues);
            var zeroValues =
                from match in _matches
                where match.Model.Multiplicity == ArgumentMultiplicity.Zero
                && match.Actual.HasValues
                select $"The argument '{match.Actual.KeyNode.Text} may not have values" into msg
                select new CommandLineParseException(msg);
            errors.AddRange(zeroValues);
        }

        private void ValidateFilesAndDirectories(
            List<CommandLineParseException> errors,
            List<CommandLineParseException> warnings)
        {
            var fileNotFound =
                from match in _matches
                where match.Model.Flags.HasFlag(ArgumentFlags.ExistingFile)
                from fileNameNode in match.Actual.ValueNodes
                let fileName = fileNameNode.Text
                where !_filesystem.FileExists(fileName)
                select $"The file {fileName} could not be found" into msg
                select new CommandLineParseException(msg);
            errors.AddRange(fileNotFound);
            var dirNotFound =
                from match in _matches
                where match.Model.Flags.HasFlag(ArgumentFlags.ExistingDirectory)
                from dirNameNode in match.Actual.ValueNodes
                let dirName = dirNameNode.Text
                where !_filesystem.DirectoryExists(dirName)
                select $"The Directory {dirName} could not be found" into msg
                select new CommandLineParseException(msg);
            errors.AddRange(dirNotFound);
        }


        /// <summary>
        /// Gets the model passed into the constructor, for reference.
        /// </summary>
        public ParseModel Model => _model;

        /// <summary>
        /// Gets a collection of required model argumentss that are not specified 
        /// on the actual command line.
        /// </summary>
        public ImmutableArray<Argument> MissingModelArguments => _missingModelArguments;

        /// <summary>
        /// Gets a collection of argumentss specified on the actual command line 
        /// that match more than one model argument.
        /// </summary>
        internal ImmutableArray<CommandLineNodeGroup> AmbiguousActuals => _ambiguousActuals;

        /// <summary>
        /// Gets a collection of arguments specified on the actual command line 
        /// that don't couldn't be matched to any model item.
        /// </summary>
        internal ImmutableArray<CommandLineNodeGroup> UndefinedActuals => _undefinedActuals;

        /// <summary>
        /// Gets the actual command line passed into the object's constructor, 
        /// for reference.
        /// </summary>
        internal ImmutableArray<CommandLineNodeGroup> Actuals => _actuals;

        /// <summary>
        /// Gets a collection of actual command line nodes that match exactly 
        /// one model argument.
        /// </summary>
        internal ImmutableArray<ActualModelMatch> Matches => _matches;
    }
}
