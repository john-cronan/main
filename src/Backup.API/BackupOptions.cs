using JPC.Common;

namespace JPC.Backup
{
    public enum FileComparisonMethod
    {
        LastWriteTimeDifferent,
        LastWriteTimeNewer,
        SizeDifferent,
        ArchiveBit
    }

    public enum MatchType
    {
        Substring,
        CaseSensitiveSubstring,
        RegEx
    }

    /// <summary>
    /// Represents an immutable encapsulation of the properties of a string
    /// expression to be matched.
    /// </summary>
    public class MatchExpression
    {
        private readonly string _expression;
        private readonly MatchType _matchType;

        public MatchExpression(string expression, MatchType matchType)
        {
            _expression = expression;
            _matchType = matchType;
        }

        public string Expression => _expression;
        public MatchType MatchType => _matchType;

        public override bool Equals(object obj)
            => obj is MatchExpression ? Equals(obj as MatchExpression) : false;

        public bool Equals(MatchExpression other)
        {
            if (_expression != other._expression) return false; 
            if (_matchType != other._matchType) return false;
            return true;
        }

        public override int GetHashCode()
            => _expression.GetHashCode() ^ _matchType.GetHashCode();
    }

    /// <summary>
    /// Represents options governing a backup. This class' (deep) immutability 
    /// is critical to the application's design and must be maintained.
    /// </summary>
    public class BackupOptions
    {
        //
        //  Everything is readonly. This class must remain immutable for
        //  the application to function correctly.
        //

        private readonly bool _copySystemFiles;
        private readonly IEnumerable<MatchExpression> _directoryStopExpressions;
        private readonly FileSize? _maxFileSize;
        private readonly FileComparisonMethod _comparisonMethod;
        private readonly IEnumerable<MatchExpression> _fileExcludeExpressions;
        private readonly bool _resetArchiveBit;
        private readonly bool _overwriteReadOnlyFiles;
        private readonly int? _maxDepth;
        private readonly int _maxRetriesOnFailure;
        private readonly TimeSpan? _retryDelay;
        private readonly bool _whatIf;

        public BackupOptions(bool copySystemFiles, FileSize? maxFileSize,
            FileComparisonMethod comparisonMethod, 
            IEnumerable<MatchExpression> directoryStopExpressions,
            IEnumerable<MatchExpression> fileExcludeExpressions,
            bool resetArchiveBit, bool overwriteReadOnlyFiles, int? maxDepth,
            int maxRetriesOnFailure, TimeSpan? retryDelay, bool whatIf)
        {
            _copySystemFiles = copySystemFiles;
            _directoryStopExpressions = directoryStopExpressions ?? new MatchExpression[0];
            _maxFileSize = maxFileSize;
            _comparisonMethod = comparisonMethod;
            _fileExcludeExpressions = fileExcludeExpressions ?? new MatchExpression[0];
            _resetArchiveBit = resetArchiveBit;
            _overwriteReadOnlyFiles = overwriteReadOnlyFiles;
            _maxDepth = maxDepth;
            _maxRetriesOnFailure = maxRetriesOnFailure;
            _retryDelay = retryDelay;
            _whatIf = whatIf;
        }

        public bool CopySystemFiles => _copySystemFiles;
        public IEnumerable<MatchExpression> DirectoryStopExpressions => _directoryStopExpressions;
        public int? MaxDepth => _maxDepth;
        public FileSize? MaxFileSize => _maxFileSize;
        public IEnumerable<MatchExpression> FileExcludeExpressions => _fileExcludeExpressions;
        public bool ResetArchiveBit => _resetArchiveBit;
        public bool OverwriteReadOnlyFiles => _overwriteReadOnlyFiles;
        public int MaxRetriesOnFailure => _maxRetriesOnFailure;
        public TimeSpan? RetryDelay => _retryDelay;
        public FileComparisonMethod ComparisonMethod => _comparisonMethod;
        public bool WhatIf => _whatIf;


        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return obj is BackupOptions ? Equals((BackupOptions)obj) : false;
        }

        public bool Equals(BackupOptions obj)
        {
            if (_copySystemFiles != obj._copySystemFiles) return false;
            if (_comparisonMethod != obj._comparisonMethod) return false;
            if (!_directoryStopExpressions.SequenceEqual(obj._directoryStopExpressions)) return false;
            if (!_fileExcludeExpressions.SequenceEqual(obj._fileExcludeExpressions)) return false;
            if (_maxFileSize != obj._maxFileSize) return false;
            if (_maxRetriesOnFailure != obj._maxRetriesOnFailure) return false;
            if (_overwriteReadOnlyFiles != obj._overwriteReadOnlyFiles) return false;
            if (_retryDelay != obj._retryDelay) return false;
            if (_resetArchiveBit != obj._resetArchiveBit) return false;
            if (_whatIf != obj._whatIf) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var rv = _copySystemFiles.GetHashCode();
            rv ^= _comparisonMethod.GetHashCode();
            foreach (var d in _directoryStopExpressions)
            {
                rv ^= d.GetHashCode();
            }
            foreach (var f in _fileExcludeExpressions)
            {
                rv ^= f.GetHashCode();
            }
            rv ^= _maxRetriesOnFailure.GetHashCode();
            rv ^= _overwriteReadOnlyFiles.GetHashCode();
            rv ^= _retryDelay.GetHashCode();
            rv ^= _resetArchiveBit.GetHashCode();
            rv ^= _whatIf.GetHashCode();
            return rv;
        }
    }
}
