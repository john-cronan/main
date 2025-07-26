using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JC.CommandLine.TargetTypeConverters
{
    internal class ReadExistingDirectoryConverter : TargetTypeConverter
    {
        public ReadExistingDirectoryConverter(
            ITargetTypeConverterInstances otherConverters,
            IFilesystem filesystem)
            : base(otherConverters, filesystem)
        {
        }

        public override TargetTypeConverterResult TryConvert(string value,
            TargetType targetType, ArgumentFlags argumentFlags)
        {
            Guard.IsNotNullOrWhitespace(value, nameof(value));
            Guard.IsNotNull(targetType, nameof(targetType));

            if (argumentFlags.HasFlag(ArgumentFlags.ExistingDirectory))
            {
                return Convert(value, targetType, argumentFlags);
            }
            else
            {
                return TargetTypeConverterResult.Unsucessful;
            }
        }

        private TargetTypeConverterResult Convert(string value,
            TargetType targetType, ArgumentFlags argumentFlags)
        {
            IEnumerable<object> result = null;
            var valueExpanded = Environment.ExpandEnvironmentVariables(value);
            var absolutePath = Filesystem.MakePathFullyQualified(valueExpanded);
            if (!Filesystem.DirectoryExists(absolutePath))
            {
                throw new CommandLineParseException($"Directory '{absolutePath}' not found");
            }
            switch (targetType.IsVectorType)
            {
                case true when targetType.ScalarType == typeof(FileSystemInfo):
                    result = Filesystem.GetFileSystemEntries(absolutePath);
                    break;
                case true when targetType.ScalarType == typeof(DirectoryInfo):
                    result = Filesystem.GetFileSystemEntries(absolutePath).OfType<DirectoryInfo>();
                    break;
                case true when targetType.ScalarType == typeof(FileInfo):
                    result = Filesystem.GetFileSystemEntries(absolutePath).OfType<FileInfo>();
                    break;
                case false when targetType.Target == typeof(string):
                    result = new object[] { absolutePath };
                    break;
                case false when typeof(FileSystemInfo).IsAssignableFrom(targetType.Target):
                    result = new object[] { new DirectoryInfo(absolutePath) };
                    break;
                default:
                    throw new CommandLineParseException($"Type {targetType} is not valid for {ArgumentFlags.ExistingDirectory}");
            }
            return TargetTypeConverterResult.FromResult(result);
        }
    }
}
