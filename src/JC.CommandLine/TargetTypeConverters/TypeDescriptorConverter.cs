using System.Collections.Generic;
using System.ComponentModel;

namespace JC.CommandLine.TargetTypeConverters
{
    internal class TypeDescriptorConverter : TargetTypeConverter
    {
        public TypeDescriptorConverter(ITargetTypeConverterInstances otherConverters,
            IFilesystem filesystem)
            : base(otherConverters, filesystem)
        {
        }

        public override TargetTypeConverterResult TryConvert(string value, TargetType targetType, 
            ArgumentFlags argumentFlags)
        {
            Guard.IsNotNullOrWhitespace(value, nameof(value));
            Guard.IsNotNull(targetType, nameof(targetType));

            IEnumerable<object> result = null;
            if (targetType.ScalarType == typeof(object) ||
                targetType.ScalarType == typeof(string))
            {
                result = new object[] { value };
                return TargetTypeConverterResult.FromResult(result);
            }
            else
            {
                try
                {
                    var converter = TypeDescriptor.GetConverter(targetType.ScalarType);
                    var convertedValue = converter.ConvertFromString(value);
                    result = new object[] { convertedValue };
                    return TargetTypeConverterResult.FromResult(result);
                }
                catch
                {
                    return TargetTypeConverterResult.Unsucessful;
                }
            }
        }
    }
}
