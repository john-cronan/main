using JPC.Common;
using JPC.Common.JsonConverters;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JPC.Backup
{
    internal static class SpecificationFileHelper
    {
        public static string GetSourcePath(SpecificationFile specFile,
            IRuntime runtime)
        {
            var hasSourcePath = !string.IsNullOrWhiteSpace(specFile.SourcePath);
            var hasSourceVolume = !string.IsNullOrWhiteSpace(specFile.SourceVolume);
            if (hasSourcePath && hasSourceVolume)
            {
                throw new InvalidOptionException("SourcePath and SourceVolume cannot both be specified");
            }
            else if (hasSourcePath && !hasSourceVolume)
            {
                return specFile.SourcePath;
            }
            else if (!hasSourcePath && hasSourceVolume)
            {
                var drive = runtime.Filesystem.GetDrives()
                    .FirstOrDefault(d => d.VolumeLabel == specFile.SourceVolume
                        && d.IsReady);
                if (drive == null)
                {
                    throw new InvalidOptionException($"Volume '{specFile.SourceVolume}' not found or is not ready.");
                }
                return drive.RootDirectory;
            }
            else
            {
                throw new InvalidOptionException("Either SourcePath or SourceVolume must be specified");
            }
        }

        public static string GetDestinationPath(SpecificationFile specFile,
            IRuntime runtime)
        {
            var hasDestinationPath = !string.IsNullOrWhiteSpace(specFile.DestinationPath);
            var hasDestinationVolume = !string.IsNullOrWhiteSpace(specFile.DestinationVolume);
            if (hasDestinationPath && hasDestinationVolume)
            {
                throw new InvalidOptionException("DestinationPath and DestinationVolume cannot both be specified");
            }
            else if (hasDestinationPath && !hasDestinationVolume)
            {
                return specFile.DestinationPath;
            }
            else if (!hasDestinationPath && hasDestinationVolume)
            {
                var drive = runtime.Filesystem.GetDrives()
                    .FirstOrDefault(d => d.VolumeLabel == specFile.DestinationVolume
                        && d.IsReady);
                if (drive == null)
                {
                    throw new InvalidOptionException($"Volume '{specFile.DestinationVolume}' not found or is not ready.");
                }
                return drive.RootDirectory;
            }
            else
            {
                throw new InvalidOptionException("Either DestinationPath or DestinationVolume must be specified");
            }
        }

        public static SpecificationFile ParseJson(string json)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new FileSizeJsonConverter());
            options.PropertyNameCaseInsensitive = true;
            var instance = JsonSerializer.Deserialize<SpecificationFile>(json, options);
            return instance;
        }

        public static BackupOptions ToBackupOptions(SpecificationFile specFile)
        {
            var directoryStopExpressions =
                specFile.StopWhenDirectoryMatches == null
                    ? ImmutableArray.Create<MatchExpression>()
                    : specFile.StopWhenDirectoryMatches
                        .Select(s => new MatchExpression(s.Expression, s.MatchType))
                        .ToImmutableArray();
            var fileExcludeExpressions =
                specFile.ExcludeFilesMatching == null
                    ? ImmutableArray.Create<MatchExpression>()
                    : specFile.ExcludeFilesMatching
                        .Select(e => new MatchExpression(e.Expression, e.MatchType))
                        .ToImmutableArray();
            return new BackupOptions(
                (specFile.CopySystemFiles ?? false), specFile.MaxFileSize,
                specFile.ComparisonMethod, directoryStopExpressions,
                specFile.StopWhenDirectoryNameHasColon ?? false,
                fileExcludeExpressions, (specFile.ResetArchiveBit ?? true),
                (specFile.OverwriteReadOnlyFiles ?? true),
                specFile.MaxDepth, (specFile.MaxRetriesOnFailure ?? 1),
                specFile.RetryDelay, (specFile.WhatIf ?? false));
        }

        public static string ToJson(SpecificationFile specFile)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new FileSizeJsonConverter());
            return JsonSerializer.Serialize(specFile, options);
        }

        public static bool Validate(SpecificationFile specificationFile, IRuntime runtime)
        {
            var errors = 0;

            //
            //  The source directory has to exist.
            if (string.IsNullOrWhiteSpace(specificationFile.SourcePath)
                && string.IsNullOrWhiteSpace(specificationFile.SourceVolume))
            {
                runtime.Console.WriteLine("Error: Source not specified");
                errors++;
            }
            else if (!string.IsNullOrWhiteSpace(specificationFile.SourcePath))
            {
                var sourceDirectoryInfo = runtime.Filesystem.GetDirectoryInformation(specificationFile.SourcePath);
                if (sourceDirectoryInfo == null || !sourceDirectoryInfo.Exists)
                {
                    runtime.Console.WriteLine($"Error: Source directory {specificationFile.SourcePath} not found");
                    errors++;
                }
            }
            else if (!string.IsNullOrWhiteSpace(specificationFile.SourceVolume))
            {
                var drive = runtime.Filesystem.GetDrives()
                    .FirstOrDefault(d => d.VolumeLabel == specificationFile.SourceVolume
                        && d.IsReady);
                if (drive == null)
                {
                    runtime.Console.WriteLine($"Error: Volume '{specificationFile.SourceVolume}' not found or is not ready.");
                    errors++;
                }
            }
            else if (!string.IsNullOrWhiteSpace(specificationFile.SourcePath)
                && !string.IsNullOrWhiteSpace(specificationFile.SourceVolume))
            {
                runtime.Console.WriteLine("Error: Both SourcePath and SourceVolume specified");
            }

            //
            //  The destination directory must at least be on a root that
            //  exists (it won't if, for example, it's on a device that's
            //  not currently mounted).
            if (string.IsNullOrWhiteSpace(specificationFile.DestinationPath)
                && string.IsNullOrWhiteSpace(specificationFile.DestinationVolume))
            {
                runtime.Console.WriteLine("Error: Destination not specified");
                errors++;
            }
            else if (!string.IsNullOrWhiteSpace(specificationFile.DestinationPath))
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
            else if (!string.IsNullOrWhiteSpace(specificationFile.DestinationVolume))
            {
                var drive = runtime.Filesystem.GetDrives()
                    .FirstOrDefault(d => d.VolumeLabel == specificationFile.DestinationVolume
                        && d.IsReady);
                if (drive == null)
                {
                    runtime.Console.WriteLine($"Error: Volume '{specificationFile.DestinationVolume}' not found or is not ready.");
                    errors++;
                }
            }
            else if (!string.IsNullOrWhiteSpace(specificationFile.DestinationPath)
                && !string.IsNullOrWhiteSpace(specificationFile.DestinationVolume))
            {
                runtime.Console.WriteLine("Error: Both DestinationPath and DestinationVolume specified");
                errors++;
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
                    "must be zero or greater");
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
            return errors == 0;
        }
    }
}
