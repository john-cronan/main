using System.Collections.Generic;
using JC.CommandLine.TargetTypeConverters;

namespace JC.CommandLine
{
    internal class ArgumentValueConverter
    {
        private readonly IFilesystem _filesystem;
        private readonly TargetTypeConverterCollection _converters;

        public ArgumentValueConverter()
            : this(new Filesystem())
        {
        }

        public ArgumentValueConverter(IFilesystem filesystem)
        {
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _filesystem = filesystem;
            _converters = new TargetTypeConverterCollection(_filesystem);
            _converters.Add<ReadExistingDirectoryConverter>();
            _converters.Add<ReadFileContentConverter>();
            _converters.Add<BinaryValueConverter>();
            _converters.Add<TypeDescriptorConverter>();
        }

        public IEnumerable<object> Convert(string value, TargetType targetType, ArgumentFlags argumentFlags)
        {
            Guard.IsNotNullOrWhitespace(value, nameof(value));
            Guard.IsNotNull(targetType, nameof(targetType));

            var conversionResult = _converters.TryConvert(value, targetType, argumentFlags);
            if (conversionResult.Success)
            {
                return conversionResult.Result;
            }
            else
            {
                var valueSnippet = value.Length > 50 ? (value.Substring(0, 47) + "...") : value;
                string msg = $"Unable to convert '{valueSnippet}' to type {targetType}";
                throw new CommandLineParseException(msg);
            }
        }
    }
}
