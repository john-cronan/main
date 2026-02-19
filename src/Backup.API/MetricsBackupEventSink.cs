using JPC.Common;

namespace JPC.Backup
{
    /// <summary>
    /// Acts as a base class for event sinks that require copy metrics. This 
    /// will include anything that outputs such measures.
    /// </summary>
    /// <remarks>
    /// It's less than ideal to do this in a base class, as that leads to each
    /// derived class keeping its own private copy of these measures. But (1)
    /// there should normally only be one or two in effect at any given time; and
    /// (2) the other approaches considered would lead to awkward design issues.
    /// </remarks>
    public abstract class MetricsBackupEventSink : BackupEventSinkBase
    {
        private readonly IRuntime _runtime;
        private long _bytesCopied;
        private TimeSpan _copyTime;
        private object _elapsedTimeTimer;
        private TimeSpan _elapsedTime;
        private long _filesAttempted;
        private long _filesCopied;
        private int _filesFailed;
        private int _filesSkipped;
        private readonly object _lock;

        public MetricsBackupEventSink(IRuntime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            _lock = new object();
            _runtime = runtime;
        }

        protected long BytesCopied => _bytesCopied;
        protected TimeSpan CopyTime => _copyTime;
        protected TimeSpan ElapsedTime => _elapsedTime;
        protected long FilesAttempted => _filesAttempted;
        protected long FilesCopied => _filesCopied;
        protected int FilesFailed => _filesFailed;
        protected int FilesSkipped => _filesSkipped;
        protected object Lock => _lock;


        protected override void AttemptingFile(string sourceFilePath)
        {
            Interlocked.Increment(ref _filesAttempted);
        }

        protected override void Completed(string sourcePath, string destinationPath)
        {
            _elapsedTime = _runtime.Clock.StopTimer(_elapsedTimeTimer);
        }

        protected override void FileCopied(string sourcePath, string destinationPath,
            long? bytesCopied, TimeSpan? elapsedTime)
        {
            lock (_lock)
            {
                _filesCopied++;
                _bytesCopied += bytesCopied ?? 0;
                _copyTime += elapsedTime ?? TimeSpan.Zero;
            }
        }


        protected override void FileExcluded(string sourcePath, string destinationPath,
            IEnumerable<IExcludeRule> excludingRules)
        {
            Interlocked.Increment(ref _filesSkipped);
        }

        protected override void FileFailed(string sourcePath, string destinationPath, Exception ex)
        {
            Interlocked.Increment(ref _filesFailed);
        }

        protected override void Started(string sourcePath, string destinationPath, BackupOptions options)
        {
            _elapsedTimeTimer = _runtime.Clock.StartTimer();
        }
    }
}
