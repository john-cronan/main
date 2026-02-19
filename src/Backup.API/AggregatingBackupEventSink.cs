namespace JPC.Backup
{
    /// <summary>
    /// An implementation of <see cref="IBackupEvents"/> that broadcasts
    /// events to multiple sinks.
    /// </summary>
    public class AggregatingBackupEventSink : IBackupEvents
    {
        private readonly IEnumerable<IBackupEvents> _handlers;

        public AggregatingBackupEventSink(IEnumerable<IBackupEvents> handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            _handlers = handlers;
        }

        void IBackupEvents.AttemptingFile(string sourceFilePath)
        {
            foreach (var item in _handlers)
            {
                item.AttemptingFile(sourceFilePath);
            }
        }

        void IBackupEvents.Completed(string sourcePath, string destinationPath)
        {
            foreach (var item in _handlers)
            {
                item.Completed(sourcePath, destinationPath);
            }
        }

        void IBackupEvents.DirectoryAborted(string directoryPath, Exception ex)
        {
            foreach (var item in _handlers)
            {
                item.DirectoryAborted(directoryPath, ex);
            }
        }

        void IBackupEvents.DirectoryBegin(string directoryPath)
        {
            foreach (var item in _handlers)
            {
                item.DirectoryBegin(directoryPath);
            }
        }

        void IBackupEvents.DirectoryComplete(string directoryPath)
        {
            foreach (var item in _handlers)
            {
                item.DirectoryComplete(directoryPath);
            }
        }

        void IBackupEvents.DirectoryCreated(string directoryPath)
        {
            foreach (var item in _handlers)
            {
                item.DirectoryCreated(directoryPath);
            }
        }

        void IBackupEvents.DirectoryFailed(string directoryPath, string reason)
        {
            foreach (var item in _handlers)
            {
                item.DirectoryFailed(directoryPath, reason);
            }
        }

        void IBackupEvents.DirectoryStop(string directoryPath, 
            IEnumerable<string> rejectingRules)
        {
            foreach (var item in _handlers)
            {
                item.DirectoryStop(directoryPath, rejectingRules);
            }
        }

        void IBackupEvents.EnterRetryDelay(TimeSpan howLong)
        {
            foreach (var item in _handlers)
            {
                item.EnterRetryDelay(howLong);
            }
        }

        void IBackupEvents.Exception(Exception ex)
        {
            foreach(var item in _handlers)
            {
                item.Exception(ex);
            }
        }

        void IBackupEvents.ExitRetryDelay()
        {
            foreach (var item in _handlers)
            {
                item.ExitRetryDelay();
            }
        }

        void IBackupEvents.FileCopied(string sourcePath, string destinationPath, long? bytesCopied, TimeSpan? elapsedTime)
        {
            foreach (var item in _handlers)
            {
                item.FileCopied(sourcePath, destinationPath, bytesCopied, elapsedTime);
            }
        }

        void IBackupEvents.FileExcluded(string sourcePath, string destinationPath, IEnumerable<IExcludeRule> excludingRules)
        {
            foreach (var item in _handlers)
            {
                item.FileExcluded(sourcePath, destinationPath, excludingRules);
            }
        }

        void IBackupEvents.FileFailed(string sourcePath, string destinationPath, Exception ex)
        {
            foreach (var item in _handlers)
            {
                item.FileFailed(sourcePath, destinationPath, ex);
            }
        }

        void IBackupEvents.FilesEqual(string sourcePath, string destinationPath, IFileComparer accordingToComparer)
        {
            foreach (var item in _handlers)
            {
                item.FilesEqual(sourcePath, destinationPath, accordingToComparer);
            }
        }

        void IBackupEvents.FileTransientFailure(string sourcePath, string destinationPath, Exception ex, int attempt, int maxRetries, DateTimeOffset willRetryAt)
        {
            foreach (var item in _handlers)
            {
                item.FileTransientFailure(sourcePath, destinationPath, ex, attempt, maxRetries, willRetryAt);
            }
        }

        void IBackupEvents.PhantomFile(string sourcePath)
        {
            foreach (var item in _handlers)
            {
                item.PhantomFile(sourcePath);
            }
        }

        void IBackupEvents.RetryingFile(string sourcePath, string destinationPath)
        {
            foreach (var item in _handlers)
            {
                item.RetryingFile(sourcePath, destinationPath);
            }
        }

        void IBackupEvents.Started(string sourcePath, string destinationPath, BackupOptions options)
        {
            foreach (var item in _handlers)
            {
                item.Started(sourcePath, destinationPath, options);
            }
        }
    }
}
