using JPC.Common;

namespace JPC.Backup
{
    internal class WhatIfBackupFileOperations : BackupFileOperationsBase
    {
        private readonly IBackupEvents _events;
        private readonly IFilesystem _filesystem;

        public WhatIfBackupFileOperations(IFilesystem filesystem, IBackupEvents events)
            : base(filesystem)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));
            }
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            _filesystem = filesystem;
            _events = events;
        }

        protected override Task AfterCopyAsync(string source, string destination)
            => Task.CompletedTask;

        protected override Task CopyAsync(string source, string destination)
        {
            var sourceFileInfo = _filesystem.GetFileInformation(source);

            //
            //  These events must be published as if we had actually copied the file
            //  so the user is notified what *would have* happened if "What-If"
            //  mode was off.
            _events.FileCopied(source, destination, sourceFileInfo.Length, TimeSpan.Zero);
            return Task.CompletedTask;
        }

        protected override Task<bool> EnsureDirectoryExistsAsync(string directoryPath)
        {
            var directoryInfo = _filesystem.GetDirectoryInformation(directoryPath);
            if (directoryInfo.Exists)
            {
                return Task.FromResult(false);
            }
            else
            {
                //
                //  These events must be published as if we had actually created the
                //  directory  so the user is notified what *would have* happened if
                //  "What-If" mode was off.
                _events.DirectoryCreated(directoryPath);
                return Task.FromResult(true);
            }
        }
    }
}
