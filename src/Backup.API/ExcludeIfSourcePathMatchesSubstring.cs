namespace JPC.Backup
{
    internal class ExcludeIfSourcePathMatchesSubstring : IExcludeRule
    {
        private readonly string _expression;
        private readonly bool _caseSensitive;

        public ExcludeIfSourcePathMatchesSubstring(string expression, bool caseSensitive)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentNullException(nameof(expression));
            }

            _expression = expression;
            _caseSensitive = caseSensitive;
        }

        string IExcludeRule.FriendlyName 
            => _caseSensitive
                ? $"Matches substring (case-sensitive): {_expression}"
                : $"Matches substring: {_expression}";

        bool IExcludeRule.ExcludeObject(string sourcePath, string destinationPath)
            => _caseSensitive
                ? sourcePath.Contains(_expression, StringComparison.InvariantCulture)
                : sourcePath.Contains(_expression, StringComparison.InvariantCultureIgnoreCase);
    }
}
