using JPC.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace JPC.Backup
{
    internal static class ServiceContainer
    {    
        public static IServiceProvider Build(SpecificationFile specFile)
        {
            var services = new ServiceCollection();
            services.AddRuntimeWrappers();
            AddBackupAPI(services);
            BuildBackupEvents(services, specFile);
            return services.BuildServiceProvider();
        }

        private static void AddBackupAPI(ServiceCollection services)
        {
            services.AddTransient<BackupProcessor>();
            services.AddTransient<ISourceDirectoryWalkerBuilder, SourceDirectoryWalkerBuilder>();
            services.AddTransient<IDirectoryFileCopyFactory, DirectoryFileCopyFactory>();
        }

        private static void BuildBackupEvents(ServiceCollection services,
            SpecificationFile specFile)
        {

            var serviceProvider =
                new ServiceCollection()
                    .AddRuntimeWrappers()
                    .BuildServiceProvider();
            var runtime = serviceProvider.GetService<IRuntime>();
            var eventSinks = new List<IBackupEvents>();
            CreateConsoleBackupEventSink(specFile, eventSinks, runtime);
            CreateLogFileBackEventSink(specFile, eventSinks, runtime);
            switch (eventSinks.Count())
            {
                case 0:
                    services.AddSingleton<IBackupEvents>(new NullBackupEventSink());
                    break;
                case 1:
                    services.AddSingleton<IBackupEvents>(eventSinks.First());
                    break;
                case > 1:
                default:
                    services.AddSingleton<IBackupEvents>(new AggregatingBackupEventSink(eventSinks.ToImmutableArray()));
                    break;
            }
        }

        private static void CreateConsoleBackupEventSink(SpecificationFile specFile, 
            List<IBackupEvents> eventSinkCollection, IRuntime runtime)
        {
            if (specFile.OutputLevel == null)
            {
                eventSinkCollection.Add(new ConsoleBackupEventSink(LogLevel.Information, 
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
                    eventSinkCollection.Add(eventSink);
                }
            }
        }

        private static void CreateLogFileBackEventSink(SpecificationFile specFile,
            List<IBackupEvents> eventSinkCollection, IRuntime runtime)
        {
            if (specFile.LogFile == null)
            {
                return;
            }
            string fileNameAndPath = specFile.LogFile.FileName ??
                (runtime.Clock.DateTimeOffsetNow.ToString("yyyy.MM.dd-HH.mm.ss") + ".log");
            fileNameAndPath = runtime.Filesystem.CombinePath(specFile.DestinationPath, fileNameAndPath);
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
                eventSinkCollection.Add(eventSink);
            }
        }
    }
}
