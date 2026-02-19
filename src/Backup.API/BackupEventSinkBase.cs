namespace JPC.Backup
{
    public abstract class BackupEventSinkBase : IBackupEvents
    {
        protected virtual void AttemptingFile(string sourceFilePath)
        {
        }

        protected virtual void Completed(string sourcePath, string destinationPath)
        {
        }

        protected virtual void DirectoryAborted(string directoryPath, Exception ex)
        {
        }

        protected virtual void DirectoryBegin(string directoryPath)
        {
        }

        protected virtual void DirectoryComplete(string directoryPath)
        {
        }

        protected virtual void DirectoryCreated(string directoryPath)
        {
        }

        protected virtual void DirectoryFailed(string directoryPath, string reason)
        {
        }

        protected virtual void DirectoryStop(string directoryPath, IEnumerable<string> rejectingRules)
        {
        }

        protected virtual void EnterRetryDelay(TimeSpan howLong)
        {
        }

        protected virtual void Exception(Exception ex)
        {
        }

        protected virtual void ExitRetryDelay()
        {
        }

        protected virtual void FileCopied(string sourcePath, string destinationPath,
            long? bytesCopied, TimeSpan? elapsedTime)
        {
        }


        protected virtual void FileExcluded(string sourcePath, string destinationPath,
            IEnumerable<IExcludeRule> excludingRules)
        {
        }


        protected virtual void FilesEqual(string sourcePath, string destinationPath,
            IFileComparer accordingToComparer)
        {
        }


        protected virtual void FileFailed(string sourcePath, string destinationPath, Exception ex)
        {
        }


        protected virtual void FileTransientFailure(string sourcePath, string destinationPath, Exception ex,
            int attempt, int maxRetries, DateTimeOffset willRetryAt)
        {
        }


        protected virtual void PhantomFile(string sourcePath)
        {
        }


        protected virtual void RetryingFile(string sourcePath, string destinationPath)
        {
        }

        protected virtual void Started(string sourcePath, string destinationPath, BackupOptions options)
        {
        }






        void IBackupEvents.AttemptingFile(string sourceFilePath)
        {
            AttemptingFile(sourceFilePath);
        }

        void IBackupEvents.Completed(string sourcePath, string destinationPath)
        {
            Completed(sourcePath, destinationPath);
        }

        void IBackupEvents.DirectoryAborted(string directoryPath, Exception ex)
        {
            DirectoryAborted(directoryPath, ex);
        }

        void IBackupEvents.DirectoryBegin(string directoryPath)
        {
            DirectoryBegin(directoryPath);
        }

        void IBackupEvents.DirectoryComplete(string directoryPath)
        {
            DirectoryComplete(directoryPath);
        }

        void IBackupEvents.DirectoryCreated(string directoryPath)
        {
            DirectoryCreated(directoryPath);
        }

        void IBackupEvents.DirectoryFailed(string directoryPath, string reason)
        {
            DirectoryFailed(directoryPath, reason);
        }

        void IBackupEvents.DirectoryStop(string directoryPath, IEnumerable<string> rejectingRules)
        {
            DirectoryStop(directoryPath, rejectingRules);
        }

        void IBackupEvents.EnterRetryDelay(TimeSpan howLong)
        {
            EnterRetryDelay(howLong);
        }

        void IBackupEvents.Exception(Exception ex)
        {
            Exception(ex);
        }

        void IBackupEvents.ExitRetryDelay()
        {
            ExitRetryDelay();
        }

        void IBackupEvents.FileCopied(string sourcePath, string destinationPath,
            long? bytesCopied, TimeSpan? elapsedTime)
        {
            FileCopied(sourcePath, destinationPath, bytesCopied, elapsedTime);
        }

        void IBackupEvents.FileExcluded(string sourcePath, string destinationPath,
            IEnumerable<IExcludeRule> excludingRules)
        {
            FileExcluded(sourcePath, destinationPath, excludingRules);
        }

        void IBackupEvents.FileFailed(string sourcePath, string destinationPath, Exception ex)
        {
            FileFailed(sourcePath, destinationPath, ex);
        }

        void IBackupEvents.FilesEqual(string sourcePath, string destinationPath,
            IFileComparer accordingToComparer)
        {
            FilesEqual(sourcePath, destinationPath, accordingToComparer);
        }

        void IBackupEvents.FileTransientFailure(string sourcePath, string destinationPath,
            Exception ex, int attempt, int maxRetries, DateTimeOffset willRetryAt)
        {
            FileTransientFailure(sourcePath, destinationPath, ex, attempt, maxRetries,
                willRetryAt);
        }

        void IBackupEvents.PhantomFile(string sourcePath)
        {
            PhantomFile(sourcePath);
        }

        void IBackupEvents.RetryingFile(string sourcePath, string destinationPath)
        {
            RetryingFile(sourcePath, destinationPath);
        }

        void IBackupEvents.Started(string sourcePath, string destinationPath,
            BackupOptions options)
        {
            Started(sourcePath, destinationPath, options);
        }
    }
}
