using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine
{
    internal class CommandLineParseResults : ICommandLineParseResults
    {
        private readonly ActualModelResolution _actualModelResolution;
        private readonly IObjectBinder _objectBinder;
        private readonly ArgumentValueConverter _converter;
        private readonly Lazy<CommandLineParseException> _parseWarnings;
        private readonly ICommandLineParseResults _self;

        public CommandLineParseResults(IObjectBinder objectBinder,
            ActualModelResolution actualModelResolution,
            IFilesystem filesystem)
        {
            Guard.IsNotNull(objectBinder, nameof(objectBinder));
            Guard.IsNotNull(actualModelResolution, nameof(actualModelResolution));
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _self = this;
            _objectBinder = objectBinder;
            _actualModelResolution = actualModelResolution;
            _converter = new ArgumentValueConverter(filesystem);

            _parseWarnings = new Lazy<CommandLineParseException>(() =>
            {
                (var errors, var warnings) = _actualModelResolution.Validate();
                return warnings;
            });
        }

        CommandLineParseException ICommandLineParseResults.ParseWarnings => _parseWarnings.Value;

        T ICommandLineParseResults.Bind<T>()
        {
            return _objectBinder.CreateObject<T>(_actualModelResolution);
        }

        string ICommandLineParseResults.GetValue(string argumentName)
        {
            Guard.IsNotNullOrWhitespace(argumentName, nameof(argumentName));

            var match = FindSingleArgument(argumentName, true);
            var valueAsString = GetSingleValueAsString(match);
            return valueAsString;
        }

        T ICommandLineParseResults.GetValueAs<T>(string argumentName)
        {
            Guard.IsNotNullOrWhitespace(argumentName, nameof(argumentName));

            var match = FindSingleArgument(argumentName, true);
            var valueAsString = _self.GetValue(argumentName);
            var converted = _converter.Convert(valueAsString, typeof(T), match.Model.Flags);
            if (converted.Count() > 1)
            {
                throw new ArgumentException($"The argument '{argumentName}' has more than one value", nameof(argumentName));
            }
            return (T)converted.First();
        }

        IEnumerable<string> ICommandLineParseResults.GetValues(string argumentName)
        {
            Guard.IsNotNullOrWhitespace(argumentName, nameof(argumentName));

            var match = FindSingleArgument(argumentName, true);
            var valuesAsStrings = GetValuesAsStrings(match);
            return valuesAsStrings;
        }

        IEnumerable<T> ICommandLineParseResults.GetValuesAs<T>(string argumentName)
        {
            Guard.IsNotNullOrWhitespace(argumentName, nameof(argumentName));

            var match = FindSingleArgument(argumentName, true);
            var valuesAsStrings = GetValuesAsStrings(match);
            var targetType = new TargetType(typeof(IEnumerable<T>));
            var converted = valuesAsStrings.SelectMany(v => _converter.Convert(v, targetType, match.Model.Flags));
            foreach (var item in converted)
            {
                yield return (T)item;
            }
        }

        bool ICommandLineParseResults.IsPresent(string argumentName)
        {
            Guard.IsNotNullOrWhitespace(argumentName, nameof(argumentName));

            var match = FindSingleArgument(argumentName, false);
            return match != null;
        }

        private ActualModelMatch FindSingleArgument(string name, bool throwOnNotFound)
        {
            var nameMatching = _actualModelResolution.Model.NameMatching;
            var stringComparisons = _actualModelResolution.Model.StringComparisons;
            var m =
                (from match in _actualModelResolution.Matches
                 where NameMatching.IsMatch(name, match.Model.Names, nameMatching, stringComparisons)
                 select match).FirstOrDefault();
            if (m == null && throwOnNotFound)
            {
                throw new ArgumentException($"An argument cannot be found with the name '{name}'", nameof(name));
            }
            return m;
        }

        private string GetSingleValueAsString(ActualModelMatch match)
        {
            var strings = match.Actual.ValueNodes.Select(n => n.Text);
            if (!strings.Any())
            {
                return null;
            }
            if (strings.Count() > 1)
            {
                throw new ArgumentException("The specified argument has more than one value");
            }
            return strings.Single();
        }

        private IEnumerable<string> GetValuesAsStrings(ActualModelMatch match)
        {
            var strings = match.Actual.ValueNodes.Select(n => n.Text);
            if (!strings.Any())
            {
                return new string[0];
            }
            return strings;
        }
    }
}
