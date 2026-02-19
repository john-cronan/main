using JPC.Common;

namespace JPC.Backup
{
    /// <summary>
    /// Forms the root object of a backup operation.
    /// </summary>
    public class BackupProcessor
    {
        private readonly ISourceDirectoryWalkerBuilder _directoryWalkerBuilder;
        private readonly IBackupEvents _events;
        private readonly IRuntime _runtime;
        private readonly IDirectoryFileCopyFactory _directoryFileCopyFactory;

        public BackupProcessor(IRuntime runtime, 
            ISourceDirectoryWalkerBuilder directoryWalkerBuilder,
            IDirectoryFileCopyFactory directoryFileCopyFactory, 
            IBackupEvents events)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }
            if (directoryWalkerBuilder == null)
            {
                throw new ArgumentNullException(nameof(directoryWalkerBuilder));
            }
            if (directoryFileCopyFactory == null)
            {
                throw new ArgumentNullException(nameof(directoryFileCopyFactory));
            }
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            _runtime = runtime;
            _directoryWalkerBuilder = directoryWalkerBuilder;
            _directoryFileCopyFactory = directoryFileCopyFactory;
            _events = events;
        }

        public async Task DoBackupAsync(string sourcePath, string destinationPath, 
            BackupOptions options)
        {
            Validate(sourcePath, destinationPath);
            _events.Started(sourcePath, destinationPath, options);
            try
            {
                IDirectoryFileCopy directoryFileCopy = null;
                var directoryWalker = BuildSourceDirectoryWalker(options);
                foreach (var sourceDirectory in directoryWalker.Enumerate(sourcePath, options))
                {
                    var destinationDirectoryPath = PathUtility.ComputeDestinationPath(
                        _runtime.Filesystem, sourcePath, destinationPath, sourceDirectory.Path);
                    directoryFileCopy = await CopyDirectoryAsync(sourceDirectory.Path,
                        destinationDirectoryPath, directoryFileCopy, sourceDirectory.Options);
                }
            }
            finally
            {
                _events.Completed(sourcePath, destinationPath);
            }
        }

        private ISourceDirectoryWalker BuildSourceDirectoryWalker(BackupOptions options)
        {
            _directoryWalkerBuilder.DirectoryStopExpressions.Clear();
            foreach (var expression in options.DirectoryStopExpressions)
            {
                _directoryWalkerBuilder.DirectoryStopExpressions.Add(expression);
            }
            return _directoryWalkerBuilder.BuildSourceDirectoryWalker();
        }

        private async Task<IDirectoryFileCopy> CopyDirectoryAsync(string copyFrom, 
            string copyTo, IDirectoryFileCopy existingDirectoryFileCopy,  
            BackupOptions options)
        {
            var directoryFileCopy = _directoryFileCopyFactory.Create(options, existingDirectoryFileCopy);
            await directoryFileCopy.CopyFilesAsync(copyFrom, copyTo);
            return directoryFileCopy;
        }

        private void Validate(string sourcePath, string destinationPath)
        {
            ValidateSourcePath(sourcePath);
            ValidateDestinationPath(destinationPath);
        }

        private void ValidateSourcePath(string sourcePath)
        {
            var sourceDirectoryInfo = _runtime.Filesystem.GetDirectoryInformation(sourcePath);
            if (sourceDirectoryInfo == null || !sourceDirectoryInfo.Exists)
            {
                throw new InvalidOperationException($"Source {sourcePath} not found or is not a directory");
            }
        }

        private void ValidateDestinationPath(string destinationPath)
        {
            //
            //  The destination directory needn't exist; it can be created, but
            //  if the root directory doesn't exist (maybe it's on a device that
            //  isn't currently mounted), that's a problem.
            var rootDirectory = _runtime.Filesystem.GetDirectoryRoot(destinationPath);
            var rootDirectoryInfo = _runtime.Filesystem.GetDirectoryInformation(rootDirectory);
            if (!rootDirectoryInfo.Exists)
            {
                throw new InvalidOperationException($"Destination {destinationPath} is on a volume that was not found or is not accessible");
            }
        }
    }
}
