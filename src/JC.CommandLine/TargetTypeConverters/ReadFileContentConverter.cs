using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace JC.CommandLine.TargetTypeConverters
{
    internal class ReadFileContentConverter : TargetTypeConverter
    {
        public ReadFileContentConverter(ITargetTypeConverterInstances otherInstances,
            IFilesystem filesystem)
            : base(otherInstances, filesystem)
        {
        }

        public override TargetTypeConverterResult TryConvert(string value,
            TargetType targetType, ArgumentFlags argumentFlags)
        {
            Guard.IsNotNullOrWhitespace(value, nameof(value));
            Guard.IsNotNull(targetType, nameof(targetType));

            if (argumentFlags.HasFlag(ArgumentFlags.ReadFileContent))
            {
                var result = Convert(value, targetType, argumentFlags);
                return TargetTypeConverterResult.FromResult(result);
            }
            else
            {
                return TargetTypeConverterResult.Unsucessful;
            }
        }

        private IEnumerable<object> Convert(string value, TargetType targetType,
            ArgumentFlags argumentFlags)
        {
            IEnumerable<object> result = null;
            var expandedPath = Environment.ExpandEnvironmentVariables(value);
            if (targetType == typeof(string))
            {
                var content = Filesystem.ReadAllText(expandedPath);
                result = new object[] { content };
            }
            else if (targetType == typeof(string[])
                || targetType == typeof(IEnumerable<string>)
                || targetType == typeof(ImmutableArray<string>)
                || targetType == typeof(IEnumerable)
                || targetType == typeof(IReadOnlyCollection<string>)
                || targetType == typeof(IList)
                || targetType == typeof(IList<string>)
                || targetType == typeof(ICollection<string>))
            {
                var lines = Filesystem.ReadAllLines(expandedPath);
                result = lines;
            }
            else if (targetType == typeof(byte[])
                || targetType == typeof(IEnumerable<byte>)
                || targetType == typeof(ImmutableArray<byte>)
                || targetType == typeof(IEnumerable)
                || targetType == typeof(IReadOnlyCollection<byte>)
                || targetType == typeof(IList)
                || targetType == typeof(IList<byte>)
                || targetType == typeof(ICollection<byte>))
            {
                var bytes = Filesystem.ReadAllBytes(expandedPath);
                result = bytes.Select(b => (object)b);
            }
            else if (targetType == typeof(XmlDocument))
            {
                var documentText = (string)Convert(expandedPath, typeof(string), argumentFlags).First();
                var doc = new XmlDocument();
                using (var reader = new StringReader(documentText))
                {
                    doc.Load(reader);
                }
                result = new object[] { doc };
            }
            else if (targetType == typeof(XmlNode))
            {
                var doc = (XmlDocument)Convert(expandedPath, typeof(XmlDocument), 
                    argumentFlags).First();
                result = new object[] { doc.DocumentElement };
            }
            else if (targetType == typeof(XDocument))
            {
                var documentText = (string)Convert(expandedPath, typeof(string), 
                    argumentFlags).First();
                using (var reader = new StringReader(documentText))
                {
                    var doc = XDocument.Load(reader);
                    result = new object[] { doc };
                }
            }
            else if (targetType == typeof(XElement)
                || targetType == typeof(XNode))
            {
                var doc = (XDocument)Convert(expandedPath, typeof(XDocument),
                    argumentFlags).First();
                result = new object[] { doc.Root };
            }
            else
            {
                var content = Convert(expandedPath, typeof(string[]), argumentFlags)
                                .Cast<string>();
                var typeDescriptorConverter = GetConverter<TypeDescriptorConverter>();
                //result = content.SelectMany(l => 
                //    typeDescriptorConverter.TryConvert(l, targetType, optionFlags).Result);
                result =
                    from line in content
                    let trimmed = line.Trim()
                    where !string.IsNullOrEmpty(trimmed)
                    let conversionResult = typeDescriptorConverter.TryConvert(trimmed, targetType, argumentFlags)
                    from resultItem in Unpack(conversionResult, trimmed, targetType)
                    select resultItem;
            }
            return result;
        }

        private IEnumerable<object> Unpack(TargetTypeConverterResult result, string value, TargetType targetType)
        {
            if (result.Success)
            {
                return result.Result;
            }
            else
            {
                var valueTruncated = value.Length > 10 ? (value.Substring(0, 10) + "...") : value;
                var msg = $"The value '{valueTruncated}' cannot be converted to {targetType}";
                throw new CommandLineParseException(msg);
            }
        }
    }
}
