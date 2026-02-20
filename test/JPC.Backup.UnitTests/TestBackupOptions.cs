using JPC.Common;
using System.Collections.Immutable;

namespace JPC.Backup.UnitTests
{
    internal static class TestBackupOptions
    {
        public static BackupOptions Create(bool copySystemFiles = false, 
            FileSize? maxFileSize = null, 
            FileComparisonMethod comparisonMethod = FileComparisonMethod.LastWriteTimeDifferent,
            IEnumerable<MatchExpression> directoryStopExpressions = null,
            bool directoryStopOnColon = true,
            IEnumerable<MatchExpression> fileExcludeExpressions = null,
            bool resetArchiveBit = true, bool overwriteReadOnlyFiles = true, 
            int? maxDepth = null, int maxRetriesOnFailure = 0, 
            TimeSpan? retryDelay = null, bool whatIf = false)
        {
            return new BackupOptions(copySystemFiles, maxFileSize, comparisonMethod,
                directoryStopExpressions ?? ImmutableArray.Create<MatchExpression>(),
                directoryStopOnColon, 
                fileExcludeExpressions ?? ImmutableArray.Create<MatchExpression>(),
                resetArchiveBit, overwriteReadOnlyFiles, maxDepth,
                maxRetriesOnFailure, retryDelay, whatIf);
        }
    }
}
