using JPC.Common;
using System.Collections.Immutable;

namespace JPC.Backup
{
    public class SourceDirectoryWalkerBuilder : ISourceDirectoryWalkerBuilder
    {
        private IList<MatchExpression> _directoryStopExpressions;
        private readonly IRuntime _runtime;
        private readonly IBackupEvents _events;

        public SourceDirectoryWalkerBuilder(IRuntime runtime, IBackupEvents events)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            _directoryStopExpressions = new List<MatchExpression>();
            _runtime = runtime;
            _events = events;
        }

        IList<MatchExpression> ISourceDirectoryWalkerBuilder.DirectoryStopExpressions
        {
            get => _directoryStopExpressions;
            set => _directoryStopExpressions = value;
        }

        ISourceDirectoryWalker ISourceDirectoryWalkerBuilder.BuildSourceDirectoryWalker()
        {
            var stopRules = new List<IExcludeRule>();
            if (_directoryStopExpressions != null && _directoryStopExpressions.Any())
            {
                stopRules.AddRange(_directoryStopExpressions.Select(
                    e => ExpressionExcludeRules.ToExcludeRule(e)));
            }
            return new SourceDirectoryWalker(stopRules.ToImmutableArray(),
                _runtime, _events);
        }
    }
}
