using JPC.Common;

namespace JPC.Backup
{
    internal abstract class BackupFileOperationsBase : IBackupFileOperations
    {
        private readonly IFilesystem _filesystem;

        protected BackupFileOperationsBase(IFilesystem filesystem)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));
            }

            _filesystem = filesystem;
        }

        protected virtual Task AfterCopyAsync(string source, string destination)
        {
            return Task.CompletedTask;
        }

        protected virtual Task CopyAsync(string source, string destination)
        {
            return Task.CompletedTask;
        }

        protected virtual Task<bool> EnsureDirectoryExistsAsync(string directoryPath)
        {
            return Task.FromResult(false);
        }

        protected virtual IEnumerable<string> EnumerateFiles(string inDirectory)
        {
            return _filesystem.EnumerateFiles(inDirectory);
        }


        Task IBackupFileOperations.AfterCopyAsync(string source, string destination)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentNullException(nameof(destination));
            }

            return AfterCopyAsync(source, destination);
        }
        Task IBackupFileOperations.CopyAsync(string source, string destination)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentNullException(nameof(destination));
            }


            return CopyAsync(source, destination);
        }

        Task<bool> IBackupFileOperations.EnsureDirectoryExistsAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            return EnsureDirectoryExistsAsync(directoryPath);
        }

        IEnumerable<string> IBackupFileOperations.EnumerateFiles(string inDirectory)
            => EnumerateFiles(inDirectory);
    }
}
