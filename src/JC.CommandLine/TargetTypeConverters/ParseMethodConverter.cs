using System;
using System.Linq;
using System.Reflection;

namespace JC.CommandLine.TargetTypeConverters
{
    internal class ParseMethodConverter : TargetTypeConverter
    {
        public ParseMethodConverter(ITargetTypeConverterInstances otherConverters,
            IFilesystem filesystem)
            : base(otherConverters, filesystem)
        {
        }

        public override TargetTypeConverterResult TryConvert(string value,
            TargetType targetType, ArgumentFlags argumentFlags)
        {
            Guard.IsNotNullOrWhitespace(value, nameof(value));
            Guard.IsNotNull(targetType, nameof(targetType));

            if (argumentFlags != ArgumentFlags.None)
            {
                return TargetTypeConverterResult.Unsucessful;
            }
            if (targetType.ScalarType == typeof(object) ||
                targetType.ScalarType == typeof(string))
            {
                return TargetTypeConverterResult.FromResult(new object[] { value });
            }
            else
            {
                var result = InvokeTryParse(value, targetType);
                return result.Success
                    ? result : InvokeParse(value, targetType);
            }
        }

        private static TargetTypeConverterResult InvokeTryParse(string value,
            TargetType targetType)
        {
            var matchingMethods =
                from method in targetType.Target.GetMethods(BindingFlags.InvokeMethod)
                where method.IsPublic && method.IsStatic
                && method.Name == "TryParse"
                && method.ReturnType == typeof(bool)
                let parameterTypes = method.GetParameters().Select(p => p.ParameterType)
                where parameterTypes.SequenceEqual(new Type[] { typeof(string), 
                    targetType.Target.MakeByRefType() })
                && method.GetParameters()[1].IsOut
                select method;
            var tryParseMethod = matchingMethods.FirstOrDefault();
            if (tryParseMethod == null)
            {
                return TargetTypeConverterResult.Unsucessful;
            }
            var p = new object[] { value, null };
            var tryParseReturn = tryParseMethod.Invoke(null, p);
            return (bool)tryParseReturn
                ? TargetTypeConverterResult.FromResult(new object[] { p[1] })
                : TargetTypeConverterResult.Unsucessful;
        }

        private static TargetTypeConverterResult InvokeParse(string value,
            TargetType targetType)
        {
            var matchingMethods =
                from method in targetType.Target.GetMethods(BindingFlags.InvokeMethod)
                where method.IsPublic && method.IsStatic
                && method.Name == "Parse"
                && method.ReturnType == targetType.Target
                where method.GetParameters().Length == 1
                && method.GetParameters()[0].ParameterType == typeof(string)
                select method;
            var parseMethod = matchingMethods.FirstOrDefault();
            if (parseMethod == null)
            {
                return TargetTypeConverterResult.Unsucessful;
            }
            try
            {
                var parseReturn = parseMethod.Invoke(null, new object[] { value });
                return TargetTypeConverterResult.FromResult(
                    new object[] { parseReturn });
            }
            catch
            {
                return TargetTypeConverterResult.Unsucessful;
            }
        }
    }
}
