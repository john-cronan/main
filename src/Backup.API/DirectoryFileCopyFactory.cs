using JPC.Common;
using System.Collections.Immutable;

namespace JPC.Backup
{
    public class DirectoryFileCopyFactory : IDirectoryFileCopyFactory
    {
        private readonly IBackupEvents _events;
        private readonly IRuntime _runtime;

        public DirectoryFileCopyFactory(IRuntime runtime, IBackupEvents events)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            _runtime = runtime;
            _events = events;
        }

        IDirectoryFileCopy IDirectoryFileCopyFactory.Create(BackupOptions options,
            IDirectoryFileCopy existingInstance)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (existingInstance != null && existingInstance.Options.Equals(options))
            {
                //
                //  This is, by design, a form of caching. As long as the BackupOptions
                //  instance is unchanged, we keep returning the same instance. If it
                //  were to change (or the existing instance is null), we build a
                //  new instance and return it.
                return existingInstance;
            }

            var excludeRules = new List<IExcludeRule>();
            IFileComparer fileComparer = null;
            fileComparer = options.ComparisonMethod switch

            {
                FileComparisonMethod.SizeDifferent => new SizeIsDifferentFileComparer(_runtime.Filesystem),
                FileComparisonMethod.LastWriteTimeDifferent => new LastWriteTimeDifferentFileComparer(_runtime.Filesystem),
                FileComparisonMethod.LastWriteTimeNewer => new LastWriteTimeNewerFileComparer(_runtime.Filesystem),
                FileComparisonMethod.ArchiveBit => new ArchiveBitFileComparer(_runtime.Filesystem),
                _ => throw new ArgumentException("Invalid file comparison method value")
            };
            if (!options.CopySystemFiles)
            {
                excludeRules.Add(new ExcludeIfSourceIsSystemObject(_runtime.Filesystem));
            }
            foreach (var excludeExpression in options.FileExcludeExpressions)
            {
                excludeRules.Add(ExpressionExcludeRules.ToExcludeRule(excludeExpression));
            }
            if (options.MaxFileSize != null)
            {
                excludeRules.Add(new ExcludeIfFileOverSize(_runtime.Filesystem, options.MaxFileSize.Value));
            }
            if (!options.OverwriteReadOnlyFiles)
            {
                excludeRules.Add(new ExcludeIfDestinationFileReadOnly(_runtime.Filesystem));
            }
            IBackupFileOperations fileCopyOperations = options.WhatIf ?
                new WhatIfBackupFileOperations(_runtime.Filesystem, _events) :
                new BackupFileOperations(_runtime, options.ResetArchiveBit);
            return new DirectoryFileCopy(fileComparer, excludeRules.ToImmutableArray(),
                fileCopyOperations, _runtime, _events, options);
        }
    }
}
