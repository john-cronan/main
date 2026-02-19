using System.Text.RegularExpressions;

namespace JPC.Backup
{
    internal class ExcludeIfSourcePathMatchesRegex : IExcludeRule
    {
        //
        //  Note: Can't do RegexOptions.IgnoreCase here due to platform
        //  and filesystem differences. That must be specified in the
        //  expression itself.
        private const RegexOptions MatchOptions = RegexOptions.Compiled
            | RegexOptions.ExplicitCapture | RegexOptions.Singleline;

        private readonly string _expression;

        public ExcludeIfSourcePathMatchesRegex(string expression)
        {
            _expression = expression;
        }

        string IExcludeRule.FriendlyName => $"Matches Regular Expression: {_expression}";

        bool IExcludeRule.ExcludeObject(string sourcePath, string destinationPath)
            => Regex.IsMatch(sourcePath, _expression, MatchOptions);
    }
}
