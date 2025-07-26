using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine.TargetTypeConverters
{
    internal class BinaryValueConverter : TargetTypeConverter
    {
        public BinaryValueConverter(ITargetTypeConverterInstances otherConverters,
            IFilesystem filesystem)
            : base(otherConverters, filesystem)
        {
        }

        public override TargetTypeConverterResult TryConvert(string value, 
            TargetType targetType, ArgumentFlags argumentFlags)
        {
            Guard.IsNotNullOrWhitespace(value, nameof(value));
            Guard.IsNotNull(targetType, nameof(targetType));

            if (targetType.ScalarType == typeof(byte))
            {
                //
                //  Handling here is actually the same for a byte[] and a byte.
                var result = Convert(value, targetType, argumentFlags);
                return result;
            }
            else
            {
                return TargetTypeConverterResult.Unsucessful;
            }
        }

        private TargetTypeConverterResult Convert(string value, TargetType targetType,
            ArgumentFlags argumentFlags)
        {
            IEnumerable<byte> result = null;
            bool success = false;
            if (argumentFlags.HasFlag(ArgumentFlags.AssumeBase64))
            {
                success = AttemptBase64(value, out result);
            }
            else if (argumentFlags.HasFlag(ArgumentFlags.AssumeHexadecimal)
                || value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                success = AttemptHex(value, out result);
            }
            else
            {
                success = AttemptBase64(value, out result);
            }
            if (success)
            {
                var resultAsObjectArray = result.Cast<object>();
                return TargetTypeConverterResult.FromResult(resultAsObjectArray);
            }
            else
            {
                return TargetTypeConverterResult.Unsucessful;
            }
        }

        private bool AttemptHex(string value, out IEnumerable<byte> result)
        {
            try
            {
                result = Binary.FromHex(value).ToArray();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private bool AttemptBase64(string value, out IEnumerable<byte> result)
        {
            try
            {
                result = Binary.FromBase64(value).ToArray();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
