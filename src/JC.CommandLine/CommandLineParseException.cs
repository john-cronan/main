using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine
{
    public class CommandLineParseException : Exception
    {
        private readonly IEnumerable<Exception> _errors;

        public CommandLineParseException(string message)
            : base(message)
        {
        }

        public CommandLineParseException(string message, 
            IEnumerable<Exception> errors)
            :base(message)
        {
            Guard.IsNotNullOrEmpty(errors, nameof(errors));

            _errors = errors.ToArray();
        }

        public IEnumerable<Exception> ParseErrors => _errors;
    }
}
