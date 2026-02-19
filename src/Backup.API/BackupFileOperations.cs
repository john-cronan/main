using JPC.Common;

namespace JPC.Backup
{
    /// <summary>
    /// Forms the primary concrete implementation of 
    /// <see cref="IBackupFileOperations"/>. This implementation is designed
    /// to be in effect whenever WhatIf mode is off.
    /// </summary>
    internal class BackupFileOperations : BackupFileOperationsBase
    {
        private readonly IFilesystem _filesystem;
        private readonly bool _resetArchiveBit;

        public BackupFileOperations(IRuntime runtime, bool resetArchiveBit)
            : base(runtime.Filesystem)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            _filesystem = runtime.Filesystem;
            _resetArchiveBit = resetArchiveBit;
        }

        public bool ResetArchiveBit => _resetArchiveBit;

        protected override Task AfterCopyAsync(string source, string destination)
        {
            if (_resetArchiveBit)
            {
                var sourceFileInfo = _filesystem.GetFileInformation(source);
                sourceFileInfo.Attributes = sourceFileInfo.Attributes & ~FileAttributes.Archive;
                _filesystem.SetFileInformation(sourceFileInfo);
            }
            return Task.CompletedTask;
        }

        protected override Task CopyAsync(string source, string destination)
        {
            ///TODO: Make this async? And how? The framework has no platform-independent
            ///mechanism to copy in an asynchronous fashion. Async Stream operations 
            ///would have to be used, which may deny optimizations underlying 
            ///File.Copy. And performance may not improve by doing multiple copies
            ///in parallel anyway.
            _filesystem.CopyFile(source, destination, true);
            return Task.CompletedTask;
        }

        protected override Task<bool> EnsureDirectoryExistsAsync(string directoryPath)
        {
            var directoryInfo = _filesystem.GetDirectoryInformation(directoryPath);
            if (!directoryInfo.Exists)
            {
                _filesystem.CreateDirectory(directoryPath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
