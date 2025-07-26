using System.Collections.Generic;

namespace JC.CommandLine.TargetTypeConverters
{
    internal class TargetTypeConverterResult
    {
        public static readonly TargetTypeConverterResult Unsucessful = new TargetTypeConverterResult(false, null);

        public static TargetTypeConverterResult FromResult(IEnumerable<object> result)
        {
            Guard.IsNotNull(result, nameof(result));

            return new TargetTypeConverterResult(true, result);
        }


        private readonly bool _success;
        private readonly IEnumerable<object> _result;

        private TargetTypeConverterResult(bool success, IEnumerable<object> result)
        {
            _success = success;
            _result = result;
        }

        public bool Success => _success;
        public IEnumerable<object> Result => _result;
    }
}
