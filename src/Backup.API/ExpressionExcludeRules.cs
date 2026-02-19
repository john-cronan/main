namespace JPC.Backup
{
    internal static class ExpressionExcludeRules
    {
        public static IExcludeRule ToExcludeRule(MatchExpression matchExpression)
        {
            return matchExpression.MatchType switch
            {
                MatchType.RegEx => new ExcludeIfSourcePathMatchesRegex(
                    matchExpression.Expression),
                MatchType.CaseSensitiveSubstring => new ExcludeIfSourcePathMatchesSubstring(
                    matchExpression.Expression, true),
                MatchType.Substring => new ExcludeIfSourcePathMatchesSubstring(
                    matchExpression.Expression, false),
                _ => throw new ArgumentException($"Invalid MatchType {matchExpression.MatchType}")
            };
        }
    }
}
