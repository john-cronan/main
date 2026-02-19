using JPC.Common;
using System.Collections.Concurrent;

namespace JPC.Backup
{
    public class ConsoleBackupEventSink : MetricsBackupEventSink
    {
        private readonly LogLevel _logLevel;
        private readonly IRuntime _runtime;
        private readonly string _indent;

        //
        //  Note: This collection should be populated in a lazy fashion,
        //  only adding an item when a reportable event occurs, in order
        //  to reduce unnecessary object creation (the number of 
        //  directories processed can be large).
        private readonly ConcurrentDictionary<string, DirectoryStatus> _directoryStatuses;

        public ConsoleBackupEventSink(LogLevel logLevel, IRuntime runtime)
            : base(runtime)
        {
            if (runtime == null)

            {
                throw new ArgumentNullException(nameof(runtime));
            }

            _logLevel = logLevel;
            _runtime = runtime;
            _indent = "  ";
            _directoryStatuses = new ConcurrentDictionary<string, DirectoryStatus>();
        }

        private bool TestLogLevel(params LogLevel[] includedLevels)
            => includedLevels.Any(l => _logLevel == l);


        protected override void Completed(string sourcePath, string destinationPath)
        {
            base.Completed(sourcePath, destinationPath);
            if (TestLogLevel(LogLevel.Information, LogLevel.Verbose))
            {
                var outputLines = new string[]
                {
                    "",
                    $"Completed at {_runtime.Clock.Now.ToString("g")}",
                    $"Total elapsed time: {ElapsedTime}",
                    "",
                    $"{FilesAttempted.ToString("#,##0")} files attempted",
                    $"{FilesCopied.ToString("#,##0")} files copied",
                    $"{BytesCopied.ToString("#,##0")} bytes copied",
                    $"{FilesFailed.ToString("#,##0")} files failed",
                    $"{FilesSkipped.ToString("#,##0")} files skipped"
                };
                var output = string.Join(_runtime.Environment.NewLine, outputLines);
                _runtime.Console.WriteLine(output);
            }
        }

        protected override void DirectoryAborted(string directoryPath, Exception ex)
        {
            base.DirectoryAborted(directoryPath, ex);

            _directoryStatuses.TryRemove(directoryPath, out var dirStatus);
            var outputLines = new List<string>();
            outputLines.Add(directoryPath);
            if (dirStatus != null)
            {
                if (dirStatus.FilesCopied > 0)
                {
                    outputLines.Add($"{_indent}{dirStatus.FilesCopied.ToString("#,##0")} file(s) copied");
                }
                if (dirStatus.FilesSkipped > 0)
                {
                    outputLines.Add($"{_indent}{dirStatus.FilesSkipped.ToString("#,##0")} file(s) skipped");
                }
                foreach (var failedFileMsg in dirStatus.FilesFailed)
                {
                    outputLines.Add($"{_indent}{failedFileMsg}");
                }
                outputLines.Add($"{_indent}Copy aborted - {ex.GetType().Name}: {ex.Message})");
                var output = string.Join(_runtime.Environment.NewLine, outputLines);
                _runtime.Console.WriteLine(output);
            }
        }

        protected override void DirectoryComplete(string directoryPath)
        {
            base.DirectoryComplete(directoryPath);

            _directoryStatuses.TryRemove(directoryPath, out var dirStatus);
            if (TestLogLevel(LogLevel.Verbose))
            {
                var outputLines = new List<string>();
                outputLines.Add(directoryPath);
                if (dirStatus != null)
                {
                    if (dirStatus.FilesCopied > 0)
                    {
                        outputLines.Add($"{_indent}{dirStatus.FilesCopied.ToString("#,##0")} file(s) copied");
                    }
                    if (dirStatus.FilesSkipped > 0)
                    {
                        outputLines.Add($"{_indent}{dirStatus.FilesSkipped.ToString("#,##0")} file(s) skipped");
                    }
                    foreach (var failedFileMsg in dirStatus.FilesFailed)
                    {
                        outputLines.Add($"{_indent}{failedFileMsg}");
                    }
                    var output = string.Join(_runtime.Environment.NewLine, outputLines);
                    _runtime.Console.WriteLine(output);
                }
            }
        }

        protected override void DirectoryFailed(string directoryPath, string reason)
        {
            base.DirectoryFailed(directoryPath, reason);

            _directoryStatuses.TryRemove(directoryPath, out var dirStatus);
            var outputLines = new List<string>();
            outputLines.Add(directoryPath);
            if (dirStatus != null)
            {
                if (dirStatus.FilesCopied > 0)
                {
                    outputLines.Add($"{_indent}{dirStatus.FilesCopied.ToString("#,##0")} file(s) copied");
                }
                if (dirStatus.FilesSkipped > 0)
                {
                    outputLines.Add($"{_indent}{dirStatus.FilesSkipped.ToString("#,##0")} file(s) skipped");
                }
                foreach (var failedFileMsg in dirStatus.FilesFailed)
                {
                    outputLines.Add($"{_indent}{failedFileMsg}");
                }
                outputLines.Add($"{_indent}Copy failed - {reason})");
                var output = string.Join(_runtime.Environment.NewLine, outputLines);
                _runtime.Console.WriteLine(output);
            }
        }

        protected override void Exception(Exception ex)
        {
            base.Exception(ex);
            _runtime.Console.WriteLine(ex.Message);
        }

        protected override void FileCopied(string sourcePath, string destinationPath, long? bytesCopied, TimeSpan? elapsedTime)
        {
            base.FileCopied(sourcePath, destinationPath, bytesCopied, elapsedTime);
            var directoryPath = _runtime.Filesystem.GetDirectoryName(sourcePath);
            var dirStatus = _directoryStatuses.GetOrAdd(directoryPath,
                key => new DirectoryStatus(key));
            dirStatus.FilesCopied++;
            dirStatus.BytesCopied += (bytesCopied ?? 0);
        }

        protected override void FileExcluded(string sourcePath, string destinationPath, IEnumerable<IExcludeRule> excludingRules)
        {
            base.FileExcluded(sourcePath, destinationPath, excludingRules);
            var directoryPath = _runtime.Filesystem.GetDirectoryName(sourcePath);
            var dirStatus = _directoryStatuses.GetOrAdd(directoryPath,
                key => new DirectoryStatus(key));
            dirStatus.FilesSkipped++;
        }

        protected override void FileFailed(string sourcePath, string destinationPath,
            Exception ex)
        {
            base.FileFailed(sourcePath, destinationPath, ex);

            var message = $"{_indent}File failed: {_runtime.Filesystem.GetFileName(sourcePath)} " +
                $"({ex.GetType().Name}: {ex.Message})";
            _runtime.Console.WriteLine(message);
            var directoryPath = _runtime.Filesystem.GetDirectoryName(sourcePath);
            var dirStatus = _directoryStatuses.GetOrAdd(directoryPath,
                key => new DirectoryStatus(key));
            dirStatus.FilesFailed.Add(message);
        }

        protected override void Started(string sourcePath, string destinationPath,
            BackupOptions options)
        {
            base.Started(sourcePath, destinationPath, options);
            if (TestLogLevel(LogLevel.Information, LogLevel.Verbose))
            {
                _runtime.Console.WriteLine($"Started at {_runtime.Clock.Now.ToString("g")}");
            }
            if (options.WhatIf
                && TestLogLevel(LogLevel.Warning, LogLevel.Information, LogLevel.Verbose))
            {
                _runtime.Console.WriteLine("WhatIf mode is on");
            }
        }


        private class DirectoryStatus
        {
            private readonly string _path;

            public DirectoryStatus(string path)
            {
                _path = path;
                FilesFailed = new List<string>();
            }

            public int FilesCopied { get; set; }
            public long BytesCopied { get; set; }
            public IList<string> FilesFailed { get; set; }
            public int FilesSkipped { get; set; }
            public string Path => _path;
        }
    }
}
