using JPC.Common;
using System.Collections.Concurrent;
using System.Text;

namespace JPC.Backup
{
    public enum LogFileWriteMode
    {
        Append,
        Replace
    }

    public class LogFileBackupEventSink : MetricsBackupEventSink
    {
        private static readonly Encoding TextEncoding = Encoding.UTF8;

        private readonly IRuntime _runtime;
        private readonly string _fileNameAndPath;
        private readonly LogLevel _logLevel;
        private TextWriter _output;
        private readonly string _indent;
        private readonly LogFileWriteMode _writeMode;

        //
        //  Note: This collection should be populated in a lazy fashion,
        //  only adding an item when a reportable event occurs, in order
        //  to reduce unnecessary object creation (the number of 
        //  directories processed can be large).
        private readonly ConcurrentDictionary<string, DirectoryStatus> _directoryStatuses;

        public LogFileBackupEventSink(string fileNameAndPath, LogLevel logLevel,
            LogFileWriteMode overwriteMode, IRuntime runtime)
            : base(runtime)
        {
            if (string.IsNullOrWhiteSpace(fileNameAndPath))
            {
                throw new ArgumentNullException(nameof(fileNameAndPath));
            }
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            _fileNameAndPath = fileNameAndPath;
            _indent = "  ";
            _logLevel = logLevel;
            _runtime = runtime;
            _directoryStatuses = new ConcurrentDictionary<string, DirectoryStatus>();
            _writeMode = overwriteMode;
        }

        protected override void Completed(string sourcePath, string destinationPath)
        {
            base.Completed(sourcePath, destinationPath);
            lock (this.Lock)
            {
                _output.WriteLine($"Completed at {_runtime.Clock.Now.ToString("g")}");
                _output.WriteLine($"Total elapsed time: {ElapsedTime}");
                _output.WriteLine("");
                _output.WriteLine($"{FilesAttempted.ToString("#,##0")} files attempted");
                _output.WriteLine($"{FilesCopied.ToString("#,##0")} files copied");
                _output.WriteLine($"{BytesCopied.ToString("#,##0")} bytes copied");
                _output.WriteLine($"{FilesFailed.ToString("#,##0")} files failed");
                _output.WriteLine($"{FilesSkipped.ToString("#,##0")} files skipped");
            }
            _output.Close();
            _output.Dispose();
        }

        protected override void DirectoryAborted(string directoryPath, Exception ex)
        {
            base.DirectoryAborted(directoryPath, ex);

            _directoryStatuses.TryRemove(directoryPath, out var dirStatus);
            var outputLines = new List<string>();
            if (dirStatus != null && TestLogLevel(LogLevel.Information))
            {
                var anyInfoLevelEvents = dirStatus.FilesCopied.Any()
                    || dirStatus.FilesFailed.Any() || dirStatus.FilesSkipped.Any()
                    || dirStatus.PhantomFiles.Any();
                if (anyInfoLevelEvents)
                {
                    outputLines.Add(directoryPath);
                    AddDirectoryStatusCounts(outputLines, dirStatus);
                }
            }
            if (dirStatus != null && TestLogLevel(LogLevel.Verbose))
            {
                outputLines.Add(directoryPath);
                AddDirectoryStatusFileNames(outputLines, dirStatus);
            }
            outputLines.Add($"{_indent}Directory aborted due to " +
                $"{ex.GetType().Name}: {ex.Message}");
            var output = string.Join(_runtime.Environment.NewLine, outputLines);
            _output.WriteLine(output);
        }

        protected override void DirectoryComplete(string directoryPath)
        {
            base.DirectoryComplete(directoryPath);

            _directoryStatuses.TryRemove(directoryPath, out var dirStatus);
            var outputLines = new List<string>();
            if (dirStatus != null && TestLogLevel(LogLevel.Information))
            {
                var anyInfoLevelEvents = dirStatus.FilesCopied.Any()
                    || dirStatus.FilesFailed.Any() || dirStatus.FilesSkipped.Any()
                    || dirStatus.PhantomFiles.Any();
                if (anyInfoLevelEvents)
                {
                    outputLines.Add(directoryPath);
                    AddDirectoryStatusCounts(outputLines, dirStatus);
                }
            }
            if (dirStatus != null && TestLogLevel(LogLevel.Verbose))
            {
                outputLines.Add(directoryPath);
                AddDirectoryStatusFileNames(outputLines, dirStatus);
            }
            if (outputLines.Any())
            {
                var output = string.Join(_runtime.Environment.NewLine, outputLines.ToArray());
                _output.WriteLine(output);
            }
        }

        protected override void DirectoryCreated(string directoryPath)
        {
            base.DirectoryCreated(directoryPath);
            if (TestLogLevel(LogLevel.Information, LogLevel.Verbose))
            {
                _output.WriteLine($"Created {directoryPath}");
            }
        }

        protected override void DirectoryFailed(string directoryPath, string reason)
        {
            base.DirectoryFailed(directoryPath, reason);

            _directoryStatuses.TryRemove(directoryPath, out var dirStatus);
            var outputLines = new List<string>();
            if (dirStatus != null && TestLogLevel(LogLevel.Information))
            {
                var anyInfoLevelEvents = dirStatus.FilesCopied.Any()
                    || dirStatus.FilesFailed.Any() || dirStatus.FilesSkipped.Any()
                    || dirStatus.PhantomFiles.Any();
                if (anyInfoLevelEvents)
                {
                    outputLines.Add(directoryPath);
                    AddDirectoryStatusCounts(outputLines, dirStatus);
                }
                outputLines.Add($"{_indent}{reason}");
            } else if (dirStatus != null && TestLogLevel(LogLevel.Verbose))
            {
                outputLines.Add(directoryPath);
                AddDirectoryStatusFileNames(outputLines, dirStatus);
                outputLines.Add($"{_indent}{reason}");
            }
            else
            {
                outputLines.Add($"{reason}");
            }
            var output = string.Join(_runtime.Environment.NewLine, outputLines);
            _output.WriteLine(output);
        }

        protected override void DirectoryStop(string directoryPath, 
            IEnumerable<string> rejectingRules)
        {
            base.DirectoryStop(directoryPath, rejectingRules);

            if (TestLogLevel(LogLevel.Information, LogLevel.Verbose))
            {
                _output.WriteLine($"Directory {directoryPath} will not be processed or " +
                    "traversed due to one or more stop conditions");
            }
        }

        protected override void EnterRetryDelay(TimeSpan howLong)
        {
            base.EnterRetryDelay(howLong);
            if (TestLogLevel(LogLevel.Verbose))
            {
                _output.WriteLine($"Retry delay ({howLong})");
            }
        }

        protected override void Exception(Exception ex)
        {
            base.Exception(ex);
            _output.WriteLine(ex.Message);
        }

        protected override void FileCopied(string sourcePath, string destinationPath,
            long? bytesCopied, TimeSpan? elapsedTime)
        {
            base.FileCopied(sourcePath, destinationPath, bytesCopied, elapsedTime);

            var directoryPath = _runtime.Filesystem.GetDirectoryName(sourcePath);
            var dirStatus = _directoryStatuses.GetOrAdd(directoryPath,
                key => new DirectoryStatus(key));
            var fileName = _runtime.Filesystem.GetFileName(sourcePath);
            dirStatus.FilesCopied.Add(fileName);
            dirStatus.BytesCopied += BytesCopied;
        }

        protected override void FileExcluded(string sourcePath, string destinationPath, IEnumerable<IExcludeRule> excludingRules)
        {
            base.FileExcluded(sourcePath, destinationPath, excludingRules);

            var directoryPath = _runtime.Filesystem.GetDirectoryName(sourcePath);
            var dirStatus = _directoryStatuses.GetOrAdd(directoryPath,
                key => new DirectoryStatus(key));
            var fileName = _runtime.Filesystem.GetFileName(sourcePath);
            dirStatus.FilesSkipped.Add(fileName);
        }

        protected override void FileFailed(string sourcePath, string destinationPath,
            Exception ex)
        {
            base.FileFailed(sourcePath, destinationPath, ex);

            var directoryPath = _runtime.Filesystem.GetDirectoryName(sourcePath);
            var dirStatus = _directoryStatuses.GetOrAdd(directoryPath,
                key => new DirectoryStatus(key));
            var fileName = _runtime.Filesystem.GetFileName(sourcePath);
            dirStatus.FilesFailed.Add(fileName);

            _output.WriteLine($"Failed: {sourcePath} - {ex.GetType().Name} - {ex.Message}");
        }

        protected override void PhantomFile(string sourcePath)
        {
            base.PhantomFile(sourcePath);

            var directoryPath = _runtime.Filesystem.GetDirectoryName(sourcePath);
            var dirStatus = _directoryStatuses.GetOrAdd(directoryPath,
                key => new DirectoryStatus(key));
            var fileName = _runtime.Filesystem.GetFileName(sourcePath);
            dirStatus.PhantomFiles.Add(fileName);
        }

        protected override void Started(string sourcePath, string destinationPath, BackupOptions options)
        {
            base.Started(sourcePath, destinationPath, options);
            _output = new StreamWriter(_fileNameAndPath, TextEncoding, new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = _writeMode switch
                {
                    LogFileWriteMode.Append => FileMode.Append,
                    LogFileWriteMode.Replace => FileMode.Create,
                    _ => throw new ArgumentException($"{_writeMode} is not a valid value for WriteMode")
                },
                Share = FileShare.Read
            })
            { AutoFlush = true };
            _output.WriteLine($"Started at {_runtime.Clock.Now.ToString("g")}");
            if (options.WhatIf)
            {
                _output.WriteLine("WhatIf mode is on");
            }
        }


        private bool TestLogLevel(params LogLevel[] includedLevels)
            => includedLevels.Any(l => _logLevel == l);

        private void AddDirectoryStatusCounts(List<string> outputLines, 
            DirectoryStatus dirStatus)
        {
            if (dirStatus.FilesCopied.Any())
            {
                outputLines.Add($"{_indent}{dirStatus.FilesCopied.Count().ToString("#,##0")} " +
                    "file(s) copied");
            }
            if (dirStatus.BytesCopied > 0)
            {
                outputLines.Add($"{_indent}{dirStatus.BytesCopied.ToString("#,##0")} " +
                    "bytes copied");
            }
            if (dirStatus.FilesFailed.Any())
            {
                outputLines.Add($"{_indent}{dirStatus.FilesFailed.Count().ToString("#,##0")} " +
                    "file(s) failed");
            }
            if (dirStatus.FilesSkipped.Any())
            {
                outputLines.Add($"{{_indent}}{dirStatus.FilesSkipped.Count().ToString("#,##0")} " +
                    $"file(s) skipped");
            }
            if (dirStatus.PhantomFiles.Any())
            {
                outputLines.Add($"{_indent}{dirStatus.PhantomFiles.Count().ToString("#,##0")} " +
                    $"phantom file(s)");
            }
        }

        private void AddDirectoryStatusFileNames(List<string> outputLines,
            DirectoryStatus dirStatus)
        {
            if (dirStatus.BytesCopied > 0)
            {
                outputLines.Add($"{_indent}{dirStatus.BytesCopied.ToString("#,##0")} " +
                    "bytes copied");
            }
            foreach (var item in dirStatus.FilesCopied)
            {
                outputLines.Add($"{_indent}Copied: {item}");
            }
            foreach (var item in dirStatus.FilesFailed)
            {
                outputLines.Add($"{_indent}Failed: {item}");
            }
            foreach (var item in dirStatus.FilesSkipped)
            {
                outputLines.Add($"{_indent}Skipped: {item}");
            }
            foreach (var item in dirStatus.PhantomFiles)
            {
                outputLines.Add($"{_indent}Phantom: {item}");
            }
        }


        private class DirectoryStatus
        {
            private readonly string _path;

            public DirectoryStatus(string path)
            {
                _path = path;
                this.FilesCopied = new List<string>();
                this.FilesFailed = new List<string>();
                this.FilesSkipped = new List<string>();
                this.PhantomFiles = new List<string>();
            }

            public string Path => _path;
            public IList<string> FilesCopied { get; set; }
            public long BytesCopied { get; set; }
            public IList<string> FilesFailed { get; set; }
            public IList<string> FilesSkipped { get; set; }
            public IList<string> PhantomFiles { get; set; }
        }
    }
}
