using JPC.Common;
using System.Collections.Immutable;

namespace JPC.Backup
{
    public class SourceDirectoryWalkerBuilder : ISourceDirectoryWalkerBuilder
    {
        private IList<MatchExpression> _directoryStopExpressions;
        private bool _directoryStopOnColon;
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
            _directoryStopOnColon = false;
            _runtime = runtime;
            _events = events;
        }

        IList<MatchExpression> ISourceDirectoryWalkerBuilder.DirectoryStopExpressions
        {
            get => _directoryStopExpressions;
            set => _directoryStopExpressions = value;
        }

        bool ISourceDirectoryWalkerBuilder.DirectoryStopOnColon
        {
            get => _directoryStopOnColon;
            set => _directoryStopOnColon = value;
        }

        ISourceDirectoryWalker ISourceDirectoryWalkerBuilder.BuildSourceDirectoryWalker()
        {
            var stopRules = new List<IExcludeRule>();
            if (_directoryStopExpressions != null && _directoryStopExpressions.Any())
            {
                stopRules.AddRange(_directoryStopExpressions.Select(
                    e => ExpressionExcludeRules.ToExcludeRule(e)));
            }
            if (_directoryStopOnColon)
            {
                stopRules.Add(new ExcludeIfDirectoryNameHasColon(_runtime));

            }
            return new SourceDirectoryWalker(stopRules.ToImmutableArray(),
                _runtime, _events);
        }
    }
}
