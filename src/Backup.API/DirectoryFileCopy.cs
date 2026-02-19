using JPC.Common;
using System.Collections.Concurrent;

namespace JPC.Backup
{
    internal class DirectoryFileCopy : IDirectoryFileCopy
    {
        private readonly IFileComparer _fileComparer;
        private readonly IBackupFileOperations _fileOperations;
        private readonly IEnumerable<IExcludeRule> _excludeRules;
        private readonly IBackupEvents _events;
        private readonly IRuntime _runtime;
        private readonly BackupOptions _options;

        public DirectoryFileCopy(IFileComparer fileComparer,
            IEnumerable<IExcludeRule> excludeRules, IBackupFileOperations fileOperations,
            IRuntime runtime, IBackupEvents events, BackupOptions options)
        {
            if (fileComparer == null)
            {
                throw new ArgumentNullException(nameof(fileComparer));
            }
            if (excludeRules == null)
            {
                throw new ArgumentNullException(nameof(excludeRules));
            }
            if (fileOperations == null)
            {
                throw new ArgumentNullException(nameof(fileOperations));
            }
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _fileComparer = fileComparer;
            _excludeRules = excludeRules;
            _fileOperations = fileOperations;
            _runtime = runtime;
            _events = events;
            _options = options;
        }

        /// <summary>
        /// Returns the instance's <see cref="IBackupFileOperations"/> implementation.
        /// This property is provided for testibility purposes only and should not be
        /// used by application code.
        /// </summary>
        internal IBackupFileOperations FileOperations => _fileOperations;

        /// <summary>
        /// Returns the instance's <see cref="IFileComparer"/> implementation. This 
        /// property is provided for testibility purposes only and should not be
        /// used by application code.
        /// </summary>
        internal IFileComparer FileComparer => _fileComparer;

        /// <summary>
        /// Returns the instance's <see cref="IExcludeRule"/> collection. This property 
        /// is provided for testibility purposes only and should not be used by 
        /// application code.
        /// </summary>
        internal IEnumerable<IExcludeRule> ExcludeRules => _excludeRules;

        BackupOptions IDirectoryFileCopy.Options => _options;

        async Task IDirectoryFileCopy.CopyFilesAsync(
            string sourceDirectoryPath, string destinationDirectoryPath)
        {
            try
            {
                await CopyFilesAsync(sourceDirectoryPath, destinationDirectoryPath);
            }
            catch (Exception ex)
            {
                _events.DirectoryAborted(sourceDirectoryPath, ex);
            }
        }

        private async Task CopyFilesAsync(string sourceDirectoryPath,
            string destinationDirectoryPath)
        {
            var sourceDirectoryTimer = _runtime.Clock.StartTimer();
            var retryQueue = 
                _options.MaxRetriesOnFailure == 0 
                    ? (ConcurrentQueue<RetryQueueEntry>)null
                    : new ConcurrentQueue<RetryQueueEntry>();
            _events.DirectoryBegin(sourceDirectoryPath);
            if (await _fileOperations.EnsureDirectoryExistsAsync(destinationDirectoryPath))
            {
                _events.DirectoryCreated(destinationDirectoryPath);
            }
            foreach (var sourceFilePath in _fileOperations.EnumerateFiles(sourceDirectoryPath))
            {
                _events.AttemptingFile(sourceFilePath);
                var sourceFileInfo = _runtime.Filesystem.GetFileInformation(sourceFilePath);
                if (!sourceFileInfo.Exists)
                {
                    //  The file has gone away since it was enumerated; not much
                    //  we can do here.
                    _events.PhantomFile(sourceFilePath);
                    continue;
                }
                var destinationFilePath = PathUtility.ComputeDestinationPath(_runtime.Filesystem,
                    sourceDirectoryPath, destinationDirectoryPath, sourceFilePath);
                var destinationFileInfo = _runtime.Filesystem.GetFileInformation(
                    destinationFilePath);
                if (!PassesAllExcludeRules(sourceFilePath, destinationFilePath))
                {
                    continue;
                }
                if (!ShouldDoCopy(sourceFilePath, destinationFilePath, sourceFileInfo,
                    destinationFileInfo))
                {
                    continue;
                }
                try
                {
                    await AttemptCopyAsync(sourceFilePath, destinationFilePath, sourceFileInfo);
                }
                catch (Exception ex)
                {
                    HandleInitialCopyAttemptFailure(sourceFilePath, destinationFilePath,
                        retryQueue, ex);
                }
            }
            if (retryQueue != null)
            {
                await ProcessRetryQueueAsync(retryQueue);
            }
            var totalElapsedTime = _runtime.Clock.StopTimer(sourceDirectoryTimer);
            _events.DirectoryComplete(sourceDirectoryPath);
        }

        private bool ShouldDoCopy(string sourceFilePath, string destinationFilePath,
            FileInformation sourceFileInfo, FileInformation destinationFileInfo)
        {
            if (_fileComparer.ShouldCopy(sourceFilePath, destinationFilePath))
            {
                return true;
            }
            else
            {
                _events.FilesEqual(sourceFilePath, destinationFilePath, _fileComparer);
                return false;
            }
        }

        private bool PassesAllExcludeRules(string sourceFilePath, string destinationFilePath)
        {
            var failedExcludeRules = _excludeRules
                .Where(r => r.ExcludeObject(sourceFilePath, destinationFilePath))
                .ToArray();
            if (failedExcludeRules.Any())
            {
                _events.FileExcluded(sourceFilePath, destinationFilePath, failedExcludeRules);
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task AttemptCopyAsync(string sourceFilePath, string destinationFilePath,
            FileInformation sourceFileInfo)
        {
            var sourceFileTimer = _runtime.Clock.StartTimer();
            await _fileOperations.CopyAsync(sourceFilePath, destinationFilePath);
            await _fileOperations.AfterCopyAsync(sourceFilePath, destinationFilePath);
            var elapsed = _runtime.Clock.StopTimer(sourceFileTimer);
            var effectiveSourceFileInfo = sourceFileInfo ?? _runtime.Filesystem.GetFileInformation(sourceFilePath);
            _events.FileCopied(sourceFilePath, destinationFilePath,
                effectiveSourceFileInfo.Length, elapsed);
        }

        private void HandleInitialCopyAttemptFailure(string sourceFilePath,
            string destinationFilePath, ConcurrentQueue<RetryQueueEntry> retryQueue,
            Exception ex)
        {
            if (_options.MaxRetriesOnFailure < 1)
            {
                _events.FileFailed(sourceFilePath, destinationFilePath, ex);
            }
            else
            {
                var effectiveRetryDelay = _options.RetryDelay ?? TimeSpan.Zero;
                var retryAt = _runtime.Clock.DateTimeOffsetNow + effectiveRetryDelay;
                _events.FileTransientFailure(sourceFilePath, destinationFilePath, ex, 1,
                    _options.MaxRetriesOnFailure, retryAt);
                var retryQueueEntry = new RetryQueueEntry(sourceFilePath, destinationFilePath,
                    _options.MaxRetriesOnFailure, retryAt, effectiveRetryDelay, 1);
                retryQueue.Enqueue(retryQueueEntry);
            }
        }

        private async Task ProcessRetryQueueAsync(ConcurrentQueue<RetryQueueEntry> retryQueue)
        {
            while (retryQueue.TryDequeue(out var entry))
            {
                try
                {
                    var delayFor = entry.NextAttemptOnOrAfter - _runtime.Clock.DateTimeOffsetNow;
                    if (delayFor > TimeSpan.Zero)
                    {
                        _events.EnterRetryDelay(delayFor);
                        await _runtime.Clock.SleepAsync(delayFor);
                        _events.ExitRetryDelay();
                    }
                    await AttemptCopyAsync(entry.SourceFilePath, entry.DestinationFilePath, null);
                }
                catch (Exception ex)

                {
                    //
                    //  The retry count is equal to the number of attempts minus one
                    //  (for the initial attempt) plus one (for the attempt we just
                    //  made).
                    var retryCount = entry.AttemptCount;
                    if (retryCount >= entry.MaxRetries)
                    {
                        _events.FileFailed(entry.SourceFilePath, entry.DestinationFilePath, ex);
                    }
                    else
                    {
                        var nextRetryAt = _runtime.Clock.DateTimeOffsetNow + entry.RetryDelay;
                        retryQueue.Enqueue(entry.CreateRetry(nextRetryAt));
                    }
                }
            }
        }

        private class RetryQueueEntry
        {
            private readonly string _sourceFilePath;
            private readonly string _destinationFilePath;
            private readonly int _maxRetries;
            private readonly DateTimeOffset _nextAttemptOnOrAfter;
            private readonly TimeSpan _retryDelay;
            private readonly int _attemptCount;

            public RetryQueueEntry(string sourceFilePath, string destinationFilePath,
                int maxRetries, DateTimeOffset nextAttemptAtOrAfter, TimeSpan retryDelay,
                int attemptCount)
            {
                _sourceFilePath = sourceFilePath;
                _destinationFilePath = destinationFilePath;
                _maxRetries = maxRetries;
                _nextAttemptOnOrAfter = nextAttemptAtOrAfter;
                _retryDelay = retryDelay;
                _attemptCount = attemptCount;
            }

            public string SourceFilePath => _sourceFilePath;
            public string DestinationFilePath => _destinationFilePath;
            public int MaxRetries => _maxRetries;
            public DateTimeOffset NextAttemptOnOrAfter => _nextAttemptOnOrAfter;
            public TimeSpan RetryDelay => _retryDelay;
            public int AttemptCount => _attemptCount;

            public RetryQueueEntry CreateRetry(DateTimeOffset nextRetryAt)
                => new RetryQueueEntry(_sourceFilePath, _destinationFilePath, _maxRetries,
                    nextRetryAt, _retryDelay, _attemptCount + 1);
        }
    }
}
