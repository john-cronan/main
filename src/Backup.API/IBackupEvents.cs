namespace JPC.Backup
{
    /// <summary>
    /// Defines the members implemented by classes that respond to events
    /// published during a backup operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A more sophisticated publish/subscribe eventing system could result 
    /// in excessive object creation, as the number of files and directories
    /// processed can be quite large, and there are many events published for
    /// each. A simple interface, implemented by event sinks, is a better
    /// choice here.
    /// </para>
    /// </remarks>
    public interface IBackupEvents
    {
        void AttemptingFile(string sourceFilePath);
        void Completed(string sourcePath, string destinationPath);
        void DirectoryAborted(string directoryPath, Exception ex);
        void DirectoryBegin(string directoryPath);
        void DirectoryComplete(string directoryPath);
        void DirectoryCreated(string directoryPath);
        void DirectoryFailed(string directoryPath, string reason);
        void DirectoryStop(string directoryPath, IEnumerable<string> rejectingRules);
        void EnterRetryDelay(TimeSpan howLong);
        void Exception(Exception ex);
        void ExitRetryDelay();  
        void FileCopied(string sourcePath, string destinationPath, 
            long? bytesCopied, TimeSpan? elapsedTime);

        void FileExcluded(string sourcePath, string destinationPath,
            IEnumerable<IExcludeRule> excludingRules);

        void FilesEqual(string sourcePath, string destinationPath,
            IFileComparer accordingToComparer);

        void FileFailed(string sourcePath, string destinationPath, Exception ex);

        void FileTransientFailure(string sourcePath, string destinationPath, Exception ex,
            int attempt, int maxRetries, DateTimeOffset willRetryAt);

        void PhantomFile(string sourcePath);

        void RetryingFile(string sourcePath, string destinationPath);
        void Started(string sourcePath, string destinationPath, BackupOptions options);
    }
}
