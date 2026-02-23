using JPC.Common;
using Microsoft.Extensions.DependencyInjection;

namespace JPC.Backup
{
    public static class Program
    {
        public static async Task Main()
        {
            var serviceContainer = ServiceContainer.Build();
            var specFile = FindSpecificationFile(serviceContainer);
            InitializeEventSinks(serviceContainer, specFile);
            await InvokeBackupProcessor(serviceContainer, specFile);
        }

        private static Task InvokeBackupProcessor(IServiceProvider serviceContainer,
            SpecificationFile specFile)
        {
            var options = SpecificationFileHelper.ToBackupOptions(specFile);
            var runtime = serviceContainer.GetService<IRuntime>();
            var sourcePath = SpecificationFileHelper.GetSourcePath(specFile, runtime);
            var destinationPath = SpecificationFileHelper.GetDestinationPath(specFile, runtime);
            var root = serviceContainer.GetService<BackupProcessor>();
            return root.DoBackupAsync(sourcePath, destinationPath, options);
        }

        private static void InitializeEventSinks(IServiceProvider serviceContainer,
            SpecificationFile specFile)
        {
            var aggregator = serviceContainer.GetService<IBackupEvents>() as AggregatingBackupEventSink;
            var runtime = serviceContainer.GetService<IRuntime>();
            Startup.InitializeEventSinks(aggregator, specFile, runtime);
        }

        private static SpecificationFile FindSpecificationFile(IServiceProvider serviceContainer)
        {
            var finder = serviceContainer.GetService<SpecificationFileFinder>();
            finder.Find();
            if (string.IsNullOrWhiteSpace(finder.FoundFile.SourcePath)
                && string.IsNullOrWhiteSpace(finder.FoundFile.SourceVolume))
            {
                var filesystem = serviceContainer.GetService<IFilesystem>();
                finder.FoundFile.SourcePath = filesystem.GetDirectoryName(finder.FoundFilePath);
            }
            var runtime = serviceContainer.GetService<IRuntime>();
            if (!SpecificationFileHelper.Validate(finder.FoundFile, runtime))
            {
                runtime.Environment.Exit(ExitCodes.ConfigurationError);
            }
            return finder.FoundFile;
        }
    }
}
