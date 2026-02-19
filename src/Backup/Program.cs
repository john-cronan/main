using JPC.Common;
using Microsoft.Extensions.DependencyInjection;

namespace JPC.Backup
{
    public static class Program
    {
        public static async Task Main()
        {
            var specFile = FindSpecificationFile();
            var serviceContainer = ServiceContainer.Build(specFile);
            var root = serviceContainer.GetService<BackupProcessor>();
            await root.DoBackupAsync(specFile.SourcePath, specFile.DestinationPath,
                SpecificationFile.ToBackupOptions(specFile));
        }

        private static SpecificationFile FindSpecificationFile()
        {
            var services = new ServiceCollection();
            services.AddRuntimeWrappers();
            services.AddTransient<SpecificationFileFinder>();
            var tempContainer = services.BuildServiceProvider();
            var finder = tempContainer.GetService<SpecificationFileFinder>();
            finder.Find();
            if (string.IsNullOrWhiteSpace(finder.FoundFile.SourcePath))
            {
                var filesystem = tempContainer.GetService<IFilesystem>();
                finder.FoundFile.SourcePath = filesystem.GetDirectoryName(finder.FoundFilePath);
            }
            ValidateSpecificationFile(tempContainer.GetService<IRuntime>(), finder.FoundFile);
            return finder.FoundFile;
        }

        private static void ValidateSpecificationFile(IRuntime runtime,
            SpecificationFile specificationFile)
        {
            var errors = 0;

            //
            //  The source directory has to exist.
            if (string.IsNullOrWhiteSpace(specificationFile.SourcePath))
            {
                runtime.Console.WriteLine("Error: Source directory not specified");
                errors++;
            }
            else
            {
                var sourceDirectoryInfo = runtime.Filesystem.GetDirectoryInformation(specificationFile.SourcePath);
                if (sourceDirectoryInfo == null || !sourceDirectoryInfo.Exists)
                {
                    runtime.Console.WriteLine($"Error: Source directory {specificationFile.SourcePath} not found");
                    errors++;
                }
            }

            //
            //  The destination directory must at least be on a root that
            //  exists (it won't if, for example, it's on a device that's
            //  not currently mounted).
            if (string.IsNullOrWhiteSpace(specificationFile.DestinationPath))
            {
                runtime.Console.WriteLine("Error: Destination directory not specified");
                errors++;
            }
            else
            {
                var rootDirectory = runtime.Filesystem.GetDirectoryRoot(specificationFile.DestinationPath);
                var rootDirectoryInfo = runtime.Filesystem.GetDirectoryInformation(rootDirectory);
                if (!rootDirectoryInfo.Exists)
                {

                    runtime.Console.WriteLine($"Error: Destination {specificationFile.DestinationPath} is on " +
                        $"a volume that was not found or is not accessible");
                    errors++;
                }
            }

            if (specificationFile.MaxFileSize != null
                && specificationFile.MaxFileSize <= FileSize.Zero)
            {
                runtime.Console.WriteLine("Error: Max File Size, if specified, must " +
                    "be greater than zero");
                errors++;
            }

            if (!Enum.IsDefined(specificationFile.ComparisonMethod))
            {
                runtime.Console.WriteLine($"Error: {specificationFile.ComparisonMethod} is " +
                    $"not a valid file comparison method");
                errors++;
            }

            if (specificationFile.LogFile != null
                && !Enum.IsDefined(specificationFile.LogFile.OutputLevel))
            {
                runtime.Console.WriteLine($"Error: {specificationFile.LogFile.OutputLevel} " +
                    $"is not a valid output level");
                errors++;
            }

            if (specificationFile.MaxDepth != null && specificationFile.MaxDepth < 0)
            {
                runtime.Console.WriteLine("Error: Max Depth, if specified, must be zero " +
                    "or greater");
                errors++;
            }

            if (specificationFile.MaxRetriesOnFailure != null
                && specificationFile.MaxRetriesOnFailure < 0)
            {
                runtime.Console.WriteLine("Error: Max Retries on Failure, if specified, " +
                    "must be greater than zero");
                errors++;
            }

            if (specificationFile.OutputLevel != null 
                && !Enum.IsDefined(specificationFile.OutputLevel.Value))
            {
                runtime.Console.WriteLine($"Error: {specificationFile.OutputLevel} is " +
                    $"not a valid output level");
                errors++;
            }

            if (specificationFile.RetryDelay != null
                && specificationFile.RetryDelay < TimeSpan.Zero)
            {
                runtime.Console.WriteLine("Error: Retry Delay, if specified, must " +
                    "be greater than or equal to zero");
                errors++;
            }
            if (errors > 0)
            {
                runtime.Environment.Exit(ExitCodes.ConfigurationError);
            }
        }
    }
}
