using JPC.Common;

namespace JPC.Backup
{
    public class SourceDirectoryWalker : ISourceDirectoryWalker
    {
        private readonly IBackupEvents _events;
        private readonly IRuntime _runtime;
        private readonly IEnumerable<IExcludeRule> _stopRules;

        public SourceDirectoryWalker(IEnumerable<IExcludeRule> stopRules, IRuntime runtime, 
            IBackupEvents events)
        {
            if (stopRules == null)
            {
                throw new ArgumentNullException(nameof(stopRules));
            }
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            _stopRules = stopRules;
            _runtime = runtime;
            _events = events;
        }

        IEnumerable<SourceDirectory> ISourceDirectoryWalker.Enumerate(string startingPath,
            BackupOptions options)
        {
            var directories = new Stack<DirectoryEntry>();
            directories.Push(new DirectoryEntry(startingPath, 0, options));
            while (directories.TryPop(out var current))
            
            {
                if (DirectoryNameHasColons(current.Path)) continue;
                if (AnyStopRuleRejects(current.Path)) continue;

                yield return new SourceDirectory(current.Path, options);

                //
                //  Note: All enumerated source directories get the same, global,
                //  options for now. A future enhancement may allow overriding 
                //  these options directory-by-directory.

                PushSubdirectories(directories, current, options);
            }
        }

        //TODO: May be a platform-specific problem. Reimplement as a stop rule, driven by
        //an option making it disablable at runtime.
        private bool DirectoryNameHasColons(string currentDirectoryPath)
        {
            var currentDirectoryName = _runtime.Filesystem.GetFileName(currentDirectoryPath);
            if (currentDirectoryName.Contains(':'))
            {
                //
                //  Inexplicably, this special case does happen. Visual Studio, for
                //  one, creates directories in the TestResults directory named, 
                //  for example, "Deploy_{user name} 2020-06-18 20:34:27". So it
                //  is possible. It's just not possible to enumerate or copy
                //  those directories.
                _events.DirectoryFailed(currentDirectoryPath, $"Unable to process directory " +
                    $"with the name '{currentDirectoryPath}'");
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AnyStopRuleRejects(string sourcePath)
        {
            var rejectingRules = _stopRules.Where(r => r.ExcludeObject(sourcePath, null));
            if (rejectingRules.Any())
            {
                _events.DirectoryStop(sourcePath, rejectingRules.Select(r => r.FriendlyName));
                return true;
            }
            else
            {
                return false;
            }
        }

        private void PushSubdirectories(Stack<DirectoryEntry> directories, 
            DirectoryEntry current, BackupOptions options)
        {
            if (options.MaxDepth == null || current.Depth + 1 <= options.MaxDepth)
            {
                try
                {
                    var subDirectoryNames =
                        _runtime.Filesystem.GetSubdirectoryNames(current.Path)
                            .OrderByDescending(_ => _);
                    foreach (var item in subDirectoryNames
                        .Select(p => new DirectoryEntry(p, current.Depth + 1, options)))
                    {
                        directories.Push(item);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    _events.Exception(ex);
                }
            }
        }

        private class DirectoryEntry
        {
            private readonly string _path;
            private readonly int _depth;
            private readonly BackupOptions _options;

            public DirectoryEntry(string path, int depth, BackupOptions options)
            {
                _path = path;
                _depth = depth;
                _options = options;
            }

            public string Path => _path;
            public int Depth => _depth;
            public BackupOptions Options => _options;
        }
    }
}
