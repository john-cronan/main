using JPC.Common;

namespace JPC.Backup
{
    internal static class Startup
    {
        public static void InitializeEventSinks(AggregatingBackupEventSink aggregator,
            SpecificationFile specFile, IRuntime runtime)
        {
            InitializeConsoleBackupEventSink(specFile, aggregator, runtime);
            InitializeLogFileBackEventSink(specFile, aggregator, runtime);
        }

        private static void InitializeConsoleBackupEventSink(SpecificationFile specFile,
            AggregatingBackupEventSink aggregator, IRuntime runtime)
        {
            if (specFile.OutputLevel == null)
            {
                aggregator.Add(new ConsoleBackupEventSink(LogLevel.Information,
                    runtime));
            }
            else
            {
                IBackupEvents eventSink = specFile.OutputLevel.Value switch
                {
                    OutputLevel.Error => new ConsoleBackupEventSink(LogLevel.Error, runtime),
                    OutputLevel.Information => new ConsoleBackupEventSink(LogLevel.Information, runtime),
                    OutputLevel.None => null,
                    OutputLevel.Verbose => new ConsoleBackupEventSink(LogLevel.Verbose, runtime),
                    OutputLevel.Warning => new ConsoleBackupEventSink(LogLevel.Warning, runtime),
                    _ => throw new InvalidOptionException($"{specFile.OutputLevel.Value} is not a valid value for OutputLevel")
                };
                if (eventSink != null)
                {
                    aggregator.Add(eventSink);
                }
            }
        }

        private static void InitializeLogFileBackEventSink(SpecificationFile specFile,
            AggregatingBackupEventSink aggregator, IRuntime runtime)
        {
            if (specFile.LogFile == null)
            {
                return;
            }
            var destinationPath = SpecificationFileHelper.GetDestinationPath(specFile, runtime);
            string fileNameAndPath = specFile.LogFile.FileName ??
                (runtime.Clock.DateTimeOffsetNow.ToString("yyyy.MM.dd-HH.mm.ss") + ".log");
            fileNameAndPath = runtime.Filesystem.CombinePath(destinationPath, fileNameAndPath);
            var overwriteMode = specFile.LogFile.OverwriteMode ?? LogFileWriteMode.Append;
            IBackupEvents eventSink = specFile.LogFile.OutputLevel switch
            {
                OutputLevel.Error => new LogFileBackupEventSink(fileNameAndPath,
                    LogLevel.Error, overwriteMode, runtime),
                OutputLevel.Information => new LogFileBackupEventSink(fileNameAndPath,
                    LogLevel.Information, overwriteMode, runtime),
                OutputLevel.None => null,
                OutputLevel.Verbose => new LogFileBackupEventSink(fileNameAndPath,
                    LogLevel.Verbose, overwriteMode, runtime),
                OutputLevel.Warning => new LogFileBackupEventSink(fileNameAndPath,
                    LogLevel.Warning, overwriteMode, runtime),
                _ => throw new InvalidOptionException($"{specFile.OutputLevel.Value} is not a valid value for OutputLevel")
            };
            if (eventSink != null)
            {
                aggregator.Add(eventSink);
            }
        }
    }
}
